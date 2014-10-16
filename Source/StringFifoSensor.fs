namespace Trik.Internals
open System
open System.Threading
open Trik

[<AbstractClass>]
type StringFifoSensor<'T>(path: string) as sens = 
    let notifier = new Notifier<'T>()
    let mutable cts: CancellationTokenSource = new CancellationTokenSource()
    do cts.Cancel()
    
    let loop() = 
        let rec reading (stream: IO.StreamReader) = async {
                let line = stream.ReadLine()
                sens.Parse line |> Option.iter notifier.OnNext
                return! reading stream
            }
             
        async {
            try
                let stream = IO.File.Open(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                let streamReader = new IO.StreamReader(stream)
                let! _ = Async.StartChild(Async.TryCancelled(reading streamReader, notifier.OnCompleted))
                ()
            with e ->  eprintfn "FifoSensor %s %A" path e; notifier.OnError e
                }

    abstract Parse: string -> 'T option
    
    member self.Read() = 
        if cts.IsCancellationRequested then invalidOp "Calling Read() before Start()"
        let computation = Async.AwaitObservable notifier.Publish
        Async.RunSynchronously(computation, cancellationToken = cts.Token)

    member self.Start() =
        if not cts.IsCancellationRequested then invalidOp "Second call of Start() without Stop()"
        cts <- new Threading.CancellationTokenSource()
        Async.Start(loop(), cancellationToken = cts.Token)
    
    member self.Stop() = 
        if cts <> null then cts.Cancel()
        notifier.OnCompleted()

    member self.ToObservable() = notifier.Publish

    abstract Dispose: unit -> unit 
    default self.Dispose() = 
        notifier.OnCompleted()
        cts.Cancel()

    interface IDisposable with
        member self.Dispose() = self.Dispose()