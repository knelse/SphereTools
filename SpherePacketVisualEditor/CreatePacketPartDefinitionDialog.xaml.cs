using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpherePacketVisualEditor;

public partial class CreatePacketPartDefinitionDialog
{
    public PacketPartType? PacketPartType;

    public CreatePacketPartDefinitionDialog (Color color)
    {
        InitializeComponent();
        ColorPicker.SetColor(color);
        var names = Enum.GetNames(typeof (PacketPartType)).Select(x => new PacketTypeComboBoxItem
        {
            Name = x
        });
        PacketPartName.Text = $"new_part_{Random.Shared.Next(0, 1000)}";
        PacketPartTypeComboBox.ItemsSource = names;
        PacketPartTypeComboBox.SelectedIndex = 0;

        PacketPartName.Focus();
    }

    public string Name => PacketPartName.Text;
    public Color Color => ColorPicker.Color;

    private void DialogOkButton_OnClick (object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PacketPartName.Text))
        {
            MessageBox.Show("Please input name");
            return;
        }

        DialogResult = true;
    }

    private void PacketPartTypeComboBox_OnSelectionChanged (object sender, SelectionChangedEventArgs e)
    {
        var selected = (PacketTypeComboBoxItem) PacketPartTypeComboBox.SelectedItem;
        if (selected is null)
        {
            return;
        }

        PacketPartType = Enum.Parse<PacketPartType>(selected.Name);
    }
}

public class PacketTypeComboBoxItem
{
    public string Name { get; set; }
}