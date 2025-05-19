// SPDX-License-Identifier: GPL-3.0-only
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EldenRingTool;
using GeoGuesserBuilder.Commands;

namespace GeoGuesserBuilder.Services;

public class ERProcessService : INotifyPropertyChanged, IDisposable
{

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isGameRunning;
    public bool IsGameRunning
    {
        get => _isGameRunning;
        private set
        {
            if (_isGameRunning != value)
            {
                _isGameRunning = value;
                OnPropertyChanged(nameof(IsGameRunning));
            }
        }
    }

    private bool _playerLocationValid;
    public bool PlayerLocationValid
    {
        get => _playerLocationValid;
        private set
        {
            if (_playerLocationValid != value)
            {
                _playerLocationValid = value;
                OnPropertyChanged(nameof(PlayerLocationValid));
            }
        }
    }

    private (float, float, float, float, uint) _playerCoordinates;
    public (float, float, float, float, uint) PlayerCoordinates
    {
        get => _playerCoordinates;
        private set
        {
            if (_playerCoordinates != value)
            {
                _playerCoordinates = value;
                OnPropertyChanged(nameof(PlayerCoordinates));
                OnPropertyChanged(nameof(PlayerCoordinatesText));
            }
        }
    }

    public string PlayerCoordinatesText
        => $"[{_playerCoordinates.Item1:F2}, {_playerCoordinates.Item2:F2}, {_playerCoordinates.Item3:F2}]";

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public event Action<bool>? GameRunningStatusChanged;
    public event Action<(float X, float Y, float Z, float Rot, uint MapID), bool>? PlayerCoordinatesChanged;

    private ERProcess? _erProcess;
    private CancellationTokenSource _cancellationTokenSource = new();

    public bool IsProcessAttached => _erProcess != null;

    public ERProcessService()
    {
        _ = StartMonitoring(_cancellationTokenSource.Token);
    }

    public (float, float, float, float, uint) GetMapCoords()
    {
        var mapCoords = _erProcess!.getMapCoords();
        var mapID = TeleportHelper.mapIDString(mapCoords.Item5);
        Vector3 coords = new(mapCoords.Item1, mapCoords.Item2, mapCoords.Item3);
        Vector3 rotation = new(0, mapCoords.Item4, 0);
        //Debug.WriteLine($"coords: {coords.ToString()}");
        //Debug.WriteLine($"rotation: {rotation.ToString()}");
        //Debug.WriteLine($"mapID: {mapID.ToString()}");
        return mapCoords;
    }

    public async Task StartMonitoring(CancellationToken token)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(200));

        while (await timer.WaitForNextTickAsync(token))
        {
            try
            {
                bool isRunning = ERProcess.checkGameRunning();
                IsGameRunning = isRunning;

                if (isRunning)
                {
                    _erProcess ??= new ERProcess();

                    var coords = GetMapCoords();
                    PlayerCoordinates = coords;
                    PlayerLocationValid = !(coords.Item1 == 0
                                            && coords.Item2 == 0
                                            && coords.Item3 == 0
                                            && coords.Item4 == 0);
                }
                else
                {
                    _erProcess?.Dispose();
                    _erProcess = null;
                    PlayerLocationValid = false;
                }
            }
            catch
            {
                IsGameRunning = false;
                PlayerLocationValid = false;
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _erProcess?.Dispose();
    }
}
