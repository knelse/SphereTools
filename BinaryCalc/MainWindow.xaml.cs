using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
        public string currentValueBinStr { get; set; } = "0";
        public string currentValueOctStr { get; set; } = "0";
        public string currentValueDecStr { get; set; } = "0";
        public string currentValueHexStr { get; set; } = "0";

        public BigInteger currentValue;
        
        public BigInteger operandValue;

        public readonly SolidColorBrush basisButtonDisabledColor = new(new Color {R = 255, G = 255, B = 255, A = 50});
        public readonly SolidColorBrush basisButtonEnabledColor = new(new Color {R = 255, G = 255, B = 255, A = 100});
        public readonly SolidColorBrush numberButtonDisabledColor = new(new Color {R = 40, G = 40, B = 40, A = 255});
        public readonly SolidColorBrush numberButtonEnabledColor = new(new Color {R = 64, G = 64, B = 64, A = 255});
        public readonly SolidColorBrush hexNumberButtonDisabledColor = new(new Color {R = 30, G = 50, B = 50, A = 255});
        public readonly SolidColorBrush hexNumberButtonEnabledColor = new(new Color {R = 48, G = 80, B = 80, A = 255});

        public EditingMode EditingMode { get; set; } = EditingMode.BIN;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            PropertyChanged += OnPropertyChangedEventHandler;
            UpdateEditMode();
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void EditMode_OnClick(object sender, RoutedEventArgs e)
        {
            var button = e.Source as Button;
            var editModeValue = Enum.Parse<EditingMode>(button?.Name[12..] ?? string.Empty);
            if (editModeValue == EditingMode)
            {
                return;
            }

            EditingMode = editModeValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditingMode)));
        }

        private static BigInteger ConvertBasis(string input, EditingMode from)
        {
            return input.Aggregate(new BigInteger(), (temp, c) => temp * (int) from + (c < 'A' ? (c - '0') : (c - 'A' +
                10)));
        }

        private static string ToBinaryString(BigInteger bigInt)
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

        private void OnNumberPress(char number)
        {
            if (number == char.MaxValue)
            {
                return;
            }

            switch (EditingMode)
            {
                case EditingMode.BIN:
                    currentValueBinStr += number;
                    currentValue = ConvertBasis(currentValueBinStr, EditingMode.BIN);
                    break;
                case EditingMode.OCT:
                    currentValueOctStr += number;
                    currentValue = ConvertBasis(currentValueOctStr, EditingMode.OCT);
                    break;
                case EditingMode.DEC:
                    currentValueDecStr += number;
                    currentValue = BigInteger.Parse(currentValueDecStr, NumberStyles.Number);
                    break;
                case EditingMode.HEX:
                    currentValueHexStr += number;
                    currentValue = BigInteger.Parse(currentValueHexStr, NumberStyles.HexNumber);
                    break;
            }
            UpdateValueDisplay();
        }

        private void UpdateValueDisplay()
        {
            currentValueBinStr = ToBinaryString(currentValue);
            currentValueOctStr = ToOctalString(currentValue);
            currentValueDecStr = currentValue.ToString("D");
            currentValueHexStr = currentValue.ToString("X");

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentValueBinStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentValueOctStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentValueDecStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentValueHexStr)));
            
        }

        private void UpdateNumberButtons()
        {
            Number_0.Background = Number_0.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_1.Background = Number_1.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_2.Background = Number_2.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_3.Background = Number_3.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_4.Background = Number_4.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_5.Background = Number_5.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_6.Background = Number_6.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_7.Background = Number_7.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_8.Background = Number_8.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_9.Background = Number_9.IsEnabled ? numberButtonEnabledColor : numberButtonDisabledColor;
            Number_A.Background = Number_A.IsEnabled ? hexNumberButtonEnabledColor : hexNumberButtonDisabledColor;
            Number_B.Background = Number_B.IsEnabled ? hexNumberButtonEnabledColor : hexNumberButtonDisabledColor;
            Number_C.Background = Number_C.IsEnabled ? hexNumberButtonEnabledColor : hexNumberButtonDisabledColor;
            Number_D.Background = Number_D.IsEnabled ? hexNumberButtonEnabledColor : hexNumberButtonDisabledColor;
            Number_E.Background = Number_E.IsEnabled ? hexNumberButtonEnabledColor : hexNumberButtonDisabledColor;
            Number_F.Background = Number_F.IsEnabled ? hexNumberButtonEnabledColor : hexNumberButtonDisabledColor;
        }

        private void UpdateEditMode()
        {
            switch (EditingMode)
            {
                case EditingMode.BIN:
                    SelectedBasis_BIN.Visibility = Visibility.Visible;
                    SelectedBasis_OCT.Visibility = Visibility.Hidden;
                    SelectedBasis_DEC.Visibility = Visibility.Hidden;
                    SelectedBasis_HEX.Visibility = Visibility.Hidden;
                    EditingMode_BIN.Background = basisButtonEnabledColor;
                    EditingMode_OCT.Background = basisButtonDisabledColor;
                    EditingMode_DEC.Background = basisButtonDisabledColor;
                    EditingMode_HEX.Background = basisButtonDisabledColor;

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
                    SelectedBasis_BIN.Visibility = Visibility.Hidden;
                    SelectedBasis_OCT.Visibility = Visibility.Visible;
                    SelectedBasis_DEC.Visibility = Visibility.Hidden;
                    SelectedBasis_HEX.Visibility = Visibility.Hidden;
                    EditingMode_BIN.Background = basisButtonDisabledColor;
                    EditingMode_OCT.Background = basisButtonEnabledColor;
                    EditingMode_DEC.Background = basisButtonDisabledColor;
                    EditingMode_HEX.Background = basisButtonDisabledColor;

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
                    SelectedBasis_BIN.Visibility = Visibility.Hidden;
                    SelectedBasis_OCT.Visibility = Visibility.Hidden;
                    SelectedBasis_DEC.Visibility = Visibility.Visible;
                    SelectedBasis_HEX.Visibility = Visibility.Hidden;
                    EditingMode_BIN.Background = basisButtonDisabledColor;
                    EditingMode_OCT.Background = basisButtonDisabledColor;
                    EditingMode_DEC.Background = basisButtonEnabledColor;
                    EditingMode_HEX.Background = basisButtonDisabledColor;

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
                    SelectedBasis_BIN.Visibility = Visibility.Hidden;
                    SelectedBasis_OCT.Visibility = Visibility.Hidden;
                    SelectedBasis_DEC.Visibility = Visibility.Hidden;
                    SelectedBasis_HEX.Visibility = Visibility.Visible;
                    EditingMode_BIN.Background = basisButtonDisabledColor;
                    EditingMode_OCT.Background = basisButtonDisabledColor;
                    EditingMode_DEC.Background = basisButtonDisabledColor;
                    EditingMode_HEX.Background = basisButtonEnabledColor;

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

        private void Operator_Clear_OnClick(object sender, RoutedEventArgs e)
        {
            currentValue = BigInteger.Zero;
            operandValue = BigInteger.Zero;
            UpdateValueDisplay();
        }
    }
}