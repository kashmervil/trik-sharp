namespace Trik
open System
open System.Threading

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
                let readCnt = stream.Read(bytes, 0, bytes.Length)
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
                let stream = IO.File.Open(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                let! _ = Async.StartChild(Async.TryCancelled(reading stream, notifier.OnCompleted))
                ()
            with e ->  eprintfn "FifoSensor %s %A" path e; notifier.OnError e
              }
    
    new (path, dataSize, bufSize) = new BinaryFifoSensor<'T>(path, dataSize, bufSize, -1)
    abstract Parse: byte[] * int -> 'T option

    member self.Read() = 
        Async.RunSynchronously(Async.AwaitObservable obs, timeout)
        

    member self.Start() = 
        if not cts.IsCancellationRequested then invalidOp "Second call of Start() without Stop()"
        cts <- new Threading.CancellationTokenSource()
        Async.Start(loop(), cancellationToken = cts.Token)
    
    member self.Stop() = 
        if cts <> null then cts.Cancel()
        notifier.OnCompleted()

    member self.ToObservable() = obs

    interface IDisposable with
        member self.Dispose() = self.Stop()