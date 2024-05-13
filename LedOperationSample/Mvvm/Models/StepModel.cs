using System;

namespace LedOperationSample.Mvvm.Models;

public class StepModel
{
    public Guid StepId { get; set; }
    public string Name { get; set; }
    public List<ActionModel> Actions { get; set; }

    public StepModel Copy()
    {
        StepModel step = (StepModel)this.MemberwiseClone();
        step.Actions = new();

        for (int i = 0; i < Actions.Count; i++)
        {
            step.Actions.Add(Actions[i].Copy());
        }

        return step;
    }
}
