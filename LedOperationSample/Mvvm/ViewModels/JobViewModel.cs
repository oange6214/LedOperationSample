using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedOperationSample.Commons;
using LedOperationSample.Helplers;
using LedOperationSample.Mvvm.Models;
using LedOperationSample.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
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

    public ObservableCollection<LightModel> LightSettingList { get; set; } = [];
    public ObservableCollection<LightModel> LightsList { get; set; } = [];
    public ObservableCollection<LightModel> ActiveLightListView { get; set; } = [];
    public ObservableCollection<string> StateLogList { get; set; } = [];
    public ObservableCollection<ModeModel> ModeList { get; set; } = [];
    public ObservableCollection<ModeModel> ActiveModeList { get; set; } = [];


    private readonly IObservable<IEnumerable<LightModel>> _activeLightsObservable;
    public IObservable<IEnumerable<LightModel>> ActiveLights => _activeLightsObservable;

    private readonly List<LightService> _lightViewModels = new List<LightService>();

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

        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        StateLogList.Clear();
        InitLightState(LightSettingList);

        try
        {
            await StartJob();
        }
        catch (OperationCanceledException e)
        {
            Debug.WriteLine($"Exception: {e.Message}");
        }

    }

    [RelayCommand]
    private void StopRun()
    {
        _cancellationTokenSource?.Cancel();

        InitLightState(LightSettingList);
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
        //InitLightState(LightSettingList);

        ActiveLightListView.Clear();
        StateLogList.Clear();
    }

    [RelayCommand]
    private void EngineItem(LightModel light)
    {
        light.Mode = SelectedEngineItem;

        var newLight = light.Copy();
        ActiveLightListView.Add(newLight);
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
            LightSettingList.Add(new LightModel
            {
                Tag = $"Led{i + 1}",
                IsLightOn = false
            });

            LightsList.Add(new LightModel
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

    //private async Task StartJob(CancellationToken cancellationToken)
    //{

    //    foreach(var light in ActiveLightListView)
    //    {
    //        var targetLight = LightsList.FirstOrDefault(l => l.Tag == light.Tag);

    //        foreach (var step in light.Mode.Steps)
    //        {
    //            StateLogList.Insert(0, $"[{DateTime.UtcNow}] Start step");

    //            if (cancellationToken.IsCancellationRequested)
    //            {
    //                return;
    //            }

    //            foreach (var action in step.Actions)
    //            {
    //                switch (action.Type)
    //                {
    //                    case ActionType.Turn:
    //                        if (action.Value == "ON")
    //                        {
    //                            targetLight.IsLightOn = true;
    //                        }
    //                        if (action.Value == "OFF")
    //                        {
    //                            targetLight.IsLightOn = false;
    //                        }
    //                        break;
    //                    case ActionType.Delay:
    //                        int delayTime = int.Parse(action.Value);
    //                        await Task.Delay(delayTime, cancellationToken);
    //                        break;
    //                }


    //                StateLogList.Insert(0, $"[{DateTime.UtcNow}] {targetLight.Tag}.{action.Type}.{action.Value}");
    //            }

    //            StateLogList.Insert(0, $"[{DateTime.UtcNow}] Finish step");
    //        }
    //    }
    //}

    public async Task StartJob()
    {
        foreach (var lLightService in _lightViewModels)
        {
            LightModel light = lLightService.Light;
            var mode = light.Mode;

            foreach (var step in mode.Steps)
            {
                foreach(var action in step.Actions)
                {
                    switch (action.Type)
                    {
                        case ActionType.Turn:
                            if (action.Value == "ON")
                            {
                                light.IsLightOn = true;
                            }
                            else if (action.Value == "OFF")
                            {
                                light.IsLightOn = false;
                            }
                            break;
                        case ActionType.Delay:
                            int delayTime = int.Parse(action.Value);
                            await Task.Delay(delayTime);
                            break;
                    }
                }

                lLightService.Light = light;
            }
        }
    }

    #endregion





}
