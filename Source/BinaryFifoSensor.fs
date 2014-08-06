namespace Trik
open System

type Notifier<'T>() =
    let observers = new ResizeArray<IObserver<'T> >()
    let obs = Trik.Observable.Create(fun observer -> 
        lock observers <| fun () -> observers.Add(observer)
        { new IDisposable with 
            member this.Dispose() = lock observers <| fun () -> observers.Remove(observer) |> ignore })

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
    member self.Publish() = obs
    interface IDisposable with
        member self.Dispose() = self.OnCompleted()
    

[<AbstractClass>]
type BinaryFifoSensor<'T>(path, dataSize, bufSize) as sens = 
    
    let notifier = new Notifier<'T>()
    let bytes = Array.zeroCreate bufSize
    let mutable cts = null
    let mutable offset = 0
    let mutable lastValue = None
    
    let loop() = 
        let rec reading (stream: IO.FileStream) = 
            async {
                let readCnt = stream.Read(bytes, 0, bytes.Length)
                let blocks = readCnt / dataSize
                offset <- 0
                for i = 1 to blocks do 
                        sens.Parse (bytes, offset) 
                        |> Option.iter (fun x -> lastValue <- Some x; notifier.OnNext x)
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
        match lastValue with
        | None -> invalidOp "Read failed or missing Start() before Read()"
        | Some x -> x

    member self.Start() = 
        cts <- new Threading.CancellationTokenSource()
        Async.Start(loop(), cancellationToken = cts.Token)
    
    member self.Stop() = 
        if cts <> null then cts.Cancel()
        notifier.OnCompleted()

    member self.ToObservable() = notifier.Publish()

    abstract Dispose: unit -> unit 
    default self.Dispose() = 
        cts.Cancel()

    interface IDisposable with
        member self.Dispose() = self.Dispose()