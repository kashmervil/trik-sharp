namespace Trik
open System
open System.Threading

[<Sealed>]
type Notifier<'T>() =
    let observers = new ResizeArray<IObserver<'T> >()
    let source = { new IObservable<'T> with
        member self.Subscribe observer =  
            lock observers <| fun () -> observers.Add(observer)
            { new IDisposable with 
                member this.Dispose() = lock observers <| fun () -> observers.Remove(observer) |> ignore }
              }

    member self.OnError e = 
            lock (observers)
            <| fun () -> observers.ForEach(fun obs -> obs.OnError e)
            observers.Clear()

    member self.OnNext (x: 'T) = 
        try 
            lock (observers)
            <| fun () -> observers.ForEach(fun obs -> obs.OnNext x) 
        with e -> self.OnError e

    member self.OnCompleted _ = 
            lock (observers)
            <| fun () -> observers.ForEach(fun obs -> obs.OnCompleted()) 
            observers.Clear()
    member self.Publish with get() = source

    interface IDisposable with
        member self.Dispose() = self.OnCompleted()
    


[<AbstractClass; Sealed>]
type  Observable =   

    static member Create(subscription: Func<IObserver<'T>,IDisposable>) = 
        { new IObservable<'T> with
              member self.Subscribe observer = subscription.Invoke observer}

    static member DistinctUntilChanged(source: IObservable<'T> when 'T: equality) = 
        let hasCurrentKey = ref false
        let currentKey = ref Unchecked.defaultof<'T>
        let notifier = new Notifier<'T>()

        let inline helper x = lock currentKey
                              <| fun () -> currentKey := x
                              notifier.OnNext x

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
                                with e -> notifier.OnError e

                            member self.OnError e = notifier.OnError e
                            member self.OnCompleted() = notifier.OnCompleted()
                        }
        source.Subscribe(observer) |> ignore
        notifier.Publish


    static member Interval(timeSpan: System.TimeSpan) = 
        let counter = ref 0
        let notifier = new Notifier<int>()
        let cts = new CancellationTokenSource()

        let rec triggering (token: CancellationToken) = 
            async { 
                if not token.IsCancellationRequested then
                    notifier.OnNext !counter
                    incr counter
                    do! Async.Sleep(int timeSpan.Ticks/10000)
                    return! triggering token
                  }
        triggering cts.Token |> Async.Start
        notifier.Publish

    static member Subscribe(source : IObservable<'T>, observer: IObserver<'T>) = source.Subscribe(observer)
    static member Subscribe(source : IObservable<'T>, callback : 'T -> unit)= source.Subscribe(callback)
    static member Subscribe(source : IObservable<'T>, callback : Func<'T,unit>)= source.Subscribe(callback.Invoke)



module internal M = 
    let internal synchronize f = 
        let ctx = System.Threading.SynchronizationContext.Current 
        f (fun g ->
            let nctx = System.Threading.SynchronizationContext.Current 
            if ctx <> null && ctx <> nctx then ctx.Post((fun _ -> g()), null)
            else g() )

#nowarn "21"
#nowarn "40"
type Async =
    static member AwaitObservable(ev:IObservable<'T>) =
      M.synchronize (fun f ->
        Async.FromContinuations(fun (cont,econt,ccont) -> 
          let rec finish cont value = 
            remover.Dispose()
            f (fun () -> cont value)
          and remover : IDisposable = 
            ev.Subscribe
              ({ new IObserver<_> with
                   member x.OnNext(v) = finish cont v
                   member x.OnError(e) = finish econt e
                   member x.OnCompleted() = 
                      let msg = "Cancelling the workflow, because the Observable awaited using AwaitObservable has completed."
                      finish ccont (new System.OperationCanceledException(msg)) }) 
          () ))