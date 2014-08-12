namespace Trik
open System
open System.Threading

[<AbstractClass>]
type BinaryFifoSensor<'T>(path, dataSize, bufSize) as sens = 
    
    let notifier = new Notifier<'T>()
    let bytes = Array.zeroCreate bufSize
    let mutable cts: CancellationTokenSource = new CancellationTokenSource(0)
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
    
    abstract Parse: byte[] * int -> 'T option

    member self.Read() = 
        Async.AwaitObservable notifier.Publish |> Async.RunSynchronously
        

    member self.Start() = 
        
        if not cts.IsCancellationRequested then invalidOp "Second call of Start() without Stop()"
        cts <- new Threading.CancellationTokenSource()
        Async.Start(loop(), cancellationToken = cts.Token)
    
    member self.Stop() = 
        if cts <> null then cts.Cancel()
        notifier.OnCompleted()

    member self.ToObservable() = notifier.Publish

    interface IDisposable with
        member self.Dispose() = cts.Cancel()