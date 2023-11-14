using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using PacketLogViewer.Models;

namespace PacketLogViewer;

public partial class MainWindow
{
    public static Encoding Win1251 = null!;
    public readonly FileSystemWatcher ContentFileSystemWatcher;
    public readonly PacketCapture PacketCapture;
    public readonly FileSystemWatcher PingFileSystemWatcher;
    public readonly DispatcherTimer SphereTimeUpdateTimer;

    public MainWindow ()
    {
        InitializeComponent();
        PacketCapture = new PacketCapture();

        var defaultContent = new List<LogRecord>
        {
            new ("SRV", DateTime.Now,
                "72002C010018E4CAE6084063830C2E4C2EAC6D8E8B6BAEAC8C6C8E8B0B0000206C0E0485C646A6A626E62B2645" +
                "014006C645E6460120E60424C88908640C2D4CC565CCEC0C40C5A54D8C0C40C5850E8F0E802DADCC8DCEA50CAF0C80C7" +
                "0724068429A929890A2406008429A929890A240600")
        };

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Win1251 = Encoding.GetEncoding(1251);

        LoadContent();
        UpdateClientCoords();
        UpdateGameTime();

        // prewarm
        _ = SphObjectDb.GameObjectDataDb;

        LogList.ItemsSource = LogRecords.Any() ? LogRecords : defaultContent;
        LogList.ContextMenu = new ContextMenu();
        var menuItem = new MenuItem { Header = "Copy" };
        menuItem.Click += MenuItem_OnClick;
        LogList.ContextMenu.Items.Add(menuItem);

        LogList.SelectionChanged += OnLogListOnSelectionChanged;
        LogList.SelectedItem = LogList.Items[^1];
        LogList.ScrollIntoView(LogList.Items[^1]);

        ContentFileSystemWatcher = new FileSystemWatcher(@"c:\_sphereDumps\", "mixed");
        ContentFileSystemWatcher.Changed += (_, _) => { Dispatcher.BeginInvoke(LoadContent); };
        ContentFileSystemWatcher.EnableRaisingEvents = true;

        PingFileSystemWatcher = new FileSystemWatcher(@"c:\_sphereDumps\", "ping");
        PingFileSystemWatcher.Changed += (_, _) => { Dispatcher.BeginInvoke(UpdateClientCoords); };
        PingFileSystemWatcher.EnableRaisingEvents = true;

        LogList.KeyDown += (_, args) =>
        {
            if (args.KeyboardDevice.Modifiers != ModifierKeys.Control || args.Key != Key.C)
            {
                return;
            }

            CopySelectedRowContent();
        };

        var view = CollectionViewSource.GetDefaultView(LogList.ItemsSource);
        view.Filter = o =>
        {
            if (!ShowFavoritesOnly)
            {
                return true;
            }

            return (o as LogRecord)?.Favorite ?? true;
        };

        SphereTimeUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0 / 24)
        };
        SphereTimeUpdateTimer.Tick += (_, _) => UpdateGameTime();
        SphereTimeUpdateTimer.Start();
    }

    public BigInteger CurrentContent { get; set; }
    public int StartByteLine { get; set; }
    public int CurrentShift { get; set; }
    public ObservableCollection<LogRecord> LogRecords { get; } = new ();
    public int CurrentStreamPosition { get; set; }
    public bool ShowFavoritesOnly { get; set; }

    public void UpdateGameTime ()
    {
        var time = TimeHelper.GetCurrentSphereDateTime().AddYears(7800);
        GameTime.Text = time.ToString("dd/MM/yyyy HH:mm");
        GameTimeBits.Text = ByteArrayToBinaryString(TimeHelper.EncodeCurrentSphereDateTime(), false, true);
    }

    public void UpdateClientCoords ()
    {
        var retryCount = 0;
        while (retryCount < 10)
        {
            try
            {
                var textContent = File.ReadAllLines(@"c:\_sphereDumps\ping");
                var lastPing = textContent[^1].Split("\t", StringSplitOptions.RemoveEmptyEntries);
                if (lastPing.Length < 5)
                {
                    MessageBox.Show("Too few coords in the last ping");
                    return;
                }

                var x = double.Parse(lastPing[1]);
                var y = double.Parse(lastPing[2]);
                var z = double.Parse(lastPing[3]);
                var t = double.Parse(lastPing[4]);
                CoordsX.Text = $"{x:F4}";
                CoordsY.Text = $"{y:F4}";
                CoordsZ.Text = $"{z:F4}";
                CoordsT.Text = $"{t:F4}";

                var xBytes = EncodeServerCoordinate(x);
                var yBytes = EncodeServerCoordinate(y);
                var zBytes = EncodeServerCoordinate(z);
                var tBytes = EncodeServerCoordinate(t);

                CoordsXBits.Text = ByteArrayToBinaryString(xBytes, false, true);
                CoordsYBits.Text = ByteArrayToBinaryString(yBytes, false, true);
                CoordsZBits.Text = ByteArrayToBinaryString(zBytes, false, true);
                CoordsTBits.Text = ByteArrayToBinaryString(tBytes, false, true);
                break;
            }
            catch (IOException ex)
            {
                retryCount++;
                Thread.Sleep(10);
                if (retryCount >= 10)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        LogList.UpdateLayout();
    }

    public void LoadContent ()
    {
        var retryCount = 0;
        while (retryCount < 10)
        {
            try
            {
                var textContent = File.ReadAllLines(@"c:\_sphereDumps\mixed")
                    .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                for (var i = CurrentStreamPosition; i < textContent.Count; i++)
                {
                    var logEntry = textContent[i];
                    var cleanedUpText = logEntry.Replace("=", "").Replace("-", "");
                    if (string.IsNullOrWhiteSpace(cleanedUpText))
                    {
                        continue;
                    }

                    var split = cleanedUpText.Split('\t',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    try
                    {
                        LogRecords.Add(new LogRecord(split.Length > 2 ? split[0] : "---",
                            split.Length > 1 ? DateTime.Parse(split[1]) : DateTime.MinValue, split[^1]));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(logEntry + "\n" + ex);
                    }
                }

                var data7600 = textContent.LastOrDefault(x => x[27..].StartsWith("76002C0100"));
                var data3200 = textContent.LastOrDefault(x => x[27..].StartsWith("32002C0100"));
                DateTime? time7600 = data7600 is null ? null : DateTime.Parse(data7600[6..24]);
                DateTime? time3200 = data3200 is null ? null : DateTime.Parse(data3200[6..24]);
                var lastClientIdRecord = data7600 ?? data3200;
                if (time3200 > time7600)
                {
                    lastClientIdRecord = data3200;
                }

                var clientId = lastClientIdRecord is null
                    ? "0000"
                    : lastClientIdRecord[43..45] + lastClientIdRecord[41..43];
                ClientId.Text = clientId;
                var clientIdBytes = Convert.FromHexString(clientId);
                ClientIdBits.Text = ByteArrayToBinaryString(clientIdBytes, false, true);

                CurrentStreamPosition = textContent.Count;
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                Thread.Sleep(10);
                if (retryCount >= 10)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        LogList.UpdateLayout();
    }

    public void UpdateContentPreview (LogRecord selected)
    {
        try
        {
            var bytes = Convert.FromHexString(selected.Content);
            var sphObjects = ObjectPacketTools.GetObjectsFromPacket(bytes);
            if (sphObjects.Count > 0)
            {
                ContentPreview.Text = ObjectPacketTools.GetTextOutput(sphObjects, true);
            }
            else
            {
                ContentPreview.Text = "";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            ContentPreview.Text = "Not an item packet";
        }
    }

    private void OnLogListOnSelectionChanged (object sender, SelectionChangedEventArgs args)
    {
        try
        {
            var selected = args.AddedItems[0] as LogRecord;
            var positiveValue = "0" + selected?.Content ?? throw new InvalidOperationException();
            CurrentContent =
                new BigInteger(BigInteger.Parse(positiveValue, NumberStyles.HexNumber).ToByteArray().AsSpan(), true,
                    true);
            CurrentShift = 0;

            TrySetCurrentTextContent();
            IsFavorite.IsChecked = selected?.Favorite ?? false;
            LogList.ScrollIntoView(selected);
            UpdateContentPreview(selected);
        }
        catch
        {
            LogRecordTextDisplay.Text = "Selected value is not a hex string!";
            IsFavorite.IsChecked = false;
        }
    }

    private void TrySetCurrentTextContent ()
    {
        try
        {
            LogRecordTextDisplay.Text = ToReadableBinaryString();
        }
        catch
        {
            LogRecordTextDisplay.Text = "Selected value is not a hex string!";
        }
    }

    public static string GetBinaryPaddedString (byte b)
    {
        return Convert.ToString(b, 2).PadLeft(8, '0');
    }

    // 0xAD is a soft hyphen and doesn't render, but it's not a control/whitespace/separator
    public static char GetVisibleChar (char c)
    {
        return (c >= 0x20 && c <= 0x7E) || (c >= 'А' && c <= 'я') ? c : '·';
    }
    // char.IsControl(c) || char.IsWhiteSpace(c) || char.IsSeparator(c) || c == 0xAD ? '·' : c;

    public static char GetEncoded1251Char (byte b)
    {
        var char1251 = Win1251.GetChars(new[] { b })[0];
        return GetVisibleChar(char1251);
    }

    public static char GetEncodedLoginChar (byte b)
    {
        var loginChar = b % 2 == 0 ? (char) (b / 4 - 1 + 'A') : (char) (b / 4 - 48 + '0');
        return GetVisibleChar(loginChar);
    }

    public static string GetFormattedBinaryOutput (byte b)
    {
        return $"{GetBinaryPaddedString(b),9}{b.ToString(),7}{b,7:X}h" +
               $"{GetEncoded1251Char(b).ToString(),7}{GetEncodedLoginChar(b).ToString(),7}";
    }

    public string ToReadableBinaryString ()
    {
        var shiftedBigInt = CurrentShift switch
        {
            0 => CurrentContent,
            < 0 => CurrentContent << -CurrentShift,
            _ => CurrentContent >> CurrentShift
        };
        var shiftedValue = shiftedBigInt.ToByteArray();

        var shiftedValueBytes = new List<byte[]>();
        for (var i = 0; i <= 7; i++)
        {
            shiftedValueBytes.Add((CurrentContent >> i).ToByteArray());
        }

        var sb = new StringBuilder();
        var sbText = new StringBuilder();
        var sb1251 = new StringBuilder();
        var sbLogin = new StringBuilder();
        sbText.AppendLine("-------------------------------------------------");
        sbText.AppendLine("  #   Binary      Dec\tHex\t1251\tLogin");
        sbText.AppendLine("-------------------------------------------------");

        for (var i = 0; i <= 7; i++)
        {
            sb1251.Append($"[{i}] ");
            for (var j = StartByteLine; j < shiftedValueBytes[i].Length; j++)
            {
                sb1251.Append(GetEncoded1251Char(shiftedValueBytes[i][j]));
            }

            sb1251.AppendLine();
        }

        for (var i = StartByteLine; i < shiftedValue.Length; i++)
        {
            sbLogin.Append(GetEncodedLoginChar(shiftedValue[i]));
            sbText.Append($"{i + 1,3:D}: ");
            sbText.AppendLine(GetFormattedBinaryOutput(shiftedValue[i]));
        }

        sb.AppendLine(Convert.ToHexString(shiftedValue));
        sb.Append(sb1251);
        sb.AppendLine(sbLogin.ToString());
        sb.Append(sbText);
        return sb.ToString();
    }

    private void Shift_OnClick (object sender, RoutedEventArgs e)
    {
        var button = e.Source as Button;
        var shift = 0;
        switch (button!.Name)
        {
            case "ShiftLeft1":
            case "ShiftLeft2":
            case "ShiftLeft3":
            case "ShiftLeft4":
            case "ShiftLeft5":
            case "ShiftLeft6":
            case "ShiftLeft7":
                shift = -(button.Name[^1] - '0');
                break;
            case "ShiftRight1":
            case "ShiftRight2":
            case "ShiftRight3":
            case "ShiftRight4":
            case "ShiftRight5":
            case "ShiftRight6":
            case "ShiftRight7":
                shift = button.Name[^1] - '0';
                break;
        }

        CurrentShift = shift;
        TrySetCurrentTextContent();
    }

    private void StartByte_OnTextChanged (object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(StartByte.Text, out var startByte))
        {
            StartByteLine = startByte - 1;
            TrySetCurrentTextContent();
        }
        else if (string.IsNullOrWhiteSpace(StartByte.Text))
        {
            StartByteLine = 0;
            TrySetCurrentTextContent();
        }
    }

    private void MenuItem_OnClick (object sender, RoutedEventArgs e)
    {
        CopySelectedRowContent();
    }

    private void CopySelectedRowContent ()
    {
        var selectedRow = (LogRecord) LogList.SelectedItem;
        var text = $"{selectedRow.Origin}\t\t\t{selectedRow.Date}\t\t\t{selectedRow.Content}\n";
        Clipboard.SetText(text);
    }

    private void FavoriteToggleButton_OnChecked (object sender, RoutedEventArgs e)
    {
        if (LogList.SelectedItem is null)
        {
            return;
        }

        var item = (LogRecord) LogList.SelectedItem;
        item.Favorite = true;
    }

    private void FavoriteToggleButton_OnUnchecked (object sender, RoutedEventArgs e)
    {
        if (LogList.SelectedItem is null)
        {
            return;
        }

        var item = (LogRecord) LogList.SelectedItem;
        item.Favorite = false;
    }

    private void ShowFavoritesOnlyToggleButton_OnChecked (object sender, RoutedEventArgs e)
    {
        ShowFavoritesOnly = true;
        CollectionViewSource.GetDefaultView(LogList.ItemsSource).Refresh();
        if (LogList.Items.Count > 0)
        {
            LogList.SelectedItem = LogList.Items[^1];
            LogList.ScrollIntoView(LogList.Items[^1]);
        }
    }

    private void ShowFavoritesOnlyToggleButton_OnUnchecked (object sender, RoutedEventArgs e)
    {
        ShowFavoritesOnly = false;
        CollectionViewSource.GetDefaultView(LogList.ItemsSource).Refresh();
        if (LogList.Items.Count > 0)
        {
            LogList.SelectedItem = LogList.Items[^1];
            LogList.ScrollIntoView(LogList.Items[^1]);
        }
    }

    // copypaste, remove it
    public static byte[] EncodeServerCoordinate (double a)
    {
        var scale = 69;

        var a_abs = Math.Abs(a);
        var a_temp = a_abs;

        var steps = 0;

        if ((int) a_abs == 0)
        {
            scale = 58;
        }

        else if (a_temp < 2048)
        {
            while (a_temp < 2048)
            {
                a_temp *= 2;
                steps += 1;
            }

            scale -= (steps + 1) / 2;

            if (scale < 0)
            {
                scale = 58;
            }
        }
        else
        {
            while (a_temp > 4096)
            {
                a_temp /= 2;
                steps += 1;
            }

            scale += steps / 2;
        }

        var a_3 = (byte) (((a < 0 ? 1 : 0) << 7) + scale);
        var mul = Math.Pow(2, (int) Math.Log(a_abs, 2));
        var numToEncode = (int) (0b100000000000000000000000 * (a_abs / mul + 1));

        var a_2 = (byte) (((numToEncode & 0b111111110000000000000000) >> 16) + (steps % 2 == 1 ? 0b10000000 : 0));
        var a_1 = (byte) ((numToEncode & 0b1111111100000000) >> 8);
        var a_0 = (byte) (numToEncode & 0b11111111);

        return new[] { a_0, a_1, a_2, a_3 };
    }

    // copypaste, remove
    public static string ByteArrayToBinaryString (byte[] ba, bool noPadding = false, bool addSpaces = false)
    {
        var hex = new StringBuilder(ba.Length * 2);

        foreach (var val in ba)
        {
            var str = Convert.ToString(val, 2);
            if (!noPadding)
            {
                str = str.PadLeft(8, '0');
            }

            hex.Append(str);

            if (addSpaces)
            {
                hex.Append(' ');
            }
        }

        return hex.ToString();
    }
}

public static class TimeHelper
{
    public static readonly DateTime RealtimeOrigin = new (1998, 8, 21, 10, 00, 6);

    public static DateTime GetCurrentSphereDateTime ()
    {
        var sphereTimeOffset = (DateTime.Now - RealtimeOrigin).TotalSeconds * 12;
        var sphereDateTime = new DateTime().AddSeconds(sphereTimeOffset);

        return sphereDateTime;
    }

    // copypaste, remove
    public static byte[] EncodeCurrentSphereDateTime ()
    {
        var currentSphereTime = GetCurrentSphereDateTime();
        var seconds = currentSphereTime.Second / 12;
        var minutes_last4 = (byte) ((currentSphereTime.Minute & 0b1111) << 4);
        // 1-4 minutes 5-8 seconds 
        var firstDateByte = (byte) (minutes_last4 + (seconds & 0b1111));
        var minutes_first2 = (byte) ((currentSphereTime.Minute & 0b110000) >> 4);
        var hours = (byte) (currentSphereTime.Hour << 2);
        var days_last1 = (byte) ((currentSphereTime.Day % 2) << 7);
        // 1 days 2-6 hours 7-8 minutes
        var secondDateByte = (byte) (days_last1 + hours + minutes_first2);
        var days_first4 = (byte) ((currentSphereTime.Day & 0b11110) >> 1);
        var month = (byte) (currentSphereTime.Month << 4);
        // 1-4 months 5-8 days
        var thirdDateByte = (byte) (month + days_first4);
        var years_last8 = (byte) (currentSphereTime.Year & 0b11111111);
        var years_first2 = (byte) ((currentSphereTime.Year & 0b1100000000) >> 8);
        var fourthDateByte = (byte) (0b00110100 + years_first2);

        return new[]
        {
            firstDateByte, secondDateByte, thirdDateByte, years_last8, fourthDateByte
        };
    }
}