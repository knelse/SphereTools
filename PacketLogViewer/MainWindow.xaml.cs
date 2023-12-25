using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using LiteDB;
using PacketLogViewer.Models;
using SimpleKaitaiParser;
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

    public static readonly ILiteCollection<StoredPacket> SplittedPacketCollection =
        PacketDatabase.GetCollection<StoredPacket>("SplittedPackets");

    public readonly Dictionary<string, string> KaitaiDefinitions = new ();
    private readonly SimpleKaitaiParser.SimpleKaitaiParser KaitaiParser = new ();

    public readonly PacketCapture PacketCapture;
    public readonly DispatcherTimer SphereTimeUpdateTimer;

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

        UpdateGameTime();

        // prewarm
        _ = SphObjectDb.GameObjectDataDb;

        LogListFullPackets.ItemsSource = LogRecords;
        LogListSplitPackets.ItemsSource = LogRecordsSplitted;
        LogListFullPackets.ContextMenu = new ContextMenu();
        var menuItem = new MenuItem { Header = "Copy" };
        menuItem.Click += FullPacketsLog_MenuItem_OnClick;
        LogListFullPackets.ContextMenu.Items.Add(menuItem);
        LogListSplitPackets.ContextMenu = new ContextMenu();
        var menuItem1 = new MenuItem { Header = "Copy" };
        menuItem1.Click += SplitPacketsLog_MenuItem_OnClick;
        LogListSplitPackets.ContextMenu.Items.Add(menuItem1);

        LogListFullPackets.SelectionChanged += OnLogListOnSelectionChanged;
        LogListSplitPackets.SelectionChanged += OnLogListOnSelectionChanged;

        LogListFullPackets.KeyDown += (_, args) =>
        {
            if (args.KeyboardDevice.Modifiers != ModifierKeys.Control || args.Key != Key.C)
            {
                return;
            }

            CopySelectedRowContent(LogListFullPackets);
        };

        LogListSplitPackets.KeyDown += (_, args) =>
        {
            if (args.KeyboardDevice.Modifiers != ModifierKeys.Control || args.Key != Key.C)
            {
                return;
            }

            CopySelectedRowContent(LogListSplitPackets);
        };

        var fullPacketView = CollectionViewSource.GetDefaultView(LogListFullPackets.ItemsSource);
        var splitPacketView = CollectionViewSource.GetDefaultView(LogListSplitPackets.ItemsSource);
        var filterFunc = new Predicate<object>(o =>
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
        });
        fullPacketView.Filter = filterFunc;
        splitPacketView.Filter = filterFunc;

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
            KaitaiCompile();
        };
    }

    public BigInteger CurrentContent { get; set; }
    public byte[] CurrentContentBytes { get; set; }
    public int StartByteLine { get; set; }
    public int CurrentShift { get; set; }
    public ObservableCollection<LogRecord> LogRecords { get; } = new ();
    public ObservableCollection<LogRecord> LogRecordsSplitted { get; } = new ();
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
        var splitBytes = storedPacket.Source == PacketSource.CLIENT
            ? new List<byte[]>()
            : PacketAnalyzer.SplitPacketIntoParts(storedPacket);
        var splitPackets = splitBytes.Select(x => new StoredPacket
        {
            ContentBytes = x,
            Favorite = false,
            Timestamp = storedPacket.Timestamp,
            ContentJson = "",
            Source = storedPacket.Source,
            HiddenByDefault = storedPacket.HiddenByDefault
        }).ToList();
        SplittedPacketCollection.InsertBulk(splitPackets);


        Dispatcher.Invoke(() =>
        {
            LogRecords.Add(new LogRecord(storedPacket));
            splitPackets.ForEach(x =>
            {
                var newRecord = new LogRecord(x)
                {
                    HiddenByDefault = storedPacket.HiddenByDefault
                };
                LogRecordsSplitted.Add(newRecord);
            });
            if (PacketAnalyzer.IsClientPingPacket(storedPacket))
            {
                UpdateClientCoordsAndId(storedPacket);
            }

            LogListFullPackets.UpdateLayout();
            LogListSplitPackets.UpdateLayout();
        });
    }

    private ListView GetActivePacketLog ()
    {
        if (LogListTabControl.SelectedItem is null)
        {
            return LogListFullPackets;
        }

        var selectedLog = (string) (LogListTabControl.SelectedItem as TabItem).Header;
        return selectedLog == "Full Packets" ? LogListFullPackets : LogListSplitPackets;
    }

    public void UpdateGameTime ()
    {
        var time = TimeHelper.GetCurrentSphereDateTime().AddYears(7800);
        GameTime.Text = time.ToString("dd/MM/yyyy HH:mm");
        // TODO
        GameTimeBits.Text = "0";
    }

    public void UpdateClientCoordsAndId (StoredPacket storedPacket)
    {
        try
        {
            var coords = CoordsHelper.GetCoordsFromPingBytes(storedPacket.ContentBytes);
            CoordsX.Text = $"{coords.x:F4}";
            CoordsY.Text = $"{coords.y:F4}";
            CoordsZ.Text = $"{coords.z:F4}";
            CoordsT.Text = $"{coords.turn:F4}";

            var xBytes = CoordsHelper.EncodeServerCoordinate(coords.x);
            var yBytes = CoordsHelper.EncodeServerCoordinate(coords.y);
            var zBytes = CoordsHelper.EncodeServerCoordinate(coords.z);
            var tBytes = CoordsHelper.EncodeServerCoordinate(coords.turn);

            CoordsXBits.Text = StringConvertHelpers.ByteArrayToBinaryString(xBytes, false, true);
            CoordsYBits.Text = StringConvertHelpers.ByteArrayToBinaryString(yBytes, false, true);
            CoordsZBits.Text = StringConvertHelpers.ByteArrayToBinaryString(zBytes, false, true);
            CoordsTBits.Text = StringConvertHelpers.ByteArrayToBinaryString(tBytes, false, true);

            var id = (storedPacket.ContentBytes[16] >> 5) + (storedPacket.ContentBytes[17] << 3) +
                     ((storedPacket.ContentBytes[18] & 0b11111) << 11);
            ClientId.Text = $"{id:X4}";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void LoadContent ()
    {
        PacketCollection.EnsureIndex(x => x.Source);
        // might not be great
        PacketCollection.EnsureIndex(x => x.Timestamp);
        SplittedPacketCollection.EnsureIndex(x => x.Source);
        // might not be great
        SplittedPacketCollection.EnsureIndex(x => x.Timestamp);

        var fullPacketsToLoad = PacketCollection.Query().OrderByDescending(x => x.Timestamp)
            .Limit(1000).ToList();

        var splitPacketsToLoad = SplittedPacketCollection.Query().OrderByDescending(x => x.Timestamp)
            .Limit(1000).ToList();
        if (fullPacketsToLoad is null)
        {
            MessageBox.Show("Packets to load are null");
            return;
        }

        if (!fullPacketsToLoad.Any())
        {
            MessageBox.Show("No packets to load");
            return;
        }

        fullPacketsToLoad.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        splitPacketsToLoad.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

        fullPacketsToLoad.ForEach(x => LogRecords.Add(new LogRecord(x)));
        splitPacketsToLoad.ForEach(x => LogRecordsSplitted.Add(new LogRecord(x)));

        LogListFullPackets.UpdateLayout();
        LogListSplitPackets.UpdateLayout();
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
            CurrentContentBytes = selected.ContentBytes;
            CurrentContent = new BigInteger(selected.ContentBytes, true);
            CurrentShift = 0;

            TrySetCurrentTextContent();
            IsFavorite.IsChecked = selected.Favorite;
            LogListFullPackets.ScrollIntoView(selected);
            UpdateContentPreview(selected);
            KaitaiCompile();
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
        // zero bytes would be lost here, so we have to resize
        Array.Resize(ref shiftedValue, CurrentContentBytes.Length);

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

    private void FullPacketsLog_MenuItem_OnClick (object sender, RoutedEventArgs e)
    {
        CopySelectedRowContent(LogListFullPackets);
    }

    private void SplitPacketsLog_MenuItem_OnClick (object sender, RoutedEventArgs e)
    {
        CopySelectedRowContent(LogListSplitPackets);
    }

    private void CopySelectedRowContent (ListView listView)
    {
        var selectedRow = (LogRecord) listView.SelectedItem;
        var text =
            $"{selectedRow.Source}\t\t\t{selectedRow.Timestamp}\t\t\t{Convert.ToHexString(selectedRow.ContentBytes)}\n";
        Clipboard.SetText(text);
    }

    private void FavoriteToggleButton_OnChecked (object sender, RoutedEventArgs e)
    {
        var logList = GetActivePacketLog();
        if (logList.SelectedItem is null)
        {
            return;
        }

        var item = (LogRecord) logList.SelectedItem;
        item.Favorite = true;
        UpdateStoredPacket(item);
    }

    private void FavoriteToggleButton_OnUnchecked (object sender, RoutedEventArgs e)
    {
        var logList = GetActivePacketLog();
        if (logList.SelectedItem is null)
        {
            return;
        }

        var item = (LogRecord) logList.SelectedItem;
        item.Favorite = false;
        UpdateStoredPacket(item);
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

    private void UpdateStoredPacket (LogRecord logRecord)
    {
        var logList = GetActivePacketLog();
        var collection = logList == LogListFullPackets ? PacketCollection : SplittedPacketCollection;
        var storedPacket = collection.FindById(logRecord.Id);
        if (storedPacket is null)
        {
            Console.WriteLine($"Stored packet {logRecord.Id} not found");
            return;
        }

        storedPacket.Favorite = logRecord.Favorite;
        collection.Update(storedPacket);
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
        var logList = GetActivePacketLog();
        CollectionViewSource.GetDefaultView(LogListFullPackets.ItemsSource).Refresh();
        CollectionViewSource.GetDefaultView(LogListSplitPackets.ItemsSource).Refresh();
        if (logList.Items.Count < 1)
        {
            return;
        }

        var selected = logList.SelectedItem ?? logList.Items[^1];
        if (!logList.Items.PassesFilter(selected))
        {
            // should only happen when switching to a more restricted view with filtered out item selected
            selected = logList.Items[^1];
        }

        logList.SelectedItem = selected;

        logList.ScrollIntoView(selected);
    }

    private void KaitaiCompile_OnClick (object sender, RoutedEventArgs e)
    {
        KaitaiCompile();
    }

    public void KaitaiCompile ()
    {
        var selectedItem = KaitaiDefitionsTreeView.SelectedItem;
        KaitaiScriptCompileOutputText.Text = "";
        if (selectedItem is null)
        {
            return;
        }

        SaveCurrentKaitaiDefinition(selectedItem);
        var selectedKaitai = (string) ((TreeViewItem) selectedItem).Header;
        var logList = GetActivePacketLog();
        var currentPacket = (LogRecord) logList.SelectedItem;
        if (currentPacket is null)
        {
            return;
        }

        var currentDefinition = KaitaiDefinitions[selectedKaitai];
        try
        {
            var parsedOutput = KaitaiParser.ParseByteArray(currentDefinition, currentPacket.ContentBytes);
            PrettifyKaitaiCompileOutput(parsedOutput);
        }
        catch (Exception ex)
        {
            KaitaiScriptCompileOutputText.Text = ex.Message;
        }
    }

    public void PrettifyKaitaiCompileOutput (List<KaitaiParsedEntry> parsedEntries)
    {
        foreach (var parsedEntry in parsedEntries)
        {
            var splitPath = parsedEntry.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var indentLevel = (splitPath.Length - 1) * 2;
            var entryName = splitPath[^1];
            var padding = string.Empty.PadLeft(indentLevel, '·');
            KaitaiScriptCompileOutputText.Inlines.Add(
                new Run(padding) { Foreground = Brushes.LightGray, FontSize = 12 });
            if (parsedEntry.IsTrivialType)
            {
                KaitaiScriptCompileOutputText.Inlines.Add(new Run($"{entryName}") { Foreground = Brushes.Blue });
                KaitaiScriptCompileOutputText.Inlines.Add(new Run(" = "));

                switch (parsedEntry.Type)
                {
                    case "str":
                        KaitaiScriptCompileOutputText.Inlines.Add(
                            new Run($"\"{(string) parsedEntry.Value}\"") { FontWeight = FontWeights.Bold });
                        break;
                    case "f4":
                    case "f8":
                        KaitaiScriptCompileOutputText.Inlines.Add(
                            new Run(((double) parsedEntry.Value).ToString()) { FontWeight = FontWeights.Bold });
                        break;
                    case SimpleKaitaiParser.SimpleKaitaiParser.ByteArrayTypeName:
                        var byteArray = parsedEntry.Value as byte[];
                        var stringValues = byteArray.Select(b => $"0x{b:X2}").ToList();

                        KaitaiScriptCompileOutputText.Inlines.Add(
                            new Run($"[{string.Join(", ", stringValues)}]") { FontWeight = FontWeights.Bold });
                        KaitaiScriptCompileOutputText.Inlines.Add(new Run($" = [{string.Join(", ", byteArray)}]")
                            { Foreground = Brushes.Gray });
                        break;
                    case SimpleKaitaiParser.SimpleKaitaiParser.EnumEntryValueType:
                        KaitaiScriptCompileOutputText.Inlines.Add(new Run(parsedEntry.EnumValue.ToUpper())
                            { FontWeight = FontWeights.Bold });

                        var enumValueBytes = parsedEntry.Value as byte[];
                        var enumBytesCopy = new byte[enumValueBytes.Length];
                        Array.Copy(enumValueBytes, enumBytesCopy, enumValueBytes.Length);
                        Array.Reverse(enumValueBytes);
                        Array.Resize(ref enumValueBytes, 8);
                        var enumBytesStr = Convert.ToHexString(enumBytesCopy).TrimStart('0');
                        if (enumBytesStr.Length == 0)
                        {
                            enumBytesStr = "0";
                        }

                        var padWidth = enumBytesStr.Length % 2 == 0 ? enumBytesStr.Length : enumBytesStr.Length + 1;
                        enumBytesStr = enumBytesStr.PadLeft(padWidth, '0');

                        var enumValue = BitConverter.ToInt64(enumValueBytes);

                        KaitaiScriptCompileOutputText.Inlines.Add(
                            new Run(
                                    $" ({SnakeCaseToCamelCase(parsedEntry.EnumName)}::{parsedEntry.EnumValue.ToUpper()} " +
                                    $"= 0x{enumBytesStr} = {enumValue})")
                                { Foreground = Brushes.Gray });
                        break;
                    default:
                        var byteValue = parsedEntry.Value as byte[];
                        KaitaiScriptCompileOutputText.Inlines.Add(
                            new Run("0x") { FontWeight = FontWeights.Bold });
                        var zeroBytesToSkip = 0;

                        foreach (var b in byteValue)
                        {
                            if (b != 0)
                            {
                                break;
                            }

                            zeroBytesToSkip++;
                        }

                        if (zeroBytesToSkip == byteValue.Length)
                        {
                            KaitaiScriptCompileOutputText.Inlines.Add(
                                new Run("00") { FontWeight = FontWeights.Bold });
                        }
                        else
                        {
                            for (var i = zeroBytesToSkip; i < byteValue.Length; i++)
                            {
                                if (i != zeroBytesToSkip)
                                {
                                    KaitaiScriptCompileOutputText.Inlines.Add(
                                        new Run("·") { Foreground = Brushes.LightGray });
                                }

                                KaitaiScriptCompileOutputText.Inlines.Add(
                                    new Run($"{byteValue[i]:X2}") { FontWeight = FontWeights.Bold });
                            }
                        }

                        Array.Reverse(byteValue);
                        Array.Resize(ref byteValue, 8);
                        var longValue = BitConverter.ToInt64(byteValue);

                        KaitaiScriptCompileOutputText.Inlines.Add(new Run($" = {longValue}")
                            { Foreground = Brushes.Gray });

                        break;
                }
            }
            else
            {
                KaitaiScriptCompileOutputText.Inlines.Add(new Run($"{entryName} "));
                KaitaiScriptCompileOutputText.Inlines.Add(new Run("[") { FontSize = 12 });
                KaitaiScriptCompileOutputText.Inlines.Add(new Run($"{parsedEntry.Type}")
                    { Foreground = Brushes.ForestGreen, FontSize = 12 });
                KaitaiScriptCompileOutputText.Inlines.Add(new Run("]") { FontSize = 12 });
            }

            KaitaiScriptCompileOutputText.Inlines.Add("\n");
        }
    }

    public static string SnakeCaseToCamelCase (string input)
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
            KaitaiCompile();
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
        KaitaiScriptCompileOutputText.Text = "";
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
        KaitaiScriptCompileOutputText.Text = "";
    }

    public string GetKaitaiPath (string name)
    {
        return Path.Combine(KaitaiPath, name + ".ksy");
    }

    public void SaveCurrentKaitaiDefinition (object selectedItem)
    {
        if (selectedItem is null)
        {
            return;
        }

        var selectedScriptItem = selectedItem as TreeViewItem;
        KaitaiScriptText.Document.Text = KaitaiScriptText.Document.Text.Replace("\t", "    ");
        var contents = KaitaiScriptText.Document.Text;
        var header = (string) selectedScriptItem.Header;
        var outputFile = GetKaitaiPath(header);
        File.WriteAllText(outputFile, contents);
        KaitaiDefinitions[header] = contents;
    }
}