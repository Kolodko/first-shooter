using System;
using UniRx;
using UnityEngine;

public interface IInputHandler : IDisposable
{
    void Initialize(Subject<Vector3> move, Subject<Vector2> look, Subject<Unit> shoot, Subject<Unit> menu);
    void Update();
}
