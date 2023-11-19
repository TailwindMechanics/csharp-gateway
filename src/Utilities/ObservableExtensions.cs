//path: src\Utilities\ObservableExtensions.cs

using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Neurocache.Utilities
{
    public static class ObservableExtensions
    {
        public static IObservable<T> Firstly<T>(this IObservable<T> source, Action firstAction)
        {
            bool isFirst = true;

            return Observable.Create<T>(observer =>
            {
                return source.Subscribe(
                    onNext: item =>
                    {
                        if (isFirst)
                        {
                            firstAction();
                            isFirst = false;
                        }

                        observer.OnNext(item);
                    },
                    onError: observer.OnError,
                    onCompleted: observer.OnCompleted);
            });
        }

        public static IObservable<T> TakeUntilDisposed<T>(this IObservable<T> source, IDisposable disposable)
        {
            return Observable.Create<T>(observer =>
            {
                var subscription = source.Subscribe(observer);
                return Disposable.Create(() =>
                {
                    subscription.Dispose();
                    disposable.Dispose();
                });
            });
        }
    }
}
