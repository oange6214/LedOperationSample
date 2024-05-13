using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedOperationSample.Commons;
using LedOperationSample.Helplers;
using LedOperationSample.Mvvm.Models;
using LedOperationSample.Mvvm.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Media3D;

namespace LedOperationSample.Mvvm.ViewModels;

public partial class JobViewModel : ObservableRecipient
{

    #region Fields

    private readonly FileHelper _file;
    private CancellationTokenSource _cancellationTokenSource;

    #endregion

    #region Properties

    [ObservableProperty] private object _pageContent;
    [ObservableProperty] private ModeModel _selectedModeItem;

    public ObservableCollection<LightModel> Lights { get; set; } = [];
    public ObservableCollection<string> StateLogList { get; set; } = [];
    public ObservableCollection<ModeModel> ModeModelList { get; set; } = [];

    #endregion

    public JobViewModel()
    {
        WeakReferenceMessenger.Default.Register<ModeModel>(this, (o, s) =>
        {
            PageContent = null;
            ReadMode();
        });

        _file = new FileHelper("ModeFile");

        GeneralLightState();
        GetModes();
    }

    #region Commands

    [RelayCommand]
    private void ReadMode()
    {
        ModeModelList.Clear();

        var modes = _file.ReadAll<ModeModel>();

        if (modes.Count == 0)
        {
            MessageBox.Show("No modes found. Please create a mode.");
            return;
        }

        foreach (var mode in modes)
        {
            ModeModelList.Add(mode);
        }
    }    
    
    [RelayCommand]
    private void RemoveMode()
    {
        if (ModeModelList.Count == 0)
        {
            MessageBox.Show("Cannot delete because list is empty.");
            return;
        }

        ModeModelList.Clear();
        _file.DeleteAllFiles();

        MessageBox.Show("All schemas have been successfully removed.");
    }

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
    private void AddMode()
    {
        PageContent = new CreateModeView();
    }

    [RelayCommand]
    private void EditMode()
    {
        if (SelectedModeItem == null)
        {
            MessageBox.Show("No mode was selected.");
            return;
        }

        PageContent = new EditModeView()
        {
            DataContext = new EditModeViewModel(SelectedModeItem)
        };
    }

    #endregion

    #region Private Methods

    private void GetModes()
    {
        var modes = _file.ReadAll<ModeModel>();

        foreach (var mode in modes)
        {
            ModeModelList.Add(mode);
        }
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
