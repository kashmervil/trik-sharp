namespace Trik
open System
open System.Threading

[<AbstractClass>]
type StringFifoSensor<'T>(path: string) as sens = 
    let observers = new ResizeArray<IObserver<'T> >()
    let obs = Trik.Observable.Create(fun observer -> 
        lock observers <| fun () -> observers.Add(observer)
        { new IDisposable with 
            member this.Dispose() = lock observers <| fun () -> observers.Remove(observer) |> ignore } )
    let obsNext (x: 'T) = lock observers <| fun () -> observers.ForEach(fun obs -> obs.OnNext(x) ) 
    let obsError e = lock observers <| fun () -> observers.ForEach(fun obs -> obs.OnError e ) 
    let obsCompleted _ = lock observers <| fun () -> observers.ForEach(fun obs -> obs.OnCompleted() ) 
    
    let mutable cts: CancellationTokenSource = new CancellationTokenSource(0)
    let mutable lastValue = Unchecked.defaultof<'T>
    let readValueEvent = new Event<'T>()
    
    let loop() = 
        
        let rec reading (stream: IO.StreamReader) = async {
                let line = stream.ReadLine()
                sens.Parse line |> Option.iter obsNext
                return! reading stream
            }
             
        async {
            try
                let stream = IO.File.Open(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                let streamReader = new IO.StreamReader(stream)
                let! _ = Async.StartChild(Async.TryCancelled(reading streamReader, obsCompleted))
                ()
            with e ->  eprintfn "FifoSensor %s %A" path e; obsError e
              }

    abstract Parse: string -> 'T option
    
    member self.Read() = 
        Async.AwaitObservable obs |> Async.RunSynchronously

    member self.Start() =
        if not cts.IsCancellationRequested then invalidOp "Second call of Start() without Stop()"
        cts <- new Threading.CancellationTokenSource()
        Async.Start(loop(), cancellationToken = cts.Token)
    
    member self.Stop() = 
        if cts <> null then cts.Cancel()
        obsCompleted()

    member self.ToObservable() = obs

    abstract Dispose: unit -> unit 
    default self.Dispose() = 
        cts.Cancel()

    interface IDisposable with
        member self.Dispose() = self.Dispose()