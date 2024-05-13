using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedOperationSample.Commons;
using LedOperationSample.Helplers;
using LedOperationSample.Mvvm.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace LedOperationSample.Mvvm.ViewModels;

public partial class CreateModeViewModel : ObservableRecipient
{
    #region Fields

    private readonly FileHelper _file;

    #endregion

    #region Properties


    [ObservableProperty] private string _value;

    // Target-related properties
    [ObservableProperty] private TargetType _selectedTargetItem;
    public ObservableCollection<TargetType> TargetList { get; } = [];

    // Action-related properties
    [ObservableProperty] private ActionModel _selectedActionItem;
    [ObservableProperty] private ActionType _selectedActionTypeItem;
    public ObservableCollection<ActionType> ActionTypeList { get; } = [];
    public ObservableCollection<ActionModel> ActionList { get; } = [];


    // Step-related properties
    [ObservableProperty] private string _stepEditor;
    [ObservableProperty] private StepModel _selectedStepItem;
    public ObservableCollection<StepModel> StepModelList { get; } = [];


    // Mode-related properties
    [ObservableProperty] private bool _isAutoGenName = true;
    [ObservableProperty] private string _modeName;
    [ObservableProperty] private int _modeCount = 2;
    public ObservableCollection<ModeModel> ModeModelList { get; } = [];

    #endregion

    public CreateModeViewModel()
    {
        _file = new FileHelper("ModeFile");

        TargetList = GetTargets();
        ActionTypeList = GetActionTypes();
    }

    #region Commands
    [RelayCommand]
    private void AddAction()
    {
        var action = CreateAction(
            SelectedTargetItem,
            SelectedActionTypeItem,
            Value);

        ActionList.Add(action);
    }

    //[RelayCommand]
    //private void AddDelay()
    //{
    //    if (Delay < Constants.MinimumDelay)
    //    {
    //        MessageBox.Show($"Need setting delay [{Constants.MinimumDelay} ~ N] (ms)");
    //        return;
    //    }

    //    var action = CreateAction(
    //        SelectedTargetItem, 
    //        ActionType.Delay, 
    //        Delay.ToString());

    //    ActionList.Add(action);
    //}

    [RelayCommand]
    private void SaveAction()
    {
        if (SelectedStepItem is null)
        {
            MessageBox.Show("Cannot save because no step was selected.");
            return;
        }

        SelectedStepItem.Actions = ActionList.ToList();
        ActionList.Clear();
    }

    [RelayCommand]
    private void RemoveAction()
    {
        if (SelectedActionItem is null)
        {
            MessageBox.Show("Cannot delete because no action was selected.");
            return;
        }

        ActionList.Remove(SelectedActionItem);
    }

    [RelayCommand]
    private void CaptureAction()
    {
        if (ActionList.Count == 0)
        {
            MessageBox.Show("Please set actions.");
            return;
        }

        if (!ActionList.Any(s => s.Type == ActionType.Delay))
        {
            MessageBox.Show("Please set delay.");
            return;
        }

        string concatenatedString = BuildConcatenatedString(ActionList);

        var step = new StepModel
        {
            StepId = Guid.NewGuid(),
            Name = $"S_{Guid.NewGuid().ToString()[..^3]}",
            Actions = [.. ActionList]
        };

        StepModelList.Add(step);

        StepEditor += concatenatedString;

        ClearActionList();
    }

    [RelayCommand]
    private void CloseView()
    {
        WeakReferenceMessenger.Default.Send<ModeModel>();
    }

    [RelayCommand]
    private async Task Confirm()
    {
        var guid = Guid.NewGuid();
        var modeName = IsAutoGenName ? guid.ToString() : ModeName;

        if (string.IsNullOrEmpty(StepEditor))
        {
            MessageBox.Show("Please Capture steps");
            return;
        }

        if (!StepModelList.Any())
        {
            MessageBox.Show("Please set step.");
            return;
        }

        if (string.IsNullOrWhiteSpace(modeName))
        {
            MessageBox.Show("Please set Mode Name.");
            return;
        }

        var mode = new ModeModel
        {
            ModeId = guid,
            Name = modeName,
            Steps = [.. StepModelList]
        };

        await _file.SaveAsync(mode, $"{mode.Name}.json");

        ModeModelList.Add(mode);

        ClearModeDetails();
        CloseView();
    }

    #endregion

    #region Private Methods

    private ObservableCollection<TargetType> GetTargets()
    {
        var targetTypes = Enum.GetValues(typeof(TargetType))
                              .Cast<TargetType>();

        return new ObservableCollection<TargetType>(targetTypes);
    }
    private ObservableCollection<ActionType> GetActionTypes()
    {
        var actionTypes = Enum.GetValues(typeof(ActionType))
                              .Cast<ActionType>();

        return new ObservableCollection<ActionType>(actionTypes);
    }
    private ActionModel CreateAction(
        TargetType targetType, 
        ActionType actionType, 
        string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            MessageBox.Show("Please choose an action type.");
            return null;
        }

        return new ActionModel
        {
            ActionId = Guid.NewGuid(),
            Target = targetType,
            Type = actionType,
            Value = value,
        };
    }
    private string BuildConcatenatedString(IEnumerable<ActionModel> actions)
    {
        return string.Join("+", actions.Select(action => $"{action.Target}.{action.Type}")) + Constants.LinkString;
    }
    private void ClearActionList()
    {
        ActionList.Clear();
        Value = string.Empty;
    }
    private void ClearModeDetails()
    {
        ModeName = string.Empty;
        StepEditor = string.Empty;
    }

    #endregion
}
