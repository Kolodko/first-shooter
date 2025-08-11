using System;
using UniRx;
using UnityEngine;

namespace Game.Input
{
    public interface IInputService
    {
        IObservable<Vector3> MoveInput { get; }
        IObservable<Vector2> LookInput { get; }
        IObservable<Unit> ShootInput { get; }
        IObservable<Unit> OpenUpgradeMenu { get; }
        float MouseSensitivity { get; }
        void SetInputMode(InputMode mode);
    }
}
