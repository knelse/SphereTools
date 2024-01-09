using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BitStreams;
using LiteDB;
using PacketLogViewer.Models;
using SpherePacketVisualEditor;
using SphServer.Helpers;

namespace PacketLogViewer;

public partial class MainWindow
{
    private const string PacketDefinitionPath = @"C:\\source\\sphPacketDefinitions";
    private const string PacketDefinitionExtension = ".spd";
    private const string ExportedPartExtension = ".spdp";
    private const string EnumExtension = ".sphenum";
    public static Encoding Win1251 = null!;

    public static readonly LiteDatabase PacketDatabase =
        new (@"Filename=C:\_sphereStuff\sph_packets.db;Connection=shared;");

    public static readonly ILiteCollection<StoredPacket> PacketCollection =
        PacketDatabase.GetCollection<StoredPacket>("Packets");

    public static readonly ILiteCollection<StoredPacket> SplittedPacketCollection =
        PacketDatabase.GetCollection<StoredPacket>("SplittedPackets");

    private static Bit[] PacketContentBits = null!;

    private static BitStream? CurrentContentBitStream;

    private static SolidColorBrush SelectionBrush = null!;

    public static readonly Dictionary<string, Dictionary<int, string>> DefinedEnums = new ();
    private readonly List<string> DefinedEnumNames = new ();

    public readonly PacketCapture PacketCapture;
    public readonly ObservableCollection<PacketDefinition> PacketDefinitions = new ();

    public readonly ObservableCollection<PacketPart> PacketParts = new ();
    public readonly DispatcherTimer SphereTimeUpdateTimer;
    public readonly ObservableCollection<Subpacket> Subpackets = new ();

    private TextPointer? EndTextPointer;
    private int? LastCaretOffset;
    private double LastVerticalOffset;
    private ScrollViewer? PacketDisplayScrollViewer;
    private TextPointer? StartTextPointer;

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

        LogListFullPackets.SelectionChanged += LogListOnSelectionChanged;
        LogListSplitPackets.SelectionChanged += LogListOnSelectionChanged;

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

        PacketVisualizerControl.KeyDown += PacketVisualizerControlAddPacketPart;
        PacketVisualizerControl.KeyDown += PacketVisualizerControlHandlePartSelection;
        SelectionBrush = new SolidColorBrush
        {
            Color = ((SolidColorBrush) PacketVisualizerControl.SelectionBrush).Color,
            Opacity = PacketVisualizerControl.SelectionOpacity
        };
        PacketVisualizerControl.SelectionBrush = Brushes.Transparent;
        PacketVisualizerControl.PreviewMouseWheel += (_, _) => { };
        var scrollViewerProperty =
            typeof (RichTextBox).GetProperty("ScrollViewer", BindingFlags.NonPublic | BindingFlags.Instance)!;

        LoadEnums();

        Loaded += (_, _) =>
        {
            PacketDisplayScrollViewer = (ScrollViewer) scrollViewerProperty.GetValue(PacketVisualizerControl)!;

            PacketDisplayScrollViewer!.ScrollChanged += (sender, _) => { SynchronizeScrollValues(sender); };

            PacketVisualizerDefinedPacketValuesScrollViewer.ScrollChanged += (sender, _) =>
            {
                SynchronizeScrollValues(sender);
            };

            PacketVisualizerLineNumbersAndValuesScrollViewer.ScrollChanged +=
                (sender, _) => { SynchronizeScrollValues(sender); };
        };

        KeyUp += (_, e) =>
        {
            if (e.KeyboardDevice.Modifiers != ModifierKeys.Control || e.Key != Key.S)
            {
                return;
            }

            if (DefinedPacketsListBox.SelectedItem is null)
            {
                return;
            }

            SaveSelectedPacketDefinition();
        };

        CreateFlowDocumentWithHighlights(false, true);
        LoadPacketDefinitions();

        PacketPartsInDefinitionListBox.ItemsSource = PacketParts;

        SphereTimeUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0 / 24)
        };
        SphereTimeUpdateTimer.Tick += (_, _) => UpdateGameTime();
        SphereTimeUpdateTimer.Start();

        ScrollIntoViewIfSelectionExists();
    }

    public byte[]? CurrentContentBytes { get; set; }
    public ObservableCollection<LogRecord> LogRecords { get; } = new ();
    public ObservableCollection<LogRecord> LogRecordsSplitted { get; } = new ();
    public bool ShowFavoritesOnly { get; set; }
    public bool HideUninteresting { get; set; } = true;

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
        }.AppendAnalyticsData()).ToList();
        SplittedPacketCollection.InsertBulk(splitPackets);

        Dispatcher.Invoke(() =>
        {
            LogRecords.Add(new LogRecord(storedPacket));
            splitPackets.ForEach(x => LogRecordsSplitted.Add(new LogRecord(x)));
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
            PacketCapture.SetClientId((short) id);
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

        var packetsFull = PacketCollection.Query().Where(x => x.Favorite).OrderByDescending(x => x.Timestamp)
            .Limit(100).ToList();
        if (packetsFull is null)
        {
            MessageBox.Show("Packets to load (full) are null");
            return;
        }

        packetsFull.AddRange(PacketCollection.Query().Where(x => !x.HiddenByDefault).OrderByDescending(x => x.Timestamp)
            .Limit(1000).ToList());

        var packetsSplit = SplittedPacketCollection.Query().Where(x => x.Favorite).OrderByDescending(x => x.Timestamp)
            .Limit(100).ToList();
        if (packetsSplit is null)
        {
            MessageBox.Show("Packets to load (split) are null");
            return;
        }

        packetsSplit.AddRange(SplittedPacketCollection.Query().OrderByDescending(x => x.Timestamp)
            .Limit(1000).ToList());

        if (!packetsFull.Any())
        {
            MessageBox.Show("No full packets to load");
        }

        if (!packetsSplit.Any())
        {
            MessageBox.Show("No split packets to load");
        }

        packetsFull.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        packetsSplit.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

        packetsFull.ForEach(x => LogRecords.Add(new LogRecord(x)));
        packetsSplit.ForEach(x => LogRecordsSplitted.Add(new LogRecord(x)));

        LogListFullPackets.UpdateLayout();
        LogListSplitPackets.UpdateLayout();
    }

    public void UpdateContentPreview (LogRecord selected)
    {
        try
        {
            var bytes = selected.ContentBytes;  
            UpdatePacketPartValues();
            UpdateDefinedPackets();
            CreateFlowDocumentWithHighlights(false, true);     
            ClearSelection();
            var packetContents = PacketAnalyzer.GetTextOutputForPacket(bytes);
            ContentPreview.Text = packetContents + "\n";
            var sphObjects = ObjectPacketTools.GetObjectsFromPacket(bytes);
            ContentPreview.Text += sphObjects.Count > 0 ? ObjectPacketTools.GetTextOutput(sphObjects) : "";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            ContentPreview.Text = "Not an item packet";
        }
    }

    private void LogListOnSelectionChanged (object sender, SelectionChangedEventArgs args)
    {
        try
        {
            if (args.AddedItems.Count < 1)
            {
                return;
            }

            var selected = args.AddedItems[0] as LogRecord;
            CurrentContentBytes = selected.ContentBytes;

            IsFavorite.IsChecked = selected.Favorite;
            LogListFullPackets.ScrollIntoView(selected);
            UpdateContentPreview(selected);
        }
        catch
        {
            IsFavorite.IsChecked = false;
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

    private void LoadEnums ()
    {
        var enumFiles = Directory.EnumerateFiles(PacketDefinitionPath, $"*{EnumExtension}");
        foreach (var enumFile in enumFiles)
        {
            var enumName = Path.GetFileNameWithoutExtension(enumFile);
            DefinedEnums.Add(enumName, new Dictionary<int, string>());
            DefinedEnumNames.Add(enumName);
            var enumEntryLines = File.ReadAllLines(enumFile).Select(x =>
                x.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();
            foreach (var enumEntryLine in enumEntryLines)
            {
                var id = int.Parse(enumEntryLine[0]);
                var name = enumEntryLine[1];
                DefinedEnums[enumName].Add(id, name);
            }
        }
    }

    private void LoadPacketDefinitions ()
    {
        DefinedPacketsListBox.ItemsSource = PacketDefinitions;
        if (!Path.Exists(PacketDefinitionPath))
        {
            MessageBox.Show($"Cannot load packet definitions.\nDirectory not found: {PacketDefinitionPath}");
            return;
        }

        var definitionFiles = Directory.EnumerateFiles(PacketDefinitionPath, $"*{PacketDefinitionExtension}");
        foreach (var definitionFile in definitionFiles)
        {
            PacketDefinitions.Add(new PacketDefinition
            {
                Name = Path.GetFileNameWithoutExtension(definitionFile),
                FilePath = definitionFile
            });
        }

        SubpacketsListBox.ItemsSource = Subpackets;

        var subpacketFiles = Directory.EnumerateFiles(PacketDefinitionPath, $"*{ExportedPartExtension}");
        foreach (var subpacketFile in subpacketFiles)
        {
            Subpackets.Add(new Subpacket
            {
                Name = Path.GetFileNameWithoutExtension(subpacketFile),
                FilePath = subpacketFile
            });
        }

        if (Subpackets.Any())
        {
            SubpacketsListBox.SelectedItem = Subpackets.First();
        }
    }

    private void SynchronizeScrollValues (object source)
    {
        var scrollViewer = (ScrollViewer) source;
        if (scrollViewer != PacketVisualizerLineNumbersAndValuesScrollViewer &&
            Math.Abs(PacketVisualizerLineNumbersAndValuesScrollViewer.VerticalOffset - scrollViewer.VerticalOffset) >
            double.Epsilon)
        {
            PacketVisualizerLineNumbersAndValuesScrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
        }

        if (scrollViewer != PacketVisualizerDefinedPacketValuesScrollViewer &&
            Math.Abs(PacketVisualizerDefinedPacketValuesScrollViewer.VerticalOffset - scrollViewer.VerticalOffset) >
            double.Epsilon)
        {
            PacketVisualizerDefinedPacketValuesScrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
        }

        if (scrollViewer != PacketDisplayScrollViewer &&
            Math.Abs(PacketDisplayScrollViewer!.VerticalOffset - scrollViewer.VerticalOffset) > double.Epsilon)
        {
            PacketDisplayScrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
        }
    }

    private void PacketVisualizerControlHandlePartSelection (object sender, KeyEventArgs e)
    {
        if (e.Key != Key.S && e.Key != Key.E && e.Key != Key.Escape)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            ClearSelection();
        }

        var caretPosition = PacketVisualizerControl.CaretPosition;
        if (caretPosition is null)
        {
            ClearSelection();
            LastVerticalOffset = 0;
            return;
        }

        LastCaretOffset = PacketVisualizerControl.Document.ContentStart.GetOffsetToPosition(caretPosition);
        LastVerticalOffset = PacketVisualizerControl.VerticalOffset;

        if (e.Key == Key.S)
        {
            StartTextPointer = caretPosition;
        }

        if (e.Key == Key.E)
        {
            EndTextPointer = caretPosition;
        }

        CreateFlowDocumentWithHighlights();
    }

    private void ClearSelection ()
    {
        StartTextPointer = null;
        EndTextPointer = null;
        LastCaretOffset = null;
    }

    private void UpdateScrolling ()
    {
        if (LastCaretOffset.HasValue)
        {
            var newCaretPosition = LastCaretOffset.Value <= 2
                ? PacketVisualizerControl.Document.ContentStart.GetLineStartPosition(0)
                : PacketVisualizerControl.Document.ContentStart.GetPositionAtOffset(LastCaretOffset.Value);
            if (newCaretPosition is not null)
            {
                PacketVisualizerControl.CaretPosition = newCaretPosition;
            }
        }

        PacketVisualizerControl.ScrollToVerticalOffset(LastVerticalOffset);
        PacketVisualizerLineNumbersAndValuesScrollViewer.ScrollToVerticalOffset(LastVerticalOffset);
        PacketVisualizerDefinedPacketValuesScrollViewer.ScrollToVerticalOffset(LastVerticalOffset);
    }

    private void PacketVisualizerControlAddPacketPart (object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers != ModifierKeys.Control || e.Key != Key.D)
        {
            return;
        }

        if (StartTextPointer is null || EndTextPointer is null)
        {
            return;
        }

        var color = new Color
        {
            A = 150,
            R = (byte) Random.Shared.Next(0, 255),
            G = (byte) Random.Shared.Next(0, 255),
            B = (byte) Random.Shared.Next(0, 255)
        };

        var dialog = new CreatePacketPartDefinitionDialog(color, DefinedEnumNames)
        {
            Owner = this
        };
        if (dialog.ShowDialog() == true)
        {
            var name = dialog.Name;
            color = dialog.Color;
            var type = dialog.PacketPartType ?? PacketPartType.BITS;
            var start = StartTextPointer;
            var end = EndTextPointer;
            var enumName = dialog.EnumName;
            var lengthFromPrevious = dialog.PacketPartType == PacketPartType.STRING && dialog.LengthFromPreviousField;
            AddNewDefinedPacketPart(CreatePacketPart(name, enumName, type, lengthFromPrevious, start, end,
                new SolidColorBrush(color)));
        }
    }

    public void CreateFlowDocumentWithHighlights (bool keepSelection = true, bool firstUpdateOnLoad = false)
    {
        PacketVisualizerLineNumbersAndValues.Inlines.Clear();
        PacketVisualizerDefinedPacketValues.Inlines.Clear();
        if (CurrentContentBytes is null)
        {
            return;
        }

        CurrentContentBitStream = new BitStream(CurrentContentBytes);
        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Hack"),
            FontSize = 14,
            LineHeight = 16,
            LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
            PageWidth = 80,
            TextAlignment = TextAlignment.Right,
            PagePadding = new Thickness(10, 4, 0, 4)
        };
        var paragraph = new Paragraph
        {
            Margin = new Thickness(0)
        };
        var selectionBits = new List<Bit>();
        var sb = new StringBuilder();
        var selectionStartOffset = StartTextPointer?.GetCharOffset();
        var selectionEndOffset = EndTextPointer?.GetCharOffset();

        var actualStart = selectionStartOffset;
        var actualEnd = selectionEndOffset;
        if (actualStart > actualEnd)
        {
            actualEnd = selectionStartOffset;
            actualStart = selectionEndOffset;
        }

        PacketContentBits = CurrentContentBitStream.ReadBits(int.MaxValue);
        var wasInSelection = false;
        PacketPart? previousPacketPart = null;
        Brush? textBrush = null;

        var linesSb = new StringBuilder();
        var valueDisplayDict = new Dictionary<int, PacketPart>();
        var lineByte = 0;

        for (var i = 0; i < PacketContentBits.Length; i++)
        {
            var currentPacketPart = PacketParts.FirstOrDefault(x => x.ContainsBitPosition(i));
            var inSelection = keepSelection && actualStart <= i && actualEnd > i;

            var textBlockChanged = (inSelection && !wasInSelection) || (wasInSelection && !inSelection) ||
                                   (currentPacketPart != null && currentPacketPart != previousPacketPart) ||
                                   (currentPacketPart == null && previousPacketPart != null);
            if (inSelection)
            {
                selectionBits.Add(PacketContentBits[i]);
            }

            if (textBlockChanged)
            {
                var newTextBrush = inSelection ? SelectionBrush : currentPacketPart?.HighlightColor;
                if (textBrush is null)
                {
                    if (sb.Length > 0)
                    {
                        paragraph.Inlines.Add(sb.ToString());
                    }
                }
                else
                {
                    paragraph.Inlines.Add(new Run(sb.ToString())
                    {
                        Background = textBrush
                    });
                }

                sb.Clear();
                textBrush = newTextBrush;

                if (currentPacketPart is not null && previousPacketPart != currentPacketPart)
                {
                    valueDisplayDict.Add(i, currentPacketPart);
                }

                previousPacketPart = currentPacketPart;
            }

            wasInSelection = inSelection;
            var bit = PacketContentBits[i].AsInt();
            sb.Append(bit);
            lineByte <<= 1;
            lineByte += bit;

            if (i % 8 == 7)
            {
                // flip bits
                lineByte = (int) ((((ulong) lineByte * 0x0202020202UL) & 0x010884422010UL) % 1023);
                linesSb.Append($"[{lineByte:X2} ")
                    .Append($"{lineByte}".PadLeft(3, ' ')).Append("] ").AppendLine($"{i / 8} ".PadLeft(5, ' '));
                lineByte = 0;
            }
        }

        if (sb.Length > 0)
        {
            if (textBrush is null)
            {
                paragraph.Inlines.Add(sb.ToString());
            }
            else
            {
                paragraph.Inlines.Add(new Run(sb.ToString())
                {
                    Background = textBrush
                });
            }
        }

        PacketVisualizerLineNumbersAndValues.Text = linesSb.ToString();
        var packetDisplaySb = new StringBuilder();

        for (var i = 0; i < PacketContentBits.Length; i++)
        {
            var addedLineBreak = false;
            if (valueDisplayDict.ContainsKey(i))
            {
                var part = valueDisplayDict[i];
                if (part.StreamPositionStart.Bit == 0)
                {
                    PacketVisualizerDefinedPacketValues.Inlines.Add("\n");
                    addedLineBreak = true;
                }

                PacketVisualizerDefinedPacketValues.Inlines.Add(packetDisplaySb.ToString());
                AddPacketPartInlines(PacketVisualizerDefinedPacketValues.Inlines, part);
                packetDisplaySb.Clear();
            }

            if (i % 8 == 0 && i > 0 && !addedLineBreak)
            {
                packetDisplaySb.AppendLine();
            }
        }

        CurrentContentBitStream.Seek(0, 0);
        if (packetDisplaySb.Length > 0)
        {
            PacketVisualizerDefinedPacketValues.Inlines.Add(packetDisplaySb.ToString());
        }

        document.Blocks.Add(paragraph);

        if (firstUpdateOnLoad)
        {
            PacketReadableDisplayText.Inlines.Clear();
            PacketReadableDisplayText.Inlines.Add(Convert.ToHexString(BitStream.BitArrayToBytes(PacketContentBits)) +
                                                  "\n");
            var toShift = PacketContentBits.ToList();
            for (var i = 0; i < 8; i++)
            {
                var shiftedBytes = BitStream.BitArrayToBytes(toShift.ToArray());
                var shiftedChars = Win1251.GetString(shiftedBytes).ToCharArray();
                var shiftedString = new string(shiftedChars.Select(GetVisibleChar).ToArray());
                PacketReadableDisplayText.Inlines.Add(new Run($"\n[{i}] {shiftedString}")
                {
                    FontSize = 14
                });
                toShift.RemoveAt(0);
            }
        }

        PacketVisualizerControl.Document = document;
        UpdateSelectedValueDisplay(selectionBits);
        UpdateScrolling();
    }

    private void PacketVisualizerControl_OnSelectionChanged (object o, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    private PacketPart CreatePacketPart (string name, string? enumName, PacketPartType packetPartType,
        bool lengthFromPrevious, TextPointer start, TextPointer end, Brush highlightColor)
    {
        var bitOffsetStart = start.GetCharOffset();
        var bitOffsetEnd = end.GetCharOffset();
        var offsetStart = bitOffsetStart / 8;
        var bitStart = bitOffsetStart % 8;
        var offsetEnd = bitOffsetEnd / 8;
        var bitEnd = bitOffsetEnd % 8;

        var positionStart = new StreamPosition(offsetStart, bitStart);
        var positionEnd = new StreamPosition(offsetEnd, bitEnd);
        var actualStart = positionStart;
        var actualEnd = positionEnd;
        if (positionStart.CompareTo(positionEnd) > 0)
        {
            actualStart = positionEnd;
            actualEnd = positionStart;
        }

        var bitLength = Math.Abs(bitOffsetEnd - bitOffsetStart);
        CurrentContentBitStream.Seek(actualStart.Offset, actualStart.Bit);
        var bits = CurrentContentBitStream.ReadBits(bitLength).ToList();
        bits.Reverse();
        var streamValueLength = (long) bits.Count;
        return new PacketPart(streamValueLength, highlightColor, name, enumName, lengthFromPrevious, packetPartType,
            actualStart, actualEnd, bits);
    }

    private void UpdateDefinedPackets ()
    {
        DefinedPacketPartsControl.Document.Blocks.Clear();
        var toSort = PacketParts.ToList();
        toSort.Sort((a, b) => a.StreamPositionStart.CompareTo(b.StreamPositionStart));
        PacketParts.Clear();
        toSort.ForEach(x => PacketParts.Add(x));
        foreach (var part in PacketParts)
        {
            var paragraph = new Paragraph();
            if (part.Name == "entity_id")
            {
                // it's a hack for now
                paragraph.Inlines.Add(
                    $"=============================================== {part.DisplayText.Ulong} ===============================================\n\n");
            }

            AddPacketPartInlines(paragraph.Inlines, part);

            DefinedPacketPartsControl.Document.Blocks.Add(paragraph);
        }
    }

    private void AddPacketPartInlines (InlineCollection inlineCollection, PacketPart part)
    {
        inlineCollection.Add(new Run($"{part.Name}")
        {
            Background = part.HighlightColor
        });
        inlineCollection.Add(": ");
        var valueStr = part.GetDisplayTextForValueType();

        if (part.EnumName is not null)
        {
            var enumValue = part.DisplayText.EnumValue?.ToUpper() ?? string.Empty;
            inlineCollection.Add(new Run(enumValue) { FontWeight = FontWeights.Bold });
            var enumName = SnakeCaseToCamelCase(part.EnumName);
            inlineCollection.Add(new Run($" ({enumName}::{enumValue} = {valueStr})") { Foreground = Brushes.Gray });
        }
        else
        {
            var valueStrSplit = valueStr
                .Split('=', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
            if (valueStrSplit.Count > 1)
            {
                // hex = dec, like 0x17B0 = 6064
                inlineCollection.Add(new Run(valueStrSplit[0]) { FontWeight = FontWeights.Bold });
                inlineCollection.Add(new Run($" = {valueStrSplit[1]}") { Foreground = Brushes.Gray });
            }

            else
            {
                var valueTypeStr = part.PacketPartType == PacketPartType.BITS ? "0b" :
                    part.PacketPartType == PacketPartType.BYTES ? "0x" : string.Empty;
                inlineCollection.Add(new Run(valueTypeStr + valueStr) { FontWeight = FontWeights.Bold });
            }
        }

        inlineCollection.Add(
            new Run(
                $" [{Enum.GetName(part.PacketPartType) ?? string.Empty}] [{part.StreamPositionEnd} to {part.StreamPositionStart}, {part.BitLength} bits] ")
            {
                Foreground = Brushes.Gray
            });
    }

    private void AddNewDefinedPacketPartBulk (List<PacketPart> packetParts)
    {
        packetParts.ForEach(x => AddNewDefinedPacketPart(x, true));
        UpdatePacketPartValues();
        UpdateDefinedPackets();
        CreateFlowDocumentWithHighlights(false);
        ClearSelection();
    }

    private void AddNewDefinedPacketPart (PacketPart packetPart, bool isBulk = false)
    {
        var newPacketParts = new List<PacketPart>();
        foreach (var definedPacketPart in PacketParts)
        {
            if (packetPart.Overlaps(definedPacketPart))
            {
                // remove old one
                continue;
            }

            if (packetPart.ContainedWithin(definedPacketPart))
            {
                // split old and make it: old_start new old_end
                var oldStart = definedPacketPart.GetPiece(definedPacketPart.StreamPositionStart,
                    packetPart.StreamPositionStart, definedPacketPart.Name + "_1");
                var oldEnd = definedPacketPart.GetPiece(packetPart.StreamPositionEnd,
                    definedPacketPart.StreamPositionEnd, definedPacketPart.Name + "_2");
                newPacketParts.Add(oldStart);
                newPacketParts.Add(oldEnd);
                continue;
            }

            if (packetPart.StreamPositionStart.CompareTo(definedPacketPart.StreamPositionStart) <= 0 &&
                packetPart.StreamPositionEnd.CompareTo(definedPacketPart.StreamPositionEnd) < 0)
            {
                // leave a chunk of old when new part intersects the beginning of old
                var oldChunk = definedPacketPart.GetPiece(packetPart.StreamPositionEnd,
                    definedPacketPart.StreamPositionEnd);
                newPacketParts.Add(oldChunk);
                continue;
            }

            if (packetPart.StreamPositionStart.CompareTo(definedPacketPart.StreamPositionStart) > 0 &&
                packetPart.StreamPositionStart.CompareTo(definedPacketPart.StreamPositionEnd) < 0 &&
                packetPart.StreamPositionEnd.CompareTo(definedPacketPart.StreamPositionEnd) >= 0)
            {
                // leave a chunk of old when new part intersects the end of old
                var oldChunk = definedPacketPart.GetPiece(definedPacketPart.StreamPositionStart,
                    packetPart.StreamPositionStart);
                newPacketParts.Add(oldChunk);
                continue;
            }

            newPacketParts.Add(definedPacketPart);
        }

        newPacketParts.Add(packetPart);
        newPacketParts.Sort((a, b) => a.StreamPositionStart.CompareTo(b.StreamPositionStart));
        PacketParts.Clear();
        newPacketParts.ForEach(x => PacketParts.Add(x));
        if (!isBulk)
        {
            UpdateDefinedPackets();
            CreateFlowDocumentWithHighlights(false);
            ClearSelection();
        }
    }

    private void UpdateSelectedValueDisplay (List<Bit> bits)
    {
        if (!bits.Any())
        {
            PacketSelectedValueDisplay.Text = "Select bits to show value preview";
            return;
        }

        var displayText = PacketPart.GetValueDisplayText(bits, null);
        var sb = new StringBuilder();
        sb.AppendLine($"Bits:\t {displayText.Bits} ({displayText.Bits.Length})");
        sb.AppendLine($"Bytes:\t {displayText.Bytes}");
        sb.AppendLine($"Text:\t {displayText.Text}");
        sb.AppendLine($"Int64:\t {displayText.Long}");
        sb.AppendLine($"UInt64: {displayText.Ulong}");
        if (displayText.CoordsClient is not null)
        {
            sb.AppendLine($"CLI coords:\t {displayText.CoordsClient}");
        }

        if (displayText.CoordsServer is not null)
        {
            sb.AppendLine($"SRV coords:\t {displayText.CoordsServer}");
        }

        PacketSelectedValueDisplay.Text = sb.ToString();
    }

    public static char GetVisibleChar (char c)
    {
        return (c >= 0x20 && c <= 0x7E) || c is >= 'А' and <= 'я' ? c : '·';
    }

    private void CreateNewPacketDefinitionButton_OnClick (object sender, RoutedEventArgs e)
    {
        CreatePacketDefinition();
    }

    private void CreatePacketDefinition ()
    {
        var dialog = new SaveNewPacketDefinitionDialog
        {
            Owner = this
        };
        if (dialog.ShowDialog() == true)
        {
            var path = Path.Combine(PacketDefinitionPath, dialog.Name + PacketDefinitionExtension);
            var definition = new PacketDefinition
            {
                Name = dialog.Name,
                FilePath = path
            };

            PacketDefinitions.Add(definition);
            SavePacketDefinition(dialog.Name, 0, 0);
            DefinedPacketsListBox.SelectedItem = definition;
        }
    }

    private void SavePacketDefinition_OnClick (object sender, RoutedEventArgs e)
    {
        SaveSelectedPacketDefinition();
    }

    private void SaveSelectedPacketDefinition ()
    {
        if (DefinedPacketsListBox.SelectedItem is not PacketDefinition selectedDefinition)
        {
            CreatePacketDefinition();
            selectedDefinition = (PacketDefinition) DefinedPacketsListBox.SelectedItem;
        }

        SavePacketDefinition(selectedDefinition.Name, 0, PacketContentBits.Length);
    }

    private void SavePacketDefinition (string definitionName, int startBitOffset, int endBitOffset,
        bool exportedPart = false)
    {
        var fileContentsSb = new StringBuilder();
        var currentIndex = startBitOffset;
        var nextPacketPartIndex =
            PacketParts.ToList().FindIndex(x => x.StreamPositionStart.GetBitPosition() >= startBitOffset);
        while (currentIndex < endBitOffset)
        {
            var nextPacketPart = nextPacketPartIndex == -1
                ? null
                : PacketParts.Count > nextPacketPartIndex
                    ? PacketParts[nextPacketPartIndex]
                    : null;

            var name = PacketPart.UndefinedFieldValue;
            var partType = PacketPartType.BITS;
            var enumName = PacketPart.UndefinedFieldValue;
            Brush highlightColor = Brushes.Transparent;
            Bit[] bits;
            var startPosition = currentIndex;
            if (nextPacketPart is null)
            {
                // only undef until end of packet
                var endBitIndex = endBitOffset > PacketContentBits.Length
                    ? PacketContentBits.Length
                    : endBitOffset;
                bits = PacketContentBits[currentIndex..endBitIndex];
            }
            else
            {
                var nextPartStartIndex = (int) nextPacketPart.StreamPositionStart.GetBitPosition();
                nextPartStartIndex = nextPartStartIndex > endBitOffset ? endBitOffset : nextPartStartIndex;
                if (currentIndex < nextPartStartIndex)
                {
                    // undef between packet parts
                    bits = PacketContentBits[currentIndex..nextPartStartIndex];
                    currentIndex = nextPartStartIndex;
                }
                else
                {
                    var nextPartEndIndex = (int) nextPacketPart.StreamPositionEnd.GetBitPosition();
                    nextPartEndIndex = nextPartEndIndex > endBitOffset ? endBitOffset : nextPartEndIndex;
                    bits = PacketContentBits[nextPartStartIndex..nextPartEndIndex];
                    partType = nextPacketPart.PacketPartType;
                    name = nextPacketPart.Name;
                    enumName = nextPacketPart.EnumName ?? PacketPart.UndefinedFieldValue;
                    highlightColor = nextPacketPart.HighlightColor;
                    currentIndex = nextPartEndIndex;
                    nextPacketPartIndex++;
                }
            }

            var solidColor = ((SolidColorBrush) highlightColor).Color;

            if (exportedPart)
            {
                startPosition -= startBitOffset;
            }

            fileContentsSb.AppendLine(
                $"{name}\t{Enum.GetName(partType)}\t{startPosition}\t{bits.Length}\t{enumName}\t{solidColor.R}\t{solidColor.G}" +
                $"\t{solidColor.B}\t{solidColor.A}\t{string.Join(null, bits.Reverse().Select(x => x.AsInt()))}");

            if (nextPacketPart is null)
            {
                break;
            }
        }

        var fileName = Path.Combine(PacketDefinitionPath,
            definitionName + (exportedPart ? ExportedPartExtension : PacketDefinitionExtension));

        File.WriteAllText(fileName, fileContentsSb.ToString());
    }

    private void UpdatePacketPartValues ()
    {
        if (CurrentContentBitStream is null)
        {
            return;
        }

        for (var i = 0; i < PacketParts.Count; i++)
        {
            var packetPart = PacketParts[i];
            CurrentContentBitStream.Seek(packetPart.StreamPositionStart.Offset, packetPart.StreamPositionStart.Bit);
            var length = packetPart.BitLength;
            if (packetPart.LengthFromPreviousField)
            {
                var byteValue = BitStream.BitArrayToBytes(PacketParts[i - 1].Value.ToArray().Reverse().ToArray()) ??
                                new byte[4];
                Array.Resize(ref byteValue, 4);
                // last byte is always \0?
                length = BitConverter.ToUInt32(byteValue) - 1;
                packetPart.StreamPositionEnd.ChangeOffsetAndBit(length, 0);
                length *= 8;
                packetPart.BitLength = length;
            }

            packetPart.Value = CurrentContentBitStream.ReadBits(length).Reverse().ToList();
            packetPart.UpdateValueDisplayText();
        }
    }

    private void DefinedPacketsListBox_OnSelectionChanged (object sender, SelectionChangedEventArgs e)
    {
        if (DefinedPacketsListBox.SelectedItem is not PacketDefinition packetDefinition)
        {
            return;
        }

        packetDefinition.LoadFromFile();
        PacketParts.Clear();
        packetDefinition.PacketParts.ForEach(x => PacketParts.Add(x));
        LastVerticalOffset = PacketDisplayScrollViewer?.VerticalOffset ?? 0;
        UpdatePacketPartValues();
        UpdateDefinedPackets();
        CreateFlowDocumentWithHighlights();
    }

    private void ExportSubpacket_OnClick (object sender, RoutedEventArgs e)
    {
        var dialog = new ExportSubpacketDialog
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var name = dialog.Name;
        var startOffset = dialog.StartOffset;
        var startBit = StreamPosition.FlipBitOffset(dialog.StartBit);
        var endOffset = dialog.EndOffset;
        var endBit = StreamPosition.FlipBitOffset(dialog.EndBit);

        SavePacketDefinition(name, startOffset * 8 + startBit, endOffset * 8 + endBit, true);
        if (Subpackets.All(x => x.Name != name))
        {
            Subpackets.Add(new Subpacket
            {
                Name = name,
                FilePath = Path.Combine(PacketDefinitionPath, name + ExportedPartExtension)
            });
        }
    }

    private void DeletePacketPartInCurrentDefinition_OnClick (object sender, RoutedEventArgs e)
    {
        if (PacketPartsInDefinitionListBox.SelectedItems.Count == 0)
        {
            return;
        }

        if (MessageBox.Show("Delete selected parts?", "Delete selected parts?",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) !=
            MessageBoxResult.Yes)
        {
            return;
        }

        var listToRemove = PacketPartsInDefinitionListBox.SelectedItems.Cast<PacketPart>().ToList();
        PacketPartsInDefinitionListBox.UnselectAll();

        foreach (var selectedItem in listToRemove)
        {
            PacketParts.Remove(selectedItem);
        }

        UpdatePacketPartValues();
        UpdateDefinedPackets();
        CreateFlowDocumentWithHighlights();
    }

    private void ImportFromSubpacket_OnClick (object sender, RoutedEventArgs e)
    {
        if (SubpacketsListBox.SelectedItem is not Subpacket subpacket)
        {
            return;
        }

        var dialog = new ImportFromSubpacketDialog
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var startOffset = dialog.StartOffset;
        var startBit = StreamPosition.FlipBitOffset(dialog.StartBit);

        subpacket.LoadFromFile();

        AddNewDefinedPacketPartBulk(subpacket.PacketParts.Select(x => x.ChangeOffsetAndBit(startOffset, startBit))
            .ToList());
    }

    private void DeletePacketDefinition_OnClick (object sender, RoutedEventArgs e)
    {
        if (DefinedPacketsListBox.SelectedItem is not PacketDefinition packetDefinition)
        {
            return;
        }

        if (MessageBox.Show("Delete selected definition?", "Delete selected definition?",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
            MessageBoxResult.Yes)
        {
            DeletePacketDefinition(packetDefinition);
        }
    }

    private void DeletePacketDefinition (PacketDefinition packetDefinition)
    {
        PacketDefinitions.Remove(packetDefinition);
        File.Delete(packetDefinition.FilePath);
        if (PacketDefinitions.Any())
        {
            DefinedPacketsListBox.SelectedItem = PacketDefinitions.First();
        }
    }

    public static string SnakeCaseToCamelCase (string input)
    {
        return input
            .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }

    private void PacketPartsInDefinitionListBox_OnSelectionChanged (object sender, SelectionChangedEventArgs e)
    {
    }

    private void EditPacketPart_OnClick (object sender, RoutedEventArgs e)
    {
    }
}