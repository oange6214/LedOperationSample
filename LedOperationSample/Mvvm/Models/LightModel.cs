using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace LedOperationSample.Mvvm.Models;

public partial class LightModel : ObservableObject
{
    public string Tag { get; set; }

    [ObservableProperty]
    private bool _isLightOn;

    public ModeModel Mode { get; set; }

    public LightModel Copy()
    {
        var light = (LightModel)this.MemberwiseClone();

        light.Mode = Mode.Copy();

        return light;
    }
}
