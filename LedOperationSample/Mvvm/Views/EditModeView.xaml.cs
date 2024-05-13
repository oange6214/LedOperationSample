using System.Windows.Controls;
using System.Windows.Input;

namespace LedOperationSample.Mvvm.Views;

public partial class EditModeView : UserControl
{
    public EditModeView()
    {
        InitializeComponent();


        Focusable = true;
        Loaded += (s, e) => Keyboard.Focus(this);
    }
}
