using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace BinaryCalc
{
    public enum EditingMode
    {
        BIN = 2,
        OCT = 8,
        DEC = 10,
        HEX = 16
    }

    /// <summary>
    /// This should be MVVM but I'm lazy
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        public string CurrentValueBinStr { get; set; } = "0";
        public string CurrentValueOctStr { get; set; } = "0";
        public string CurrentValueDecStr { get; set; } = "0";
        public string CurrentValueHexStr { get; set; } = "0";

        public BigInteger CurrentValue;
        public BigInteger OperandValue;

        public readonly SolidColorBrush BasisButtonDisabledColor = new(new Color { R = 255, G = 255, B = 255, A = 50 });
        public readonly SolidColorBrush BasisButtonEnabledColor = new(new Color { R = 255, G = 255, B = 255, A = 100 });
        public readonly SolidColorBrush NumberButtonDisabledColor = new(new Color { R = 40, G = 40, B = 40, A = 255 });
        public readonly SolidColorBrush NumberButtonEnabledColor = new(new Color { R = 64, G = 64, B = 64, A = 255 });

        public readonly SolidColorBrush HexNumberButtonDisabledColor =
            new(new Color { R = 30, G = 50, B = 50, A = 255 });

        public readonly SolidColorBrush HexNumberButtonEnabledColor =
            new(new Color { R = 48, G = 80, B = 80, A = 255 });

        public readonly SolidColorBrush SelectedBasisBackgroundColor =
            new(new Color { R = 48, G = 48, B = 48, A = 255 });

        public readonly SolidColorBrush OtherBasisBackgroundColor = new(new Color { R = 48, G = 48, B = 48, A = 0 });

        public event PropertyChangedEventHandler? PropertyChanged;

        public EditingMode EditingMode { get; set; } = EditingMode.HEX;

        public static string CleanupInput(string input) =>
            input.ReplaceLineEndings("").Replace(" ", "").Replace("\t", "");

        public static int GetHexValue(char c) => c < 'A' ? c - '0' : c - 'A' + 10;

        public string GetCurrentEditedString =>
            EditingMode switch
            {
                EditingMode.BIN => CurrentValueBinStr,
                EditingMode.OCT => CurrentValueOctStr,
                EditingMode.DEC => CurrentValueDecStr,
                EditingMode.HEX => CurrentValueHexStr,
                _ => throw new ArgumentOutOfRangeException()
            };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            PropertyChanged += OnPropertyChangedEventHandler;
            UpdateEditMode();
            KeyDown += (_, args) =>
            {
                if (args.KeyboardDevice.Modifiers != ModifierKeys.None)
                {
                    return;
                }

                var key = args.Key;
                var keyStr = args.Key.ToString();
                if (keyStr.StartsWith("Numpad") || keyStr is ['D', _] ||
                    key is Key.A or Key.B or Key.C or Key.D or Key.E or Key.F)
                {
                    var numberValue = keyStr[^1];
                    OnNumberPress(numberValue);
                }
                else if (key is Key.Delete or Key.Clear)
                {
                    ClearDisplay();
                }
                else if (key is Key.Back)
                {
                    OnBackspacePress();
                }
            };

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPaste));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Find, (_, _) => CopyFormattedBinaryValue()));
        }

        private void OnPaste(object sender, ExecutedRoutedEventArgs args)
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }

            var content = CleanupInput("0" + Clipboard.GetText());
            var value = CurrentValue;

            try
            {
                value = ConvertBasis(content, EditingMode);
            }
            catch
            {
                // ignored
            }

            CurrentValue = value;
            UpdateValueDisplay();
        }

        private void OnPropertyChangedEventHandler(object? sender, PropertyChangedEventArgs args)
        {
            var propertyName = args.PropertyName;

            switch (propertyName)
            {
                case nameof(BinaryCalc.EditingMode):
                    UpdateEditMode();
                    break;
            }
        }

        private void Number_OnClick(object sender, RoutedEventArgs e)
        {
            var button = e.Source as Button;
            var numValue = button?.Name[7] ?? char.MaxValue; // 01234567890ABCDEF
            OnNumberPress(numValue);
        }

        private void OnNumberPress(char number)
        {
            if (number == char.MaxValue)
            {
                return;
            }

            var actualValue = GetHexValue(number);
            if (actualValue >= (int)EditingMode)
            {
                // e.g. pressed 9 in binary mode
                return;
            }

            CurrentValue = ConvertBasis(CleanupInput(GetCurrentEditedString + number), EditingMode);

            UpdateValueDisplay();
        }

        private void OnBackspacePress()
        {
            var currentStr = GetCurrentEditedString;
            if (currentStr.Length == 0 || currentStr is ['0'])
            {
                return;
            }

            CurrentValue = ConvertBasis(currentStr[..^1], EditingMode);

            UpdateValueDisplay();
        }

        private void Operator_Clear_OnClick(object sender, RoutedEventArgs e) => ClearDisplay();

        private string GetFormattedBinaryValue()
        {
            var text = CurrentValueBinStr.TrimStart('0');
            var length = text.Length;
            if (length % 8 != 0)
            {
                text = text.PadLeft(length / 8 * 8 + 8, '0');
            }

            var sb = new StringBuilder();
            for (var i = 0; i < text.Length; i += 8)
            {
                sb.AppendLine(text[i..(i + 8)]);
            }

            return sb.ToString();
        }

        private void CopyFormattedBinaryValue() => CopyToClipboardIfNotEmpty(GetFormattedBinaryValue());

        private void CopyValue_BIN_OnClick(object sender, RoutedEventArgs e) =>
            TrimAndCopyToClipboardIfNotEmpty(CurrentValueBinStr);

        private void CopyValue_BIN_WithBreaks_OnClick(object sender, RoutedEventArgs e) => CopyFormattedBinaryValue();

        private void CopyValue_BIN_WithBreaksAndHex_OnClick(object sender, RoutedEventArgs e) =>
            CopyToClipboardIfNotEmpty(CurrentValueHexStr.TrimStart('0') + "\n" + GetFormattedBinaryValue());

        private void CopyValue_OCT_OnClick(object sender, RoutedEventArgs e) =>
            CopyToClipboardIfNotEmpty(CurrentValueOctStr);

        private void CopyValue_DEC_OnClick(object sender, RoutedEventArgs e) =>
            CopyToClipboardIfNotEmpty(CurrentValueDecStr);

        private void CopyValue_HEX_OnClick(object sender, RoutedEventArgs e) =>
            CopyToClipboardIfNotEmpty(CurrentValueHexStr);

        private void Operator_LeftShift_OnClick(object sender, RoutedEventArgs e)
        {
            CurrentValue = ConvertBasis(CleanupInput(CurrentValueBinStr + "0"), EditingMode.BIN);

            UpdateValueDisplay();
        }

        private void Operator_RightShift_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentValueBinStr.Length == 0 || CurrentValueBinStr is ['0'])
            {
                return;
            }

            CurrentValue = ConvertBasis(CurrentValueBinStr[..^1], EditingMode.BIN);

            UpdateValueDisplay();
        }

        private void EditingMode_BIN_OnClick(object sender, RoutedEventArgs e) => ChangeEditingMode(EditingMode.BIN);

        private void EditingMode_OCT_OnClick(object sender, RoutedEventArgs e) => ChangeEditingMode(EditingMode.OCT);

        private void EditingMode_DEC_OnClick(object sender, RoutedEventArgs e) => ChangeEditingMode(EditingMode.DEC);

        private void EditingMode_HEX_OnClick(object sender, RoutedEventArgs e) => ChangeEditingMode(EditingMode.HEX);

        public static string ToBinaryString(BigInteger bigInt)
        {
            var bytes = bigInt.ToByteArray();
            var idx = bytes.Length - 1;
            var base2 = new StringBuilder(bytes.Length * 8);
            var binary = Convert.ToString(bytes[idx], 2);
            base2.Append(binary);
            for (idx--; idx >= 0; idx--)
            {
                base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));
            }

            return base2.ToString();
        }

        public static string ToOctalString(BigInteger bigInt)
        {
            var bytes = bigInt.ToByteArray();
            var idx = bytes.Length - 1;
            var base8 = new StringBuilder(((bytes.Length / 3) + 1) * 8);
            var extra = bytes.Length % 3;

            if (extra == 0)
            {
                extra = 3;
            }

            var int24 = 0;
            for (; extra != 0; extra--)
            {
                int24 <<= 8;
                int24 += bytes[idx--];
            }

            var octal = Convert.ToString(int24, 8);

            base8.Append(octal);

            for (; idx >= 0; idx -= 3)
            {
                int24 = (bytes[idx] << 16) + (bytes[idx - 1] << 8) + bytes[idx - 2];
                base8.Append(Convert.ToString(int24, 8).PadLeft(8, '0'));
            }

            return base8.ToString();
        }

        private void UpdateValueDisplay()
        {
            CurrentValueBinStr = ToBinaryString(CurrentValue);
            CurrentValueOctStr = ToOctalString(CurrentValue);
            CurrentValueDecStr = CurrentValue.ToString("D");
            CurrentValueHexStr = CurrentValue.ToString("X");

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValueBinStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValueOctStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValueDecStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValueHexStr)));
        }

        private void UpdateNumberButtons()
        {
            Number_0.Background = Number_0.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_1.Background = Number_1.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_2.Background = Number_2.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_3.Background = Number_3.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_4.Background = Number_4.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_5.Background = Number_5.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_6.Background = Number_6.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_7.Background = Number_7.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_8.Background = Number_8.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_9.Background = Number_9.IsEnabled ? NumberButtonEnabledColor : NumberButtonDisabledColor;
            Number_A.Background = Number_A.IsEnabled ? HexNumberButtonEnabledColor : HexNumberButtonDisabledColor;
            Number_B.Background = Number_B.IsEnabled ? HexNumberButtonEnabledColor : HexNumberButtonDisabledColor;
            Number_C.Background = Number_C.IsEnabled ? HexNumberButtonEnabledColor : HexNumberButtonDisabledColor;
            Number_D.Background = Number_D.IsEnabled ? HexNumberButtonEnabledColor : HexNumberButtonDisabledColor;
            Number_E.Background = Number_E.IsEnabled ? HexNumberButtonEnabledColor : HexNumberButtonDisabledColor;
            Number_F.Background = Number_F.IsEnabled ? HexNumberButtonEnabledColor : HexNumberButtonDisabledColor;
        }

        private void UpdateEditMode()
        {
            switch (EditingMode)
            {
                case EditingMode.BIN:
                    SelectedBasis_BIN.Background = SelectedBasisBackgroundColor;
                    SelectedBasis_OCT.Background = OtherBasisBackgroundColor;
                    SelectedBasis_DEC.Background = OtherBasisBackgroundColor;
                    SelectedBasis_HEX.Background = OtherBasisBackgroundColor;
                    EditingMode_BIN.Background = BasisButtonEnabledColor;
                    EditingMode_OCT.Background = BasisButtonDisabledColor;
                    EditingMode_DEC.Background = BasisButtonDisabledColor;
                    EditingMode_HEX.Background = BasisButtonDisabledColor;

                    Number_0.IsEnabled = true;
                    Number_1.IsEnabled = true;
                    Number_2.IsEnabled = false;
                    Number_3.IsEnabled = false;
                    Number_4.IsEnabled = false;
                    Number_5.IsEnabled = false;
                    Number_6.IsEnabled = false;
                    Number_7.IsEnabled = false;
                    Number_8.IsEnabled = false;
                    Number_9.IsEnabled = false;
                    Number_A.IsEnabled = false;
                    Number_B.IsEnabled = false;
                    Number_C.IsEnabled = false;
                    Number_D.IsEnabled = false;
                    Number_E.IsEnabled = false;
                    Number_F.IsEnabled = false;
                    break;
                case EditingMode.OCT:
                    SelectedBasis_BIN.Background = OtherBasisBackgroundColor;
                    SelectedBasis_OCT.Background = SelectedBasisBackgroundColor;
                    SelectedBasis_DEC.Background = OtherBasisBackgroundColor;
                    SelectedBasis_HEX.Background = OtherBasisBackgroundColor;
                    EditingMode_BIN.Background = BasisButtonDisabledColor;
                    EditingMode_OCT.Background = BasisButtonEnabledColor;
                    EditingMode_DEC.Background = BasisButtonDisabledColor;
                    EditingMode_HEX.Background = BasisButtonDisabledColor;

                    Number_0.IsEnabled = true;
                    Number_1.IsEnabled = true;
                    Number_2.IsEnabled = true;
                    Number_3.IsEnabled = true;
                    Number_4.IsEnabled = true;
                    Number_5.IsEnabled = true;
                    Number_6.IsEnabled = true;
                    Number_7.IsEnabled = true;
                    Number_8.IsEnabled = false;
                    Number_9.IsEnabled = false;
                    Number_A.IsEnabled = false;
                    Number_B.IsEnabled = false;
                    Number_C.IsEnabled = false;
                    Number_D.IsEnabled = false;
                    Number_E.IsEnabled = false;
                    Number_F.IsEnabled = false;
                    break;
                case EditingMode.DEC:
                    SelectedBasis_BIN.Background = OtherBasisBackgroundColor;
                    SelectedBasis_OCT.Background = OtherBasisBackgroundColor;
                    SelectedBasis_DEC.Background = SelectedBasisBackgroundColor;
                    SelectedBasis_HEX.Background = OtherBasisBackgroundColor;
                    EditingMode_BIN.Background = BasisButtonDisabledColor;
                    EditingMode_OCT.Background = BasisButtonDisabledColor;
                    EditingMode_DEC.Background = BasisButtonEnabledColor;
                    EditingMode_HEX.Background = BasisButtonDisabledColor;

                    Number_0.IsEnabled = true;
                    Number_1.IsEnabled = true;
                    Number_2.IsEnabled = true;
                    Number_3.IsEnabled = true;
                    Number_4.IsEnabled = true;
                    Number_5.IsEnabled = true;
                    Number_6.IsEnabled = true;
                    Number_7.IsEnabled = true;
                    Number_8.IsEnabled = true;
                    Number_9.IsEnabled = true;
                    Number_A.IsEnabled = false;
                    Number_B.IsEnabled = false;
                    Number_C.IsEnabled = false;
                    Number_D.IsEnabled = false;
                    Number_E.IsEnabled = false;
                    Number_F.IsEnabled = false;
                    break;
                case EditingMode.HEX:
                    SelectedBasis_BIN.Background = OtherBasisBackgroundColor;
                    SelectedBasis_OCT.Background = OtherBasisBackgroundColor;
                    SelectedBasis_DEC.Background = OtherBasisBackgroundColor;
                    SelectedBasis_HEX.Background = SelectedBasisBackgroundColor;
                    EditingMode_BIN.Background = BasisButtonDisabledColor;
                    EditingMode_OCT.Background = BasisButtonDisabledColor;
                    EditingMode_DEC.Background = BasisButtonDisabledColor;
                    EditingMode_HEX.Background = BasisButtonEnabledColor;

                    Number_0.IsEnabled = true;
                    Number_1.IsEnabled = true;
                    Number_2.IsEnabled = true;
                    Number_3.IsEnabled = true;
                    Number_4.IsEnabled = true;
                    Number_5.IsEnabled = true;
                    Number_6.IsEnabled = true;
                    Number_7.IsEnabled = true;
                    Number_8.IsEnabled = true;
                    Number_9.IsEnabled = true;
                    Number_A.IsEnabled = true;
                    Number_B.IsEnabled = true;
                    Number_C.IsEnabled = true;
                    Number_D.IsEnabled = true;
                    Number_E.IsEnabled = true;
                    Number_F.IsEnabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateNumberButtons();
        }

        private static BigInteger ConvertBasis(string input, EditingMode from)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return BigInteger.Zero;
            }

            return from switch
            {
                EditingMode.DEC => BigInteger.Parse(input, NumberStyles.Number),
                EditingMode.HEX => BigInteger.Parse(input, NumberStyles.HexNumber),
                _ => input.Aggregate(new BigInteger(), (temp, c) => temp * (int)from + GetHexValue(c))
            };
        }

        private void ClearDisplay()
        {
            CurrentValue = BigInteger.Zero;
            OperandValue = BigInteger.Zero;
            UpdateValueDisplay();
        }

        private void CopyToClipboardIfNotEmpty(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            Clipboard.SetText(input);
        }

        private void TrimAndCopyToClipboardIfNotEmpty(string input) => CopyToClipboardIfNotEmpty(input.TrimStart('0'));

        private void ChangeEditingMode(EditingMode to)
        {
            if (to == EditingMode)
            {
                return;
            }

            EditingMode = to;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditingMode)));
        }
    }
}