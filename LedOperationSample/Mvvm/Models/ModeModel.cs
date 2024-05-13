using System;

namespace LedOperationSample.Mvvm.Models;

public class ModeModel
{
    public Guid ModeId { get; set; }
    public string Name { get; set; }
    public List<StepModel> Steps { get; set; }

    public ModeModel Copy()
    {
        ModeModel mode = (ModeModel)this.MemberwiseClone();
        mode.Steps = new();

        for (int i = 0; i < Steps.Count; i++)
        {
            mode.Steps.Add(Steps[i].Copy());
        }

        return mode;
    }
}
