using System;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GeoGuesserBuilder.Commands;
using EldenRingTool;
using System.Windows;
using System.Linq;
using GeoGuesserBuilder.Services;
using System.Diagnostics;
using GeoGuesserBuilder.Converters;
using GeoGuesserBuilder.Models;
using System.IO.Compression;

namespace GeoGuesserBuilder.ViewModels;

public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    public ObservableCollection<GGLocationViewModel> CapturedLocations { get; } = new();
    public int CapturedLocationsCount => CapturedLocations.Count;

    public RelayCommand ExportCommand { get; }
    public RelayCommand ImportCommand { get; }
    public RelayCommand CaptureLocationCommand { get; }
    public RelayCommand LaunchEldenRingCommand { get; }
    public RelayCommand<GGLocationViewModel> DeleteLocationCommand { get; }
    public RelayCommand BuildModCommand { get; }
    public RelayCommand LaunchModCommand { get; }
    public RelayCommand PackageModCommand { get; }

    public string GameStatusText => _erProcessService.IsGameRunning ? "Game running" : "Game not found";
    public string GameStatusIcon => _erProcessService.IsGameRunning ? "✔" : "✖";
    public string GameStatusColor => _erProcessService.IsGameRunning ? "Green" : "Red";

    public bool PlayerLocationValid => _erProcessService.PlayerLocationValid;
    public string PlayerCoordinatesText => _erProcessService.PlayerCoordinatesText;
    
    private string _newLocationName;
    public string NewLocationName
    {
        get => _newLocationName;
        set
        {
            if (_newLocationName != value)
            {
                _newLocationName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewLocationName)));
            }
        }
    }
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                BuildModCommand.RaiseCanExecuteChanged();
                PackageModCommand.RaiseCanExecuteChanged();
                LaunchModCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public enum ModBuildStatus
    {
        NotReady,      // No locations
        ReadyToBuild,  // Locations exist, but no mod built
        Built,         // Mod has been built successfully
        Dirty          // Locations changed since last build
    }
    private ModBuildStatus _modStatus = ModBuildStatus.NotReady;
    public ModBuildStatus ModStatus
    {
        get => _modStatus;
        private set
        {
            if (_modStatus != value)
            {
                _modStatus = value;
                OnPropertyChanged(nameof(ModStatus));
                RaiseModCommandCanExecuteChanged();
            }
        }
    }
    private List<GGLocationModel> _lastBuiltLocations = new();

    private readonly ProjectService _projectService;
    private readonly ERProcessService _erProcessService;
    private readonly MapEditorService _mapEditorService;
    private readonly PackageModService _packageModService;

    public MainViewModel(
        ProjectService projectService,
        ERProcessService erProcessService,
        MapEditorService mapEditorService,
        PackageModService packageModService)
    {
        _projectService = projectService;
        _erProcessService = erProcessService;
        _mapEditorService = mapEditorService;
        _packageModService = packageModService;

        _newLocationName = "";
        ExportCommand = new RelayCommand(ExportProject);
        ImportCommand = new RelayCommand(ImportProject);
        CaptureLocationCommand = new RelayCommand(
            CaptureLocation,
            () => _erProcessService.IsGameRunning && _erProcessService.PlayerLocationValid);
        DeleteLocationCommand = new RelayCommand<GGLocationViewModel>(DeleteLocation);
        BuildModCommand = new RelayCommand(BuildMod,
            () => ModStatus == ModBuildStatus.ReadyToBuild && !IsBusy);
        PackageModCommand = new RelayCommand(PackageMod, () => ModStatus == ModBuildStatus.Built && !IsBusy);
        LaunchEldenRingCommand = new RelayCommand(
            () => LaunchEldenRing("launchmod_eldenring.bat"),
            () => !_erProcessService.IsGameRunning);
        LaunchModCommand = new RelayCommand(
            () => LaunchEldenRing("launchmod_geoguesser.bat"),
            () => !_erProcessService.IsGameRunning && ModStatus == ModBuildStatus.Built && !IsBusy);

        SubscribeToERServiceEvents();
    }

    private void LaunchEldenRing(string fileName)
    {
        if (_erProcessService.IsGameRunning == true) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = ".\\Resources\\ModEngine-2.1.0.0-win64",
            UseShellExecute = true,
            CreateNoWindow = true
        });
    }

    private void CaptureLocation()
    {
        if (!_erProcessService.IsProcessAttached) return;

        var coords = _erProcessService.GetMapCoords();
        string name = string.IsNullOrWhiteSpace(NewLocationName) ? "Unnamed Location" : NewLocationName;
        CapturedLocations.Add(new GGLocationViewModel
        {
            Name = name,
            Coordinates = coords
        });
        OnPropertyChanged(nameof(CapturedLocationsCount));
        BuildModCommand.RaiseCanExecuteChanged();
        NewLocationName = "";
        OnLocationsChanged();
        _ = ExportProjectAsync();
    }

    private void DeleteLocation(GGLocationViewModel? location)
    {
        if (location != null && CapturedLocations.Contains(location))
        {
            CapturedLocations.Remove(location);
            OnPropertyChanged(nameof(CapturedLocationsCount));
            BuildModCommand.RaiseCanExecuteChanged();
            OnLocationsChanged();
        }
    }

    private async void BuildMod()
    {
        if (!CapturedLocations.Any()) return;
        IsBusy = true;

        try
        {
            await Task.Run(() => _mapEditorService.ModifyMaps(CapturedLocations.ToList()));
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Build failed: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        //_mapEditorService.ModifyMaps(CapturedLocations.ToList());

        _lastBuiltLocations = GGLocationConverter.ToModelList(CapturedLocations);
        ModStatus = ModBuildStatus.Built;
        IsBusy = false;
    }

    private void PackageMod()
    {
        IsBusy = true;
        _packageModService.PackageModFiles();
        IsBusy = false;
    }

    private void SubscribeToERServiceEvents()
    {
        _erProcessService.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ERProcessService.IsGameRunning):
                    OnPropertyChanged(nameof(GameStatusText));
                    OnPropertyChanged(nameof(GameStatusIcon));
                    OnPropertyChanged(nameof(GameStatusColor));
                    CaptureLocationCommand.RaiseCanExecuteChanged();
                    LaunchEldenRingCommand.RaiseCanExecuteChanged();
                    LaunchModCommand.RaiseCanExecuteChanged();
                    break;

                case nameof(ERProcessService.PlayerLocationValid):
                    CaptureLocationCommand.RaiseCanExecuteChanged();
                    break;

                case nameof(ERProcessService.PlayerCoordinates):
                case nameof(ERProcessService.PlayerCoordinatesText):
                    OnPropertyChanged(nameof(PlayerCoordinatesText));
                    break;
            }
        };
    }

    private void RaiseModCommandCanExecuteChanged()
    {
        BuildModCommand.RaiseCanExecuteChanged();
        LaunchModCommand.RaiseCanExecuteChanged();
        PackageModCommand.RaiseCanExecuteChanged();
    }

    private void OnLocationsChanged()
    {
        if (CapturedLocations.Count == 0)
            ModStatus = ModBuildStatus.NotReady;
        else if (ModStatus == ModBuildStatus.Built)
        {
            var current = GGLocationConverter.ToModelList(CapturedLocations);
            bool hasChanged = !_lastBuiltLocations.SequenceEqual(current);
            ModStatus = hasChanged ? ModBuildStatus.ReadyToBuild : ModBuildStatus.Built;
        }
        else
            ModStatus = ModBuildStatus.ReadyToBuild;
    }

    public void ExportProject()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "GeoGuesser Project (*.json)|*.json",
            FileName = "project.json"
        };

        if (dialog.ShowDialog() == true)
        {
            var project = new GGProject
            {
                Locations = GGLocationConverter.ToModelList(CapturedLocations)
            };
            _projectService.SaveProject(project, dialog.FileName);
        }
    }

    public void ImportProject()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "GeoGuesser Project (*.json)|*.json"
        };

        if (dialog.ShowDialog() == true)
        {
            var project = _projectService.LoadProject(dialog.FileName);

            if (project != null)
            {
                CapturedLocations.Clear();
                foreach (var vm in GGLocationConverter.ToViewModelList(project.Locations))
                {
                    CapturedLocations.Add(vm);
                }

                OnPropertyChanged(nameof(CapturedLocationsCount));
                BuildModCommand.RaiseCanExecuteChanged();
                OnLocationsChanged();
            }
        }
    }

    public async Task ExportProjectAsync()
    {
        var project = new GGProject
        {
            Locations = GGLocationConverter.ToModelList(CapturedLocations),
        };

        await _projectService.SaveProjectAsync(project);
    }

    public async Task ImportProjectAsync()
    {
        var project = await _projectService.LoadProjectAsync();
        if (project == null) return;

        CapturedLocations.Clear();
        foreach (var vm in GGLocationConverter.ToViewModelList(project.Locations))
        {
            CapturedLocations.Add(vm);
        }

        OnPropertyChanged(nameof(CapturedLocationsCount));
        BuildModCommand.RaiseCanExecuteChanged();
        OnLocationsChanged();
    }

    public void Dispose()
    {
        _erProcessService?.Dispose();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
