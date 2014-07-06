namespace Trik
open System
open System.Reactive.Linq

type FifoSensor<'T>(path: string, dataSize, bufSize) as sens = 
    let stream = IO.File.Open(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
    let observers = new ResizeArray<IObserver<'T> >()
    let obs = Observable.Create(fun observer -> 
        lock observers <| fun () -> observers.Add(observer) |> ignore 
        { new IDisposable with 
            member this.Dispose() = lock observers <| fun () -> observers.Remove(observer) |> ignore } )
    let obsNext (x: 'T) = lock observers <| fun () -> observers |> Seq.iter (fun obs -> obs.OnNext(x) ) 
    let bytes = Array.zeroCreate bufSize
    let bytesBlocking = Array.zeroCreate bufSize
    let mutable continueReading = false
    let mutable offset = 0
    let rec reading() = async {
        if continueReading then 
            let! readCnt = stream.AsyncRead(bytes, 0, bytes.Length)
            let blocks = readCnt / dataSize
            offset <- 0
            seq {1 .. blocks} |> 
                Seq.iter (fun _ -> 
                    sens.ParseFunc bytes offset |> Option.iter obsNext
                    offset <- offset + dataSize) 
            return! reading()
    }
    [<DefaultValue>]
    val mutable ParseFunc: (byte[] -> int -> 'T option)
    member x.BlockingRead() = 
        let cnt = stream.Read(bytesBlocking, offset, bufSize)
        sens.ParseFunc bytesBlocking offset
    member x.ToObservable() = 
        continueReading <- true 
        Async.Start <| reading()
        obs 
    interface IDisposable with
        member x.Dispose() = 
            continueReading <- false
            stream.Dispose()