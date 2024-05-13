using LedOperationSample.Commons;

namespace LedOperationSample.Mvvm.Models;

public class ActionModel
{
    public Guid ActionId { get; set; }
    public TargetType Target { get; set; }
    public ActionType Type { get; set; }
    public string Value { get; set; }

    public ActionModel Copy()
    {
        return (ActionModel)this.MemberwiseClone();
    }
}
