namespace LedOperationSample.Mvvm.Models;

public class ModeModel
{
    public Guid ModeId { get; set; }
    public string Name { get; set; }
    public List<StepModel> Steps { get; set; }
}
