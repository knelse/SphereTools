using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using BitStreams;

namespace SpherePacketVisualEditor;

public partial class MainWindow
{
    public static readonly byte[] PacketContents = Convert.FromHexString(
        "BE002C0100340FB017008B0F80842E090000000000000000409145068002C00037054021A100E0FFFFFFFF177B4164F80048E8920000000000000000001459640028401000000000140006B829000A03010185840280FFFFFFFF1FEE0595E10320A14B02000000000000000050649101A0004100000000500018E0A600280C0404141E14C6E8BEE6C2C6BEE66A007EB91774860F80842E0900000000000000004091450680020401000000400160809B02A030101050482800F8FFFFFF07C8002C0100340FBA1720B00F80842E090000000000000000409145068002C00037054021A100E0FFFFFFBFC020803E50782040380000F8ED5E80C03E0012BA24000000000000000000451619000A0003DC140085840280FFFFFFFF028300FA40E18120E10000E0C77B0102FB0048E8920000000000000000001459640028000C70530014120A00FEFFFFFF0B0C02E803850702850300805FEF0508EC0320A14B02000000000000000050649101A00030C04D0150482800F8FFFFFF2F3008A00F141E08160E000000C4002C0100340FBE17FC8A0F80842E09000000000000000040914526F411150006B829000A090500FFFFFFFFBFDF0B7EC507404297040000000000000000A0C82233FA880A0003DC140085840280FFFFFFFF1FF005BFE20320A14B020000000000000000506491297D440580016E0A80424201C0FFFFFFFF2FF8825FF10190D02501000000000000000028B2C89C3EA202C00037054021A100E0FFFFFFFF277CC1AFF80048E892000000000000000000145964521F510160809B02A0905000F0FFFFFF0FC4002C0100340FC317FC8A0F80842E09000000000000000040914566F511150006B829000A090500FFFFFFFF3FE20B7EC507404297040000000000000000A0C822D3FA880A0003DC140085840280FFFFFFFF5FF105BFE20320A14B020000000000000000506491797D440580016E0A80424201C0FFFFFFFFCFF8825FF10190D02501000000000000000028B2C8C43EA202C00037054021A100E0FFFFFFFF777CC1AFF80048E892000000000000000000145964661F510160809B02A0905000F0FFFFFF0FCA002C0100340FC817FC8A0F80842E090000000000000000409145A6F611150006B829000A090500FFFFFFFFBFE40B88C507404297040000000000000000A0C8224B1B150006B829000A090500FFFFFFFF85870200000000809FF205C4E20320A14B020000000000000000506491B58D0A0003DC140085840280FFFFFFFFC2434100000000C06FF90262F10190D02501000000000000000028B2C8E2460580016E0A80424201C0FFFFFF7FE1A14000000000E0C77C01B1F80048E8920000000000000000001459647523B9002C0100340FCC17104B0160809B02A0905000F0FFFFFF5F78282000000000F8C93502193E0012BA24000000000000000000451619000A1004000000000580016E0A80C240404021A100E0FFFFFFFFB7E54865F80048E8920000000000000000001459640028401000000000140006B829000A03010185078531BAAFB9B0B1AF391880DF9A2395E10320A14B02000000000000000050649101A0004100000000500018E0A600280C0404141E14C6E8BEE6C2C6BEE6620000A3002C0100340F6C8E54860F80842E0900000000000000004091450680020401000000400160809B02A030101050785018A3FB9A0B1BFB9A9301F8B53952193E0012BA24000000000000000000451619000A1004000000000580016E0A80C2404040E141618CEE6B2E6CEC6B6E06E0E7E64865F80048E8920000000000000000001459640028401000000000140006B829000A03010185078531BAAFB9B0B1AF391A0017002C0100F311970AAE9DE6C1E6056910645CF961BF0E");

    private static Paragraph BitsParagraph;
    private static Encoding Win1251 = null!;

    private static readonly SolidColorBrush SelectionBrush = new ()
    {
        Color = Colors.SlateGray,
        Opacity = 0.5
    };

    public MainWindow ()
    {
        InitializeComponent();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Win1251 = Encoding.GetEncoding(1251);
        BitsParagraph = new Paragraph
        {
            Padding = new Thickness(0, 4, 0, 4)
        };
        PacketVisualizerRtb.Document.Blocks.Add(BitsParagraph);
        UpdatePacketContent(PacketContents);
    }

    public void UpdatePacketContent (byte[]? packet)
    {
        var stream = new BitStream(packet);
        var sb = new StringBuilder();
        while (stream.ValidPosition)
        {
            var currentByte = stream.ReadByte();
            var byteStr = Convert.ToString(currentByte, 2).PadLeft(8, '0');
            sb.AppendLine(byteStr);
        }

        BitsParagraph.Inlines.Clear();
        BitsParagraph.Inlines.Add(sb.ToString());
    }

    private void PacketVisualizerRtb_OnSelectionChanged (object sender, RoutedEventArgs e)
    {
        var selectionStart = PacketVisualizerRtb.Selection.Start;
        var selectionEnd = PacketVisualizerRtb.Selection.End;

        foreach (var inline in BitsParagraph.Inlines)
        {
            inline.Background = Brushes.Transparent;
        }

        PacketSelectedValueDisplay.Text = string.Empty;
        if (selectionStart is null || selectionEnd is null)
        {
            PacketSelectedValueDisplay.Text = "Select bits to show value preview";
            return;
        }

        var selectionStart_currentLineStart = selectionStart.GetLineStartPosition(0);
        var selectionStart_nextLineStart = selectionStart_currentLineStart.GetLineStartPosition(1);
        var selectionEnd_currentLineStart = selectionEnd.GetLineStartPosition(0);
        var selectionEnd_previousLineEnd = selectionEnd_currentLineStart.GetPositionAtOffset(-2);
        var selectionEnd_currentLineEnd = selectionEnd_currentLineStart.GetLineStartPosition(1);
        var startEndSameLine =
            selectionStart_currentLineStart.GetOffsetToPosition(selectionEnd_currentLineStart) == 0;

        var newSelectionRange_middle = startEndSameLine
            ? new TextRange(selectionStart, selectionEnd)
            : new TextRange(selectionStart_nextLineStart, selectionEnd_previousLineEnd);

        ColorizeTextRange(newSelectionRange_middle);

        TextRange? newSelectionRange_invertedFirstLine = null;
        TextRange? newSelectionRange_invertedLastLine = null;

        if (!startEndSameLine)
        {
            newSelectionRange_invertedFirstLine = new TextRange(selectionStart_currentLineStart, selectionStart);
            newSelectionRange_invertedLastLine = new TextRange(selectionEnd, selectionEnd_currentLineEnd);
        }

        ColorizeTextRange(newSelectionRange_invertedFirstLine);
        ColorizeTextRange(newSelectionRange_invertedLastLine);

        var bitsSelectionStart = GetBitValueInTextRange(newSelectionRange_invertedFirstLine);
        var bitsSelectionMiddle = GetBitValueInTextRange(newSelectionRange_middle);
        var bitsSelectionEnd = GetBitValueInTextRange(newSelectionRange_invertedLastLine);

        var fullValueBits = new List<Bit>(bitsSelectionEnd);
        fullValueBits.AddRange(bitsSelectionMiddle);
        fullValueBits.AddRange(bitsSelectionStart);

        Console.WriteLine($"start_start {selectionStart_currentLineStart.GetOffsetToPosition(BitsParagraph.ContentStart)} | start_actual_start {selectionStart.GetOffsetToPosition(BitsParagraph.ContentStart)} " +
                          $"| end_actual_end {selectionEnd.GetOffsetToPosition(BitsParagraph.ContentStart)} | end_end {selectionEnd_currentLineEnd.GetOffsetToPosition(BitsParagraph.ContentStart)}");

        UpdateSelectedValueDisplay(fullValueBits);
    }

    private void ColorizeTextRange (TextRange? textRange)
    {
        if (textRange is null)
        {
            return;
        }

        textRange.ApplyPropertyValue(TextElement.BackgroundProperty, SelectionBrush);
    }

    private List<Bit> GetBitValueInTextRange (TextRange? textRange)
    {
        var result = new List<Bit>();
        if (textRange is null)
        {
            return result;
        }

        var text = textRange.Text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();
        foreach (var t in text)
        {
            result.AddRange(t.Select(x => new Bit(x - '0')).ToList());
        }

        return result;
    }

    private void UpdateSelectedValueDisplay (List<Bit> bits)
    {
        if (!bits.Any())
        {
            PacketSelectedValueDisplay.Text = "Select bits to show value preview";
            return;
        }

        bits.Reverse();

        var remainingBitsToFullByte = bits.Count % 8;
        var bitsPadding = remainingBitsToFullByte == 0 ? 0 : 8 - remainingBitsToFullByte;

        if (bitsPadding != 0)
        {
            for (var i = 0; i < bitsPadding; i++)
            {
                bits.Add(0);
            }
        }
        var bytes = BitStream.BitArrayToBytes(bits.ToArray());
        var bytesString = Convert.ToHexString(bytes);
        var textString = Win1251.GetString(bytes);
        var stream = new BitStream(bytes);
        var actualBits = bits.ToArray()[..^(bitsPadding)];
        Array.Reverse(actualBits);
        var bitsString = string.Join("", actualBits.Select(x => (int) x));
        var longValue = stream.ReadInt64();
        var longValueStr = bytes.Length > 8 ? "(too large)" : $"0x{longValue:X} = {longValue}";
        stream.Seek(0, 0);
        var ulongValue = stream.ReadUInt64();
        var ulongValueStr = bytes.Length > 8 ? "(too large)" : $"0x{ulongValue:X} = {ulongValue}";
        stream.Seek(0, 0);

        var sb = new StringBuilder();
        sb.AppendLine($"Bits:\t {bitsString} ({bitsString.Length})");
        sb.AppendLine($"Bytes:\t {bytesString}");
        sb.AppendLine($"Text:\t {textString}");
        sb.AppendLine($"Int64:\t {longValueStr}");
        sb.AppendLine($"UInt64: {ulongValueStr}");
        PacketSelectedValueDisplay.Text = sb.ToString();
    }
}