using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BitStreams;

namespace SpherePacketVisualEditor;

public partial class MainWindow
{
    private const string PacketDefinitionPath = @"C:\\source\\sphPacketDefinitions";
    private const string PacketDefinitionExtension = ".spd";
    private const string ExportedPartExtension = ".spdp";
    private const string EnumExtension = ".sphenum";

    private static readonly byte[] PacketContents = Convert.FromHexString(
        "BE002C0100340FB017008B0F80842E090000000000000000409145068002C00037054021A100E0FFFFFFFF177B4164F80048E8920000000000000000001459640028401000000000140006B829000A03010185840280FFFFFFFF1FEE0595E10320A14B02000000000000000050649101A0004100000000500018E0A600280C0404141E14C6E8BEE6C2C6BEE66A007EB91774860F80842E0900000000000000004091450680020401000000400160809B02A030101050482800F8FFFFFF07C8002C0100340FBA1720B00F80842E090000000000000000409145068002C00037054021A100E0FFFFFFBFC020803E50782040380000F8ED5E80C03E0012BA24000000000000000000451619000A0003DC140085840280FFFFFFFF028300FA40E18120E10000E0C77B0102FB0048E8920000000000000000001459640028000C70530014120A00FEFFFFFF0B0C02E803850702850300805FEF0508EC0320A14B02000000000000000050649101A00030C04D0150482800F8FFFFFF2F3008A00F141E08160E000000C4002C0100340FBE17FC8A0F80842E09000000000000000040914526F411150006B829000A090500FFFFFFFFBFDF0B7EC507404297040000000000000000A0C82233FA880A0003DC140085840280FFFFFFFF1FF005BFE20320A14B020000000000000000506491297D440580016E0A80424201C0FFFFFFFF2FF8825FF10190D02501000000000000000028B2C89C3EA202C00037054021A100E0FFFFFFFF277CC1AFF80048E892000000000000000000145964521F510160809B02A0905000F0FFFFFF0FC4002C0100340FC317FC8A0F80842E09000000000000000040914566F511150006B829000A090500FFFFFFFF3FE20B7EC507404297040000000000000000A0C822D3FA880A0003DC140085840280FFFFFFFF5FF105BFE20320A14B020000000000000000506491797D440580016E0A80424201C0FFFFFFFFCFF8825FF10190D02501000000000000000028B2C8C43EA202C00037054021A100E0FFFFFFFF777CC1AFF80048E892000000000000000000145964661F510160809B02A0905000F0FFFFFF0FCA002C0100340FC817FC8A0F80842E090000000000000000409145A6F611150006B829000A090500FFFFFFFFBFE40B88C507404297040000000000000000A0C8224B1B150006B829000A090500FFFFFFFF85870200000000809FF205C4E20320A14B020000000000000000506491B58D0A0003DC140085840280FFFFFFFFC2434100000000C06FF90262F10190D02501000000000000000028B2C8E2460580016E0A80424201C0FFFFFF7FE1A14000000000E0C77C01B1F80048E8920000000000000000001459647523B9002C0100340FCC17104B0160809B02A0905000F0FFFFFF5F78282000000000F8C93502193E0012BA24000000000000000000451619000A1004000000000580016E0A80C240404021A100E0FFFFFFFFB7E54865F80048E8920000000000000000001459640028401000000000140006B829000A03010185078531BAAFB9B0B1AF391880DF9A2395E10320A14B02000000000000000050649101A0004100000000500018E0A600280C0404141E14C6E8BEE6C2C6BEE6620000A3002C0100340F6C8E54860F80842E0900000000000000004091450680020401000000400160809B02A030101050785018A3FB9A0B1BFB9A9301F8B53952193E0012BA24000000000000000000451619000A1004000000000580016E0A80C2404040E141618CEE6B2E6CEC6B6E06E0E7E64865F80048E8920000000000000000001459640028401000000000140006B829000A03010185078531BAAFB9B0B1AF391A0017002C0100F311970AAE9DE6C1E6056910645CF961BF0E");

    private static Bit[] PacketContentBits = null!;

    private static BitStream CurrentContentBitStream = null!;

    public static Encoding Win1251 = null!;

    private static SolidColorBrush SelectionBrush = null!;

    public static readonly Dictionary<string, Dictionary<int, string>> DefinedEnums = new ();
    private readonly List<string> DefinedEnumNames = new ();
    public readonly ObservableCollection<PacketDefinition> PacketDefinitions = new ();

    public readonly ObservableCollection<PacketPart> PacketParts = new ();
    public readonly ObservableCollection<Subpacket> Subpackets = new ();

    private TextPointer? EndTextPointer;
    private int? LastCaretOffset;
    private double LastVerticalOffset;
    private ScrollViewer? PacketDisplayScrollViewer;
    private TextPointer? StartTextPointer;

    public MainWindow ()
    {
        InitializeComponent();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Win1251 = Encoding.GetEncoding(1251);
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

        if (PacketDefinitions.Any())
        {
            DefinedPacketsListBox.SelectedItem = PacketDefinitions.First();
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
            AddNewDefinedPacketPart(CreatePacketPart(name, enumName, type, start, end,
                new SolidColorBrush(color)));
        }
    }

    public void CreateFlowDocumentWithHighlights (bool keepSelection = true, bool firstUpdateOnLoad = false)
    {
        CurrentContentBitStream = new BitStream(PacketContents);
        PacketVisualizerLineNumbersAndValues.Inlines.Clear();
        PacketVisualizerDefinedPacketValues.Inlines.Clear();
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
        TextPointer start,
        TextPointer end, Brush highlightColor)
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
        return new PacketPart(streamValueLength, highlightColor, name, enumName, packetPartType, actualStart, actualEnd,
            bits);
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
        foreach (var packetPart in PacketParts)
        {
            CurrentContentBitStream.Seek(packetPart.StreamPositionStart.Offset, packetPart.StreamPositionStart.Bit);
            packetPart.Value = CurrentContentBitStream.ReadBits(packetPart.BitLength).Reverse().ToList();
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
        if (dialog.ShowDialog() == true)
        {
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
    }

    private void DeletePacketPartInCurrentDefinition_OnClick (object sender, RoutedEventArgs e)
    {
        if (PacketPartsInDefinitionListBox.SelectedItem is not PacketPart packetPart)
        {
            return;
        }

        if (MessageBox.Show("Delete selected part?", "Delete selected part?",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) !=
            MessageBoxResult.Yes)
        {
            return;
        }

        PacketParts.Remove(packetPart);
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
}