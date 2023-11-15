//path: Utilities\ObservableExtensions.cs

using System.Reactive.Linq;

namespace Neurocache.Gateway.Utilities
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
    }
}
