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
using PacketLogViewer.Models;

namespace PacketLogViewer
{
    public partial class MainWindow
    {
        public static Encoding Win1251 = null!;
        public BigInteger CurrentContent { get; set; }
        public int StartByteLine { get; set; }
        public int CurrentShift { get; set; }
        public ObservableCollection<LogRecord> LogRecords { get; } = new();
        public readonly FileSystemWatcher FileSystemWatcher;
        public int CurrentStreamPosition { get; set; }
        public bool ShowFavoritesOnly { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            var defaultContent = new List<LogRecord>
            {
                new("SRV", DateTime.Now,
                    "72002C010018E4CAE6084063830C2E4C2EAC6D8E8B6BAEAC8C6C8E8B0B0000206C0E0485C646A6A626E62B2645" +
                    "014006C645E6460120E60424C88908640C2D4CC565CCEC0C40C5A54D8C0C40C5850E8F0E802DADCC8DCEA50CAF0C80C7" +
                    "0724068429A929890A2406008429A929890A240600")
            };

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Win1251 = Encoding.GetEncoding(1251);

            LoadContent();

            LogList.ItemsSource = LogRecords.Any() ? LogRecords : defaultContent;
            LogList.ContextMenu = new ContextMenu();
            var menuItem = new MenuItem { Header = "Copy", };
            menuItem.Click += MenuItem_OnClick;
            LogList.ContextMenu.Items.Add(menuItem);

            LogList.SelectionChanged += OnLogListOnSelectionChanged;
            LogList.SelectedItem = LogList.Items[^1];
            LogList.ScrollIntoView(LogList.Items[^1]);

            FileSystemWatcher = new FileSystemWatcher(@"c:\_sphereDumps\", "mixed");
            FileSystemWatcher.Changed += (_, _) => { Dispatcher.BeginInvoke(LoadContent); };
            FileSystemWatcher.EnableRaisingEvents = true;
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
        }

        public void LoadContent()
        {
            var retryCount = 0;
            while (retryCount < 10)
            {
                try
                {
                    var textContent = File.ReadAllLines(@"c:\_sphereDumps\mixed");
                    for (var i = CurrentStreamPosition; i < textContent.Length; i++)
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

                    CurrentStreamPosition = textContent.Length;
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

        private void OnLogListOnSelectionChanged(object sender, SelectionChangedEventArgs args)
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
            }
            catch
            {
                LogRecordTextDisplay.Text = "Selected value is not a hex string!";
                IsFavorite.IsChecked = false;
            }
        }

        private void TrySetCurrentTextContent()
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

        public static string GetBinaryPaddedString(byte b) => Convert.ToString(b, 2).PadLeft(8, '0');

        // 0xAD is a soft hyphen and doesn't render, but it's not a control/whitespace/separator
        public static char GetVisibleChar(char c) =>
            char.IsControl(c) || char.IsWhiteSpace(c) || char.IsSeparator(c) || c == 0xAD ? '·' : c;

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
            return $"{GetBinaryPaddedString(b),9}{b.ToString(),7}{b,7:X}h" +
                   $"{GetEncoded1251Char(b).ToString(),7}{GetEncodedLoginChar(b).ToString(),7}";
        }

        public string ToReadableBinaryString()
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

        private void Shift_OnClick(object sender, RoutedEventArgs e)
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
                    shift = (button.Name[^1] - '0');
                    break;
            }

            CurrentShift = shift;
            TrySetCurrentTextContent();
        }

        private void StartByte_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(StartByte.Text, out int startByte))
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

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            CopySelectedRowContent();
        }

        private void CopySelectedRowContent()
        {
            var selectedRow = (LogRecord)LogList.SelectedItem;
            var text = $"{selectedRow.Origin}\t\t\t{selectedRow.Date}\t\t\t{selectedRow.Content}\n";
            Clipboard.SetText(text);
        }

        private void FavoriteToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (LogList.SelectedItem is null)
            {
                return;
            }

            var item = (LogRecord) LogList.SelectedItem;
            item.Favorite = true;
        }

        private void FavoriteToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (LogList.SelectedItem is null)
            {
                return;
            }

            var item = (LogRecord) LogList.SelectedItem;
            item.Favorite = false;
        }

        private void ShowFavoritesOnlyToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            ShowFavoritesOnly = true;
            CollectionViewSource.GetDefaultView(LogList.ItemsSource).Refresh();
            LogList.SelectedItem = LogList.Items[^1];
            LogList.ScrollIntoView(LogList.Items[^1]);
        }

        private void ShowFavoritesOnlyToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ShowFavoritesOnly = false;
            CollectionViewSource.GetDefaultView(LogList.ItemsSource).Refresh();
            LogList.SelectedItem = LogList.Items[^1];
            LogList.ScrollIntoView(LogList.Items[^1]);
        }
    }
}