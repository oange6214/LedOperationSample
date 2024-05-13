using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedOperationSample.Helplers;
using LedOperationSample.Mvvm.Models;
using LedOperationSample.Mvvm.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace LedOperationSample.Mvvm.ViewModels;

public partial class RecipeViewModel : ObservableRecipient
{

    #region Fields

    private readonly FileHelper _file;

    #endregion

    #region Properties

    [ObservableProperty] private object _pageContent;
    [ObservableProperty] private ModeModel _selectedModeItem;

    public ObservableCollection<ModeModel> ModeList { get; set; } = [];

    #endregion

    public RecipeViewModel()
    {
        WeakReferenceMessenger.Default.Register<ModeModel>(this, (o, s) =>
        {
            PageContent = null;
            ReadMode();
        });

        _file = new FileHelper("ModeFile");

        GetModes();
    }

    #region Commands

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
    private void RemoveMode()
    {
        if (ModeList.Count == 0)
        {
            MessageBox.Show("Cannot delete because list is empty.");
            return;
        }

        ModeList.Clear();
        _file.DeleteAllFiles();

        MessageBox.Show("All schemas have been successfully removed.");
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
            ModeList.Add(mode);
        }
    }


    #endregion

}
