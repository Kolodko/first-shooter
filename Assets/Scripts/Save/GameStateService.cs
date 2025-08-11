using System;
using Game.Enemy;
using Game.Installers;
using UniRx;
using UnityEngine;
using Zenject;

public class GameStateService : IInitializable, IDisposable
{
    [Inject] private SignalBus _signalBus;
    [Inject] private IEnemySpawnService _enemySpawnService;
        
    private CompositeDisposable _disposables = new CompositeDisposable();

    public void Initialize()
    {
        _signalBus.GetStream<GameInstaller.PlayerDeathSignal>()
            .Subscribe(_ => OnPlayerDeath())
            .AddTo(_disposables);
    }

    private void OnPlayerDeath()
    {
        _enemySpawnService.StopSpawning();
        Time.timeScale = 0.5f;
        
        Observable.Timer(TimeSpan.FromSeconds(2f))
            .Subscribe(_ => ShowGameOverUI())
            .AddTo(_disposables);
    }

    private void ShowGameOverUI()
    {
        Time.timeScale = 0f;
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
