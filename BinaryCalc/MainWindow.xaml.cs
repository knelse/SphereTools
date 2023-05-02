using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BinaryCalc
{
    public enum EditingMode
    {
        BIN,
        OCT,
        DEC,
        HEX
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        public List<int> currentValueListBin { get; set; } = new();
        public List<int> currentValueListOct { get; set; } = new();
        public List<int> currentValueListDec { get; set; } = new();
        public List<int> currentValueListHex { get; set; } = new();
        public string currentValueBin { get; set; } = "";
        public string currentValueOct { get; set; } = "";
        public string currentValueDec { get; set; } = "";
        public string currentValueHex { get; set; } = "";

        public readonly SolidColorBrush basisButtonDisabledColor = new(new Color {R = 255, G = 255, B = 255, A = 50});
        public readonly SolidColorBrush basisButtonEnabledColor = new(new Color {R = 255, G = 255, B = 255, A = 100});
        public readonly SolidColorBrush numberButtonDisabledColor = new(new Color {R = 40, G = 40, B = 40, A = 255});
        public readonly SolidColorBrush numberButtonEnabledColor = new(new Color {R = 64, G = 64, B = 64, A = 255});
        public readonly SolidColorBrush hexNumberButtonDisabledColor = new(new Color {R = 30, G = 50, B = 50, A = 255});
        public readonly SolidColorBrush hexNumberButtonEnabledColor = new(new Color {R = 48, G = 80, B = 80, A = 255});

        public EditingMode EditingMode { get; set; } = EditingMode.HEX;

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
            var numValue = button?.Name[7..8]; // 01234567890ABCDEF
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentValueBin)));
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
    }
}