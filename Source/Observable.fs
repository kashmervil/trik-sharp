namespace Trik
open System
[<AbstractClass; Sealed>]
type  Observable =   

    static member Create(subscription) = { new IObservable<'T> with
                                    member self.Subscribe observer = subscription observer}

    static member DistinctUntilChanged(source: IObservable<'T> when 'T: equality) = 
        let hasCurrentKey = ref false
        let currentKey = ref Unchecked.defaultof<'T>
        let observers = new ResizeArray<IObserver<'T>>()
        let result = Observable.Create(fun observer -> 
            lock (observers) 
            <| fun () -> observers.Add(observer)
            { new IDisposable with 
                member self.Dispose() = 
                    lock (observers) 
                    <| fun () -> observers.Remove(observer) |> ignore 
            })

        let inline obsError e = 
            lock (observers)
            <| fun () -> observers.ForEach(fun obs -> obs.OnError e)
            observers.Clear()

        let inline obsNext value = 
            try 
                lock (observers)
                <| fun () -> observers.ForEach(fun obs -> obs.OnNext value) 
            with e -> obsError e

        let inline obsCompleted() = 
            lock (observers)
            <| fun () -> observers.ForEach(fun obs -> obs.OnCompleted()) 
            observers.Clear()

        let inline helper x = lock currentKey
                              <| fun () -> currentKey := x
                              obsNext x

        let observer = { new System.IObserver<_> with
                            member self.OnNext(x) = 
                                try 
                                    if not !hasCurrentKey 
                                    then 
                                        hasCurrentKey := true
                                        helper x
                                    elif not(currentKey.Equals(x))
                                    then 
                                        helper x
                                with e -> obsError e

                            member self.OnError e = obsError e
                            member self.OnCompleted() = obsCompleted()
                        }
        source.Subscribe(observer) |> ignore
        result


    static member Interval(timeSpan: System.TimeSpan) = 
        let counter = ref 0
        let observers = new ResizeArray<IObserver<int>>()
        let obsNext _ = lock (observers) <|
                        fun () -> 
                            observers.ForEach(fun obs -> obs.OnNext(!counter)) 
                            lock (counter) <|
                                fun () -> incr counter

        let timer = new Threading.Timer(obsNext, null, TimeSpan.Zero, timeSpan)

        Observable.Create(fun observer -> 
            lock (observers) 
            <| fun () -> observers.Add(observer)
            { new IDisposable with 
                member self.Dispose() = 
                    lock (observers) 
                    <| fun () -> observers.Remove(observer) |> ignore 
            })


    static member Subscribe(source : IObservable<'T>, observer: IObserver<'T>) = source.Subscribe(observer)
    static member Subscribe(source : IObservable<'T>, callback : 'T -> unit)= source.Subscribe(callback)
    static member Subscribe(source : IObservable<'T>, callback : Func<'T,unit>)= source.Subscribe(callback.Invoke)