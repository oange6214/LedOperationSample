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

    [ObservableProperty] private string _value;

    // Target-related properties
    [ObservableProperty] private int _selectedTargetIndex;
    [ObservableProperty] private TargetType _selectedTargetItem;
    public ObservableCollection<TargetType> TargetList { get; } = [];

    // Action-related properties
    public ObservableCollection<ActionType> ActionTypeList => GetActionTypes();

    [ObservableProperty] private ActionType _selectedActionTypeItem;
    [ObservableProperty] private ActionModel _selectedActionItem;
    public ObservableCollection<ActionModel> ActionList { get; set; } = [];

    // Step-related properties
    [ObservableProperty] private int _selectedStepIndex;
    [ObservableProperty] private StepModel _selectedStepItem;
    [ObservableProperty] private string _stepEditor;
    public ObservableCollection<StepModel> StepList { get; set; } = [];

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

        TargetList = GetTargets();
        PopulateObservableCollection(Mode.Steps, StepList);
        SetConcatenatedString(Mode.Steps);
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

    [RelayCommand]
    private void SaveAction()
    {
        if (SelectedStepItem is null)
        {
            MessageBox.Show("Cannot save because no step was selected.");
            return;
        }

        var count = ActionList.Count(a => a.Type == ActionType.Delay);

        if (count == 0)
        {
            MessageBox.Show("Unable to save because ACTION does not have Dely");
            return;
        }

        var step = Mode.Steps.First(step => step == SelectedStepItem);
        step.Actions = [.. ActionList];

        PopulateObservableCollection(Mode.Steps, StepList);
        ActionList.Clear();

        SetConcatenatedString(Mode.Steps);

        ClearActionList();
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
    private void RemoveStep()
    {
        if (SelectedStepItem is null)
        {
            MessageBox.Show("Cannot delete because no step was selected.");
            return;
        }


        Mode.Steps.Remove(SelectedStepItem);

        StepList.Remove(SelectedStepItem);
        ActionList.Clear();
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

    private ObservableCollection<TargetType> GetTargets()
    {
        var targetTypes = Enum.GetValues(typeof(TargetType))
                              .Cast<TargetType>();

        return new ObservableCollection<TargetType>(targetTypes);
    }

    private void SetConcatenatedString(List<StepModel> steps)
    {
        StepEditor = string.Empty;

        foreach (var step in steps)
        {
            // Add to textbox
            string concatenatedString = BuildConcatenatedString(step.Actions);
            StepEditor += concatenatedString;
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedModeItem):
                if (SelectedModeItem != null)
                {
                    PopulateObservableCollection(SelectedModeItem.Steps, StepList);
                }
                break;
            case nameof(SelectedStepItem):
                if (SelectedStepItem != null)
                {
                    PopulateObservableCollection(SelectedStepItem.Actions, ActionList);
                }
                break;
            case nameof(SelectedActionItem):
                if (SelectedActionItem != null)
                {
                    Value = SelectedActionItem.Value;
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

    private ObservableCollection<ActionType> GetActionTypes()
    {
        var actionTypes = Enum.GetValues(typeof(ActionType))
                              .Cast<ActionType>();

        return new ObservableCollection<ActionType>(actionTypes);
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
