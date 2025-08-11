using System;
using UniRx;
using Zenject;

public static class SignalBusExtensions
{
    public static IObservable<T> GetStream<T>(this SignalBus signalBus)
    {
        return Observable.FromEvent<T>(
            h => signalBus.Subscribe<T>(h),
            h => signalBus.TryUnsubscribe<T>(h)
        );
    }
}