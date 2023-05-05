using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using PacketLogViewer.Models;

namespace PacketLogViewer
{
    public partial class MainWindow
    {
        public static Encoding Win1251 = null!;
        
        public MainWindow()
        {
            InitializeComponent();
            var defaultContent = new List<LogRecord>
            {
                new("SRV", DateTime.Now,
                    "72002C010018E4CAE6084063830C2E4C2EAC6D8E8B6BAEAC8C6C8E8B0B0000206C0E0485C646A6A626E62B2645014006C645E6460120E60424C88908640C2D4CC565CCEC0C40C5A54D8C0C40C5850E8F0E802DADCC8DCEA50CAF0C80C70724068429A929890A2406008429A929890A240600"),
            };

            var temp = "abc";
            for (var i = 0; i < 100; i++)
            {
                temp += "di23q4ujednklxwsanrfik23u459pjhndfwrq3k2l;jr43o2p4jro32\n";
            }

            LogRecordTextDisplay.Text = temp;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Win1251 = Encoding.GetEncoding(1251);

            var loadedContent = LoadContent();

            LogList.ItemsSource = loadedContent.Any() ? loadedContent : defaultContent;
            LogList.SelectionChanged += OnLogListOnSelectionChanged;
            LogList.SelectedItem = LogList.Items[0];
        }

        public static List<LogRecord> LoadContent()
        {
            var filePath = @"c:\_sphereDumps\mixed";
            var textContent = File.ReadAllLines(filePath);
            var content = new List<LogRecord>();
            foreach (var logEntry in textContent)
            {
                var cleanedUpText = logEntry.Replace("=", "").Replace("-", "");
                if (string.IsNullOrWhiteSpace(cleanedUpText))
                {
                    continue;
                }

                var split = cleanedUpText.Split('\t',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                try
                {
                    content.Add(new LogRecord( split.Length > 2 ? split[0] : "---", split.Length > 1 ? DateTime.Parse(split[1]) : DateTime.MinValue, split[^1]));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(logEntry + "\n" + ex);
                }
            }

            return content;
        }

        private void OnLogListOnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            var selected = args.AddedItems[0] as LogRecord;
            try
            {
                var positiveValue = "0" + selected?.Content ?? throw new InvalidOperationException();
                var actualValue = BigInteger.Parse(positiveValue, NumberStyles.HexNumber);

                LogRecordTextDisplay.Text = ToReadableBinaryString(actualValue);
            }
            catch
            {
                LogRecordTextDisplay.Text = "Selected value is not a hex string!";
            }
        }

        public static string GetBinaryPaddedString(byte b) => Convert.ToString(b, 2).PadLeft(8, '0');
        
        // 0xAD is a soft hyphen and doesn't render, but it's not a control/whitespace/separator
        public static char GetVisibleChar (char c) => char.IsControl(c) || char.IsWhiteSpace(c) || char.IsSeparator(c) || c == 0xAD ? '·' : c;

        public static char GetEncoded1251Char(byte b)
        {
            var char1251 = Win1251.GetChars(new[] { b })[0];
            return GetVisibleChar(char1251);
        }

        public static char GetEncodedLoginChar(byte b)
        {
            var loginChar = b % 2 == 0 ? (char)(b / 4 - 1 + 'A') : (char)(b / 4 - 48 + '0');
            return GetVisibleChar(loginChar);
        }

        public static string GetFormattedBinaryOutput(byte b)
        {
            return $"{GetBinaryPaddedString(b)}{b.ToString(),5}{b,4:X}h" +
                   $"{GetEncoded1251Char(b).ToString(),3}{GetEncodedLoginChar(b).ToString(),3}";
        }

        public static string ToReadableBinaryString(BigInteger bigInt)
        {
            var shiftedValues = new List<byte[]>
            {
                bigInt.ToByteArray(),
                (bigInt << 1).ToByteArray(),
                (bigInt << 2).ToByteArray(),
                (bigInt << 3).ToByteArray(),
                (bigInt << 4).ToByteArray(),
                (bigInt << 5).ToByteArray(),
                (bigInt << 6).ToByteArray(),
                (bigInt << 7).ToByteArray()
            };
            var sb = new StringBuilder();
            sb.AppendLine();

            var length = shiftedValues[7].Length;

            for (var i = length - 1; i >= 0; i--)
            {
                sb.Append($"{length - i,3:D}: ");
                foreach (var shiftedValue in shiftedValues)
                {
                    var current = shiftedValue.Length > i ? shiftedValue[i] : (byte) 0;
                    sb.Append(GetFormattedBinaryOutput(current));
                    sb.Append("  ||  ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}