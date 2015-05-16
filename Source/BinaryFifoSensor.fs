namespace Trik.Internals
open System
open System.Threading
open Trik
open Trik.Reactive

[<AbstractClass>]
type BinaryFifoSensor<'T>(path, dataSize, bufSize, timeout) as sens = 
    
    let notifier = new Notifier<'T>()
    let obs = notifier.Publish
    //let _ = notifier.Publish |> Observable.subscribe (printfn "%A")
    let bytes = Array.zeroCreate bufSize
    let mutable cts: CancellationTokenSource = new CancellationTokenSource()
    do cts.Cancel()
    let mutable offset = 0

    let loop() = 
        let rec reading (stream: IO.FileStream) = 
            async {
                let! readAsync = Async.StartChild <| stream.AsyncRead(bytes, 0, bytes.Length)
                let! readCnt = readAsync
                let blocks = readCnt / dataSize
                offset <- 0
                for i = 1 to blocks do 
                        sens.Parse (bytes, offset) 
                        |> Option.iter notifier.OnNext
                        offset <- offset + dataSize 
                return! reading stream
                }
             
        async {
            try
                let stream = IO.File.Open(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
                let! _ = Async.StartChild(Async.TryCancelled(reading stream, notifier.OnCompleted))
                ()
            with e ->  eprintfn "FifoSensor %s %A" path e; notifier.OnError e
                }
    
    new (path, dataSize, bufSize) = new BinaryFifoSensor<'T>(path, dataSize, bufSize, -1)
    abstract Parse: byte[] * int -> 'T option

    member self.Read() = 
        if cts.IsCancellationRequested then invalidOp "Calling Read() before Start()"
        Async.RunSynchronously(Async.AwaitObservable obs, timeout)
        

    member self.Start() = 
        if not cts.IsCancellationRequested then invalidOp "Second call of Start() without Stop()"
        cts <- new Threading.CancellationTokenSource()
        Async.Start(loop(), cancellationToken = cts.Token)
        notifier.OnNext Unchecked.defaultof<'T>

    
    member self.Stop() = 
        if cts <> null then cts.Cancel()
        notifier.OnCompleted()

    member self.ToObservable() = obs

    abstract Dispose: unit -> unit
    default self.Dispose() = self.Stop()

    override self.Finalize() = self.Dispose()

    interface IDisposable with
        member self.Dispose() = self.Dispose()