namespace Trik
open System
[<AbstractClass; Sealed>]
type  Observable =   

    static member Create(subscription) = { new IObservable<'T> with
                                    member self.Subscribe observer = subscription observer}

    static member DistinctUntilChanged (sequence: IObservable<'T> when 'T: equality) = 
        let prev = ref None
        let func x = if (!prev).IsNone || (!prev).Value <> x 
                        then prev := Some x; !prev
                        else None
        Observable.choose func sequence
    
    static member Interval(timeSpan: System.TimeSpan) = 
        let counter = ref 0
        let observers = new ResizeArray<IObserver<int>>()
        let obsNext _ = lock (observers) <|
                        fun () -> 
                            observers |> Seq.iter (fun obs -> obs.OnNext(!counter)) 
                            lock (counter) <|
                                fun () -> incr counter

        let timer = new Threading.Timer(obsNext, null, TimeSpan.Zero, timeSpan)

        let observable = {
            new IObservable<int> with
                member self.Subscribe observer = 
                    lock observers <| fun () -> observers.Add(observer)
                    { new IDisposable with 
                        member self.Dispose() = lock (observers) <|
                                                    fun () -> observers.Remove(observer) 
                                                    |> ignore 
                    }
            interface IDisposable with
                member self.Dispose() = 
                    timer.Dispose()
                    observers.Clear()
            }
        observable

    static member Subscribe(source : IObservable<'T>, observer: IObserver<'T>) = source.Subscribe(observer)
    static member Subscribe(source : IObservable<'T>, callback : 'T -> unit)= source.Subscribe(callback)
    static member Subscribe(source : IObservable<'T>, callback : Func<'T,unit>)= source.Subscribe(callback.Invoke)