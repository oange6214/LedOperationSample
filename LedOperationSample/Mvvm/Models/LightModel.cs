using CommunityToolkit.Mvvm.ComponentModel;

namespace LedOperationSample.Mvvm.Models;

public partial class LightModel : ObservableObject
{
    public string Tag { get; set; }

    [ObservableProperty]
    public bool _isLightOn;
}
