using System.Windows;

namespace PacketLogViewer;

public partial class CreateKaitaiDialog : Window
{
    public CreateKaitaiDialog ()
    {
        InitializeComponent();
        KaitaiDefinitionName.Focus();
    }

    public string Name => KaitaiDefinitionName.Text;

    private void DialogOkButton_OnClick (object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}