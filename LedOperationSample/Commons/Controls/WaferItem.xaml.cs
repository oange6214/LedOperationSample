using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LedOperationSample.Commons.Controls;

public partial class WaferItem : UserControl
{
    public WaferItem()
    {
        InitializeComponent();
    }

    public string Slot
    {
        get { return (string)GetValue(SlotProperty); }
        set { SetValue(SlotProperty, value); }
    }

    public static readonly DependencyProperty SlotProperty =
        DependencyProperty.Register(nameof(Slot), typeof(string), typeof(WaferItem), new PropertyMetadata(string.Empty));

    public Brush Color
    {
        get { return (Brush)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }

    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(nameof(Color), typeof(Brush), typeof(WaferItem), new PropertyMetadata(new SolidColorBrush(Colors.DarkGray)));

}
