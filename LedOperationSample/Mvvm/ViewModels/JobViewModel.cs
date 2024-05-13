using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedOperationSample.Commons;
using LedOperationSample.Helplers;
using LedOperationSample.Mvvm.Models;
using LedOperationSample.Mvvm.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace LedOperationSample.Mvvm.ViewModels;

public partial class JobViewModel : ObservableRecipient
{

    #region Fields

    private readonly FileHelper _file;
    private CancellationTokenSource _cancellationTokenSource;

    #endregion

    #region Properties

    [ObservableProperty] private int _selectedEngineIndex;
    [ObservableProperty] private ModeModel _selectedEngineItem;
    [ObservableProperty] private LightModel _selectedLedItem;
    [ObservableProperty] private int _modeNum = 1;
    [ObservableProperty] private object _pageContent;
    [ObservableProperty] private ModeModel _selectedModeItem;
    [ObservableProperty] private List<ModeModel> _modeCombobox;

    public ObservableCollection<LightModel> Lights { get; set; } = [];
    public ObservableCollection<LightModel> LightListBox { get; set; } = [];
    public ObservableCollection<string> StateLogList { get; set; } = [];
    public ObservableCollection<ModeModel> ModeList { get; set; } = [];
    public ObservableCollection<ModeModel> ActiveModeList { get; set; } = [];

    #endregion

    public JobViewModel()
    {
        WeakReferenceMessenger.Default.Register<ModeModel>(this, (o, s) =>
        {
            PageContent = null;
            ReadMode();
        });

        _file = new FileHelper("ModeFile");

        PropertyChanged += OnPropertyChanged;

        GeneralLightState();
        GetModes();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ModeNum):
                if (ModeNum > 0)
                {
                    ModeCombobox = ModeList.Take(ModeNum).ToList();
                }
                break;
        }
    }

    #region Commands

    [RelayCommand]
    private async Task StartMode()
    {
        if (SelectedModeItem is null)
        {
            MessageBox.Show("Please choose an mode.");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        StateLogList.Clear();

        try
        {
            await StartJob(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Debug.WriteLine($"Exception: {e.Message}");
        }
    }

    [RelayCommand]
    private void StopMode()
    {
        _cancellationTokenSource?.Cancel();

        InitLightState(Lights);
    }

    [RelayCommand]
    private void ReadMode()
    {
        ModeList.Clear();

        var modes = _file.ReadAll<ModeModel>();

        if (modes.Count == 0)
        {
            MessageBox.Show("No modes found. Please create a mode.");
            return;
        }

        foreach (var mode in modes)
        {
            ModeList.Add(mode);
        }
    }    

    [RelayCommand]
    private void CleanUI()
    {
        //InitLightState(Lights);
        StateLogList.Clear();
    }

    [RelayCommand]
    private void EngineItem(LightModel light)
    {
        light.Mode = SelectedEngineItem.Copy();
        LightListBox.Add(light);
    }
    #endregion

    #region Private Methods

    private void GetModes()
    {
        var modes = _file.ReadAll<ModeModel>();

        foreach (var mode in modes)
        {
            ModeList.Add(mode);
        }

        ModeCombobox = ModeList.Take(ModeNum).ToList();
    }

    private void GeneralLightState()
    {
        for (int i = 0; i < 9; i++)
        {
            Lights.Add(new LightModel
            {
                Tag = $"Led{i + 1}",
                IsLightOn = false
            });
        }
    }

    private void InitLightState(IEnumerable<LightModel> lights)
    {
        foreach (var light in lights)
        {
            light.IsLightOn = false;
        }
    }

    private async Task StartJob(CancellationToken cancellationToken)
    {
        foreach (var step in SelectedModeItem.Steps)
        {
            StateLogList.Insert(0, $"[{DateTime.UtcNow}] Start step");

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            foreach (var action in step.Actions)
            {
                var light = Lights.First(l => l.Tag == action.Target.ToString());

                switch (action.Type)
                {
                    case ActionType.Turn:
                        if (action.Value == "ON")
                        {
                            light.IsLightOn = true;
                        }
                        if (action.Value == "OFF")
                        {
                            light.IsLightOn = false;
                        }
                        break;
                    case ActionType.Delay:
                        int delayTime = int.Parse(action.Value);
                        await Task.Delay(delayTime, cancellationToken);
                        break;
                }

 
                StateLogList.Insert(0, $"[{DateTime.UtcNow}] {light.Tag}.{action.Type}.{action.Value}");
            }

            StateLogList.Insert(0, $"[{DateTime.UtcNow}] Finish step");
        }
    }

    #endregion

}
