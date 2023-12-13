using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using LiteDB;
using PacketLogViewer.Models;
using SphServer.Helpers;

namespace PacketLogViewer;

public partial class MainWindow
{
    private const string KaitaiPath = @"c:\source\SphereKaitaiPackets\";
    private static Encoding Win1251 = null!;

    public static readonly LiteDatabase PacketDatabase =
        new (@"Filename=C:\_sphereStuff\sph_packets.db;Connection=shared;");

    public static readonly ILiteCollection<StoredPacket> PacketCollection =
        PacketDatabase.GetCollection<StoredPacket>("Packets");

    public readonly Dictionary<string, string> KaitaiDefinitions = new ();

    public readonly PacketCapture PacketCapture;
    public readonly DispatcherTimer SphereTimeUpdateTimer;
    private readonly string TempFileKaitaiBytes = Path.GetTempFileName();

    public MainWindow ()
    {
        InitializeComponent();
        LoadContent();
        PacketCapture = new PacketCapture
        {
            OnPacketProcessed = OnPacketProcessed
        };

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Win1251 = Encoding.GetEncoding(1251);

        // UpdateClientCoords();
        UpdateGameTime();

        // prewarm
        _ = SphObjectDb.GameObjectDataDb;

        LogList.ItemsSource = LogRecords;
        LogList.ContextMenu = new ContextMenu();
        var menuItem = new MenuItem { Header = "Copy" };
        menuItem.Click += MenuItem_OnClick;
        LogList.ContextMenu.Items.Add(menuItem);

        LogList.SelectionChanged += OnLogListOnSelectionChanged;

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
            if (ShowFavoritesOnly)
            {
                return (o as LogRecord)?.Favorite ?? false;
            }

            if (!HideUninteresting)
            {
                return true;
            }

            return !((o as LogRecord)?.HiddenByDefault ?? false);
        };

        SphereTimeUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0 / 24)
        };
        SphereTimeUpdateTimer.Tick += (_, _) => UpdateGameTime();
        SphereTimeUpdateTimer.Start();

        ScrollIntoViewIfSelectionExists();
        Loaded += OnLoaded;
        LoadKaitaiDefinitions();

        KaitaiScriptText.KeyDown += (_, args) =>
        {
            if (args.KeyboardDevice.Modifiers != ModifierKeys.Control || args.Key != Key.S)
            {
                return;
            }

            var selectedItem = KaitaiDefitionsTreeView.SelectedItem;
            if (selectedItem is null)
            {
                return;
            }

            SaveCurrentKaitaiDefinition(selectedItem);
        };
    }

    public BigInteger CurrentContent { get; set; }
    public int StartByteLine { get; set; }
    public int CurrentShift { get; set; }
    public ObservableCollection<LogRecord> LogRecords { get; } = new ();
    public bool ShowFavoritesOnly { get; set; }
    public bool HideUninteresting { get; set; } = true;

    private void OnLoaded (object o, RoutedEventArgs routedEventArgs)
    {
        using var yamlDefinitionStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("PacketLogViewer.AvalonEdit.YAML-Mode.xshd")!;
        using var reader = new XmlTextReader(yamlDefinitionStream);
        KaitaiScriptText.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    private void OnPacketProcessed (StoredPacket storedPacket)
    {
        storedPacket.Id = PacketCollection.Insert(storedPacket);
        Dispatcher.Invoke(() =>
        {
            LogRecords.Add(new LogRecord(storedPacket));
            LogList.UpdateLayout();
        });
    }

    public void UpdateGameTime ()
    {
        var time = TimeHelper.GetCurrentSphereDateTime().AddYears(7800);
        GameTime.Text = time.ToString("dd/MM/yyyy HH:mm");
        // TODO
        GameTimeBits.Text = "0";
    }

    // public void UpdateClientCoords ()
    // {
    //     var retryCount = 0;
    //     while (retryCount < 10)
    //     {
    //         try
    //         {
    //             var textContent = File.ReadAllLines(@"c:\_sphereDumps\ping");
    //             var lastPing = textContent[^1].Split("\t", StringSplitOptions.RemoveEmptyEntries);
    //             if (lastPing.Length < 5)
    //             {
    //                 MessageBox.Show("Too few coords in the last ping");
    //                 return;
    //             }
    //
    //             var x = double.Parse(lastPing[1]);
    //             var y = double.Parse(lastPing[2]);
    //             var z = double.Parse(lastPing[3]);
    //             var t = double.Parse(lastPing[4]);
    //             CoordsX.Text = $"{x:F4}";
    //             CoordsY.Text = $"{y:F4}";
    //             CoordsZ.Text = $"{z:F4}";
    //             CoordsT.Text = $"{t:F4}";
    //
    //             var xBytes = EncodeServerCoordinate(x);
    //             var yBytes = EncodeServerCoordinate(y);
    //             var zBytes = EncodeServerCoordinate(z);
    //             var tBytes = EncodeServerCoordinate(t);
    //
    //             CoordsXBits.Text = ByteArrayToBinaryString(xBytes, false, true);
    //             CoordsYBits.Text = ByteArrayToBinaryString(yBytes, false, true);
    //             CoordsZBits.Text = ByteArrayToBinaryString(zBytes, false, true);
    //             CoordsTBits.Text = ByteArrayToBinaryString(tBytes, false, true);
    //             break;
    //         }
    //         catch (IOException ex)
    //         {
    //             retryCount++;
    //             Thread.Sleep(10);
    //             if (retryCount >= 10)
    //             {
    //                 MessageBox.Show(ex.ToString());
    //             }
    //         }
    //     }
    //
    //     LogList.UpdateLayout();
    // }

    public void LoadContent ()
    {
        PacketCollection.EnsureIndex(x => x.Source);
        // might not be great
        PacketCollection.EnsureIndex(x => x.Timestamp);

        var packetsToLoad = PacketCollection.Query().OrderByDescending(x => x.Timestamp).Limit(1000).ToList();
        if (packetsToLoad is null)
        {
            MessageBox.Show("Packets to load are null");
            return;
        }

        if (!packetsToLoad.Any())
        {
            MessageBox.Show("No packets to load");
            return;
        }

        packetsToLoad.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

        packetsToLoad.ForEach(x => LogRecords.Add(new LogRecord(x)));

        LogList.UpdateLayout();
    }

    public void UpdateContentPreview (LogRecord selected)
    {
        try
        {
            var bytes = selected.ContentBytes;
            var sphObjects = ObjectPacketTools.GetObjectsFromPacket(bytes);
            ContentPreview.Text = sphObjects.Count > 0 ? ObjectPacketTools.GetTextOutput(sphObjects, true) : "";
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
            if (args.AddedItems.Count < 1)
            {
                return;
            }

            var selected = args.AddedItems[0] as LogRecord;
            CurrentContent = new BigInteger(selected.ContentBytes, true);
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
        return (c >= 0x20 && c <= 0x7E) || c is >= 'А' and <= 'я' ? c : '·';
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
        var text =
            $"{selectedRow.Source}\t\t\t{selectedRow.Timestamp}\t\t\t{Convert.ToHexString(selectedRow.ContentBytes)}\n";
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
        if (ShowFavoritesOnly)
        {
            return;
        }

        ShowFavoritesOnly = true;
        ScrollIntoViewIfSelectionExists();
    }

    private void ShowFavoritesOnlyToggleButton_OnUnchecked (object sender, RoutedEventArgs e)
    {
        ShowFavoritesOnly = false;
        ScrollIntoViewIfSelectionExists();
    }

    private void HideUninteresting_OnChecked (object sender, RoutedEventArgs e)
    {
        if (HideUninteresting)
        {
            return;
        }

        HideUninteresting = true;
        ScrollIntoViewIfSelectionExists();
    }

    private void HideUninteresting_OnUnchecked (object sender, RoutedEventArgs e)
    {
        HideUninteresting = false;
        ScrollIntoViewIfSelectionExists();
    }

    private void ScrollIntoViewIfSelectionExists ()
    {
        CollectionViewSource.GetDefaultView(LogList.ItemsSource).Refresh();
        if (LogList.Items.Count < 1)
        {
            return;
        }

        var selected = LogList.SelectedItem ?? LogList.Items[^1];
        if (!LogList.Items.PassesFilter(selected))
        {
            // should only happen when switching to a more restricted view with filtered out item selected
            selected = LogList.Items[^1];
        }

        LogList.SelectedItem = selected;

        LogList.ScrollIntoView(selected);
    }

    private void KaitaiCompile_OnClick (object sender, RoutedEventArgs e)
    {
        KaitaiCompile();
    }

    public void KaitaiCompile ()
    {
        var selectedItem = KaitaiDefitionsTreeView.SelectedItem;
        SaveCurrentKaitaiDefinition(selectedItem);
        var selectedKaitai = (string) ((TreeViewItem) selectedItem).Header;
        var currentPacket = (LogRecord) LogList.SelectedItem;
        File.WriteAllBytes(TempFileKaitaiBytes, currentPacket.ContentBytes);
        var ksdump = new Process();
        var kaitaiPath = GetKaitaiPath(selectedKaitai);
        ksdump.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        ksdump.StartInfo.FileName = @"c:\Ruby\bin\ksdump.bat";
        ksdump.StartInfo.Arguments = $@"{TempFileKaitaiBytes} {kaitaiPath}";
        ksdump.StartInfo.CreateNoWindow = true;
        ksdump.StartInfo.RedirectStandardOutput = true;
        Dispatcher.BeginInvoke(async () =>
        {
            ksdump.Start();
            await ksdump.WaitForExitAsync();
            var result = await ksdump.StandardOutput.ReadToEndAsync();
            Console.WriteLine(result);
            ParseKaitaiCompiledOutput(result);
        });
    }

    public void ParseKaitaiCompiledOutput (string input)
    {
        // we'll break it a bit for readability
        try
        {
            var lines = input.ReplaceLineEndings("\n")
                .Split("\n", StringSplitOptions.RemoveEmptyEntries);

            var formattedOutput = new StringBuilder();
            var kaitaiContent = KaitaiScriptText.Document.Text;
            var kaitaiOrder = kaitaiContent.ReplaceLineEndings("\n")
                .Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.StartsWith("- id")).Select(x => x[6..]).ToList();
            var valueMapping = new Dictionary<string, object>();
            foreach (var line in lines)
            {
                var valueSeparator = line.IndexOf(':');

                if (valueSeparator < 0)
                {
                    formattedOutput.AppendLine(line);
                    continue;
                }

                var propertyName = line[..valueSeparator];
                var propertyValue = valueSeparator + 2 > line.Length ? "" : line[(valueSeparator + 2)..];
                valueMapping.Add(propertyName, propertyValue);
                // we won't support nested types for now

                // if (string.IsNullOrWhiteSpace(propertyValue))
                // {
                //     // should only happen for nested types
                //     var trimmedName = propertyName.Trim();
                //     var indexOfPropertyId = kaitaiContent.IndexOf($"- id: {trimmedName}");
                //     var propertyIdSubstring = kaitaiContent[indexOfPropertyId..];
                //     var indexOfPropertyType = propertyIdSubstring.IndexOf("type: ") + 6;
                //     var propertyTypeSubstring = propertyIdSubstring[indexOfPropertyType..];
                //     var indexOfPropertyEnd = propertyTypeSubstring.IndexOf("\n");
                //     var propertyType = propertyTypeSubstring[..indexOfPropertyEnd];
                //     propertyName = $"{SnakeCaseToCamelCase(propertyName)} [{SnakeCaseToCamelCase(propertyType)}]";
                // }
            }

            foreach (var orderedPropertyName in kaitaiOrder)
            {
                var name = SnakeCaseToCamelCase(orderedPropertyName);
                if (!valueMapping.ContainsKey(orderedPropertyName))
                {
                    formattedOutput.AppendLine($"{name}: null");
                    continue;
                }

                var value = (string) valueMapping[orderedPropertyName];
                var formattedValue = value;
                // assuming it's at max a long
                if (long.TryParse(value, out var longValue))
                {
                    if (longValue >= 0)
                    {
                        var hexValue = $"{longValue:X}";
                        if (hexValue.Length % 2 == 1)
                        {
                            hexValue = hexValue.PadLeft(hexValue.Length + 1, '0');
                        }

                        formattedValue = $"0x{hexValue} = {value}";
                    }
                }

                formattedOutput.AppendLine($"{name} = {formattedValue}");
            }

            KaitaiScriptJsonOutputText.Document.Text = formattedOutput.ToString();
        }
        catch
        {
            KaitaiScriptJsonOutputText.Document.Text = "";
        }
    }

    public string SnakeCaseToCamelCase (string input)
    {
        return input
            .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }

    public void LoadKaitaiDefinitions ()
    {
        var kaitaiFiles = Directory.EnumerateFiles(KaitaiPath, "*.ksy");
        foreach (var definitionFile in kaitaiFiles)
        {
            var contents = File.ReadAllText(definitionFile);
            var name = Path.GetFileNameWithoutExtension(definitionFile);
            KaitaiDefinitions.Add(name, contents);
            CreateKaitaiDefinitionItem(name);
        }

        KaitaiDefitionsTreeView.SelectedItemChanged += (_, args) =>
        {
            if (args.NewValue == args.OldValue)
            {
                return;
            }

            if (args.NewValue is null)
            {
                KaitaiScriptText.Document.Text = "";
                return;
            }

            var header = (string) (args.NewValue as TreeViewItem).Header;
            if (!KaitaiDefinitions.ContainsKey(header))
            {
                KaitaiScriptText.Document.Text = "";
                return;
            }

            var kaitaiScript = KaitaiDefinitions[header];
            KaitaiScriptText.Document.Text = kaitaiScript;
            KaitaiScriptJsonOutputText.Document.Text = "";
        };

        KaitaiDefitionsTreeView.ContextMenu = new ContextMenu();
        var createItem = new MenuItem
        {
            Header = "Create new definition"
        };
        createItem.Click += (_, _) =>
        {
            var dialog = new CreateKaitaiDialog();
            var name = "";
            if (dialog.ShowDialog() == true)
            {
                name = dialog.Name;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please input name to create a new definition");
                return;
            }

            CreateKaitaiDefinitionItem(name, true);
            KaitaiDefitionsTreeView.UpdateLayout();
        };

        KaitaiDefitionsTreeView.ContextMenu.Items.Add(createItem);
        SelectFirstDefinitionItem();
    }

    public void SelectFirstDefinitionItem ()
    {
        var firstItem = (TreeViewItem) KaitaiDefitionsTreeView.Items.GetItemAt(0);
        firstItem.IsSelected = true;
        KaitaiScriptJsonOutputText.Document.Text = "";
    }

    public void CreateKaitaiDefinitionItem (string header, bool isSelected = false)
    {
        var item = new TreeViewItem
        {
            Header = header,
            IsExpanded = true,
            IsSelected = isSelected,
            ContextMenu = new ContextMenu()
        };

        var deleteMenuItem = new MenuItem
        {
            Header = "Delete definition"
        };
        deleteMenuItem.Click += (_, _) =>
        {
            var confirmDialogResult = MessageBox.Show(
                $"Do you really want to delete definition \"{header}\"?",
                "Confirm deletion?",
                MessageBoxButton.YesNo);
            if (confirmDialogResult == MessageBoxResult.Yes)
            {
                var path = GetKaitaiPath(header);
                File.Delete(path);
                KaitaiDefitionsTreeView.Items.Remove(item);
                SelectFirstDefinitionItem();
            }
        };
        item.ContextMenu.Items.Add(deleteMenuItem);
        KaitaiDefitionsTreeView.Items.Add(item);
        KaitaiScriptJsonOutputText.Document.Text = "";
    }

    public string GetKaitaiPath (string name)
    {
        return Path.Combine(KaitaiPath, name + ".ksy");
    }

    public void SaveCurrentKaitaiDefinition (object selectedItem)
    {
        var selectedScriptItem = selectedItem as TreeViewItem;
        var contents = KaitaiScriptText.Document.Text;
        var header = (string) selectedScriptItem.Header;
        var outputFile = GetKaitaiPath(header);
        File.WriteAllText(outputFile, contents);
        KaitaiDefinitions[header] = contents;
    }
}