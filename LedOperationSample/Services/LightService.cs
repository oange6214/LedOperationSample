using LedOperationSample.Mvvm.Models;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LedOperationSample.Services;

public class LightService
{
    private readonly Subject<LightModel> _lightChangedSubject = new();
    public IObservable<LightModel> LightChanged => _lightChangedSubject.AsObservable();

    private LightModel _light;
    public LightModel Light
    {
        get => _light;
        set
        {
            _light = value;
            _lightChangedSubject.OnNext(value);
        }
    }

    public LightService(LightModel light)
    {
        Light = light;
    }
}
