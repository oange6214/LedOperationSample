using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedOperationSample.Commons;
using LedOperationSample.Helplers;
using LedOperationSample.Mvvm.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace LedOperationSample.Mvvm.ViewModels;

public partial class EditModeViewModel : ObservableRecipient
{

    #region Fields

    private FileHelper _file;

    [ObservableProperty] private ModeModel _mode;

    #endregion

    #region Properties

    [ObservableProperty] private int _delay = 0;

    // Target-related properties
    [ObservableProperty] private int _selectedTargetIndex;
    [ObservableProperty] private TargetType _selectedTargetItem;
    public ObservableCollection<TargetType> TargetList { get; } = [];

    // Action-related properties
    public List<ActionType> ActionTypeList => GetActionTypes();

    [ObservableProperty] private ActionType _selectedActionTypeItem;
    [ObservableProperty] private ActionModel _selectedActionItem;
    public ObservableCollection<ActionModel> ActionModelList { get; set; } = [];

    // Step-related properties
    [ObservableProperty] private int _selectedStepIndex;
    [ObservableProperty] private StepModel _selectedStepItem;
    [ObservableProperty] private string _stepEditor;
    public ObservableCollection<StepModel> StepModelList { get; set; } = [];

    // Mode-related properties
    [ObservableProperty] private bool _isAutoGenName = true;
    [ObservableProperty] private int _modeCount = 2;
    [ObservableProperty] private string _modeName;
    [ObservableProperty] private int _selectedModeIndex;
    [ObservableProperty] private ModeModel _selectedModeItem;
    public ObservableCollection<ModeModel> ModeModelList { get; set; } = [];

    #endregion


    public EditModeViewModel(ModeModel mode)
    {
        _mode = mode;
        _file = new FileHelper("ModeFile");

        PropertyChanged += OnPropertyChanged;

        PopulateObservableCollection(Mode.Steps, StepModelList);
        SetConcatenatedString(Mode.Steps);
    }

    private void SetConcatenatedString(List<StepModel> steps)
    {
        StepEditor = string.Empty;

        foreach (var step in steps)
        {
            // Add to textbox
            string concatenatedString = BuildConcatenatedString(step.Actions) + Constants.LinkString;
            StepEditor += concatenatedString;
        }
    }

    #region Commands


    [RelayCommand]
    private void AddAction()
    {
        var action = CreateAction(
            SelectedTargetItem,
            SelectedActionTypeItem,
            SelectedActionTypeItem.ToString());

        ActionModelList.Add(action);
    }

    [RelayCommand]
    private void AddDelay()
    {
        if (Delay < Constants.MinimumDelay)
        {
            MessageBox.Show($"Need setting delay [{Constants.MinimumDelay} ~ N] (ms)");
            return;
        }

        var action = CreateAction(
            SelectedTargetItem,
            ActionType.Delay,
            Delay.ToString());

        ActionModelList.Add(action);
    }

    [RelayCommand]
    private void SaveAction()
    {
        if (SelectedStepItem is null)
        {
            MessageBox.Show("Cannot save because no step was selected.");
            return;
        }

        var count = ActionModelList.Count(a => a.Type == ActionType.Delay);

        if (count == 0)
        {
            MessageBox.Show("Unable to save because ACTION does not have Dely");
            return;
        }

        var step = Mode.Steps.First(step => step == SelectedStepItem);
        step.Actions = [.. ActionModelList];

        PopulateObservableCollection(Mode.Steps, StepModelList);
        ActionModelList.Clear();

        SetConcatenatedString(Mode.Steps);
    }

    [RelayCommand]
    private void RemoveAction()
    {
        if (SelectedActionItem is null)
        {
            MessageBox.Show("Cannot delete because no action was selected.");
            return;
        }

        ActionModelList.Remove(SelectedActionItem);
    }

    [RelayCommand]
    private void CaptureAction()
    {
        if (ActionModelList.Count == 0)
        {
            MessageBox.Show("Please set actions.");
            return;
        }

        if (!ActionModelList.Any(s => s.Type == ActionType.Delay))
        {
            MessageBox.Show("Please set delay.");
            return;
        }

        // Add to textbox
        string concatenatedString = BuildConcatenatedString(ActionModelList) + Constants.LinkString;
        StepEditor += concatenatedString;

        // Add listview
        var step = new StepModel
        {
            StepId = Guid.NewGuid(),
            Name = $"S_{Guid.NewGuid().ToString()[..^3]}",
            Actions = [.. ActionModelList]
        };
        StepModelList.Add(step);

        ClearActionList();
    }

    [RelayCommand]
    private void RemoveStep()
    {
        if (SelectedStepItem is null)
        {
            MessageBox.Show("Cannot delete because no step was selected.");
            return;
        }


        Mode.Steps.Remove(SelectedStepItem);

        StepModelList.Remove(SelectedStepItem);
        ActionModelList.Clear();
        SetConcatenatedString(Mode.Steps);
    }

    [RelayCommand]
    private void CloseView()
    {
        WeakReferenceMessenger.Default.Send<ModeModel>();
    }

    [RelayCommand]
    private async Task Confirm()
    {
        if (string.IsNullOrEmpty(StepEditor))
        {
            MessageBox.Show("Please Capture steps");
            return;
        }

        await _file.SaveAsync(Mode, $"{Mode.Name}.json");
        ModeModelList.Add(Mode);

        ClearModeDetails();

        CloseView();
    }

    #endregion

    #region Private Methods

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedModeItem):
                if (SelectedModeItem != null)
                {
                    PopulateObservableCollection(SelectedModeItem.Steps, StepModelList);
                }
                break;
            case nameof(SelectedStepItem):
                if (SelectedStepItem != null)
                {
                    PopulateObservableCollection(SelectedStepItem.Actions, ActionModelList);
                }
                break;
        }
    }

    private void PopulateObservableCollection<T>(IEnumerable<T> source, ObservableCollection<T> collection)
    {
        collection.Clear();
        foreach (var item in source)
        {
            collection.Add(item);
        }
    }

    private List<ActionType> GetActionTypes()
    {
        var actionTypes = Enum.GetValues(typeof(ActionType));

        var actionTypeList = new List<ActionType>();
        foreach (ActionType actionType in actionTypes)
        {
            if (actionType == ActionType.Delay)
            {
                continue;
            }
            actionTypeList.Add(actionType);
        }

        return actionTypeList;
    }

    private ActionModel CreateAction(
        TargetType targetType, 
        ActionType actionType, 
        string actionValue)
    {
        if (string.IsNullOrEmpty(actionValue))
        {
            MessageBox.Show("Please choose an action type.");
            return null;
        }

        return new ActionModel
        {
            ActionId = Guid.NewGuid(),
            Target = targetType,
            Type = actionType,
            Value = actionValue,
        };
    }

    private string BuildConcatenatedString(IEnumerable<ActionModel> actionModes)
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (var action in actionModes)
        {
            stringBuilder.Append($"{action.Type}::{action.Value}");
            stringBuilder.Append("+");
        }

        stringBuilder.Length--; // Remove the last "+" character

        return stringBuilder.ToString();
    }
    private void ClearActionList()
    {
        ActionModelList.Clear();
        Delay = 0;
    }

    private void ClearModeDetails()
    {
        ModeName = string.Empty;
        StepEditor = string.Empty;
    }

    #endregion
}
