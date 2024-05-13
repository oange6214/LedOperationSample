namespace LedOperationSample.Mvvm.Models;

public class StepModel
{
    public Guid StepId { get; set; }
    public string Name { get; set; }
    public List<ActionModel> Actions { get; set; }
}
