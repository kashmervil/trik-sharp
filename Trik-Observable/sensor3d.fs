module Sensor3d

open System
open System.IO
open System.Reactive.Linq
open System.Diagnostics
let event_size = 16
let ev_abs = 3us

[<AllowNullLiteralAttribute>]
type Sensor3d (min, max, deviceFile, rate) = 
   
    let stream = File.Open(deviceFile, FileMode.Open) 
    let mutable last = Array.create 3 0
    let bytes = Array.create event_size (byte 0)    
    let readFile() =  
        let readCnt = stream.Read(bytes, 0, bytes.Length)
        if readCnt <> event_size then
            failwith "event reading error\n"
        else
            let evType = BitConverter.ToUInt16(bytes, 8)
            let evCode = BitConverter.ToUInt16(bytes, 10)
            let evValue = BitConverter.ToInt32(bytes, 12)
            //printfn "evType: %A" evType
            match evType with
            | x when x = ev_abs -> 
                match evCode with
                | 0us -> (last.[0] <- evValue)
                | 1us -> (last.[1] <- evValue)
                | 2us -> (last.[2] <- evValue)
                | _ -> ()
            | _ -> ()
            (last.[0], last.[1], last.[2])

    let mutable obs:IObservable<int*int*int> = null            
    member this.Start() = 
        (readFile(), readFile(), readFile(), readFile(), readFile(), readFile(), readFile(), readFile()) |> ignore
        let sw = new Stopwatch()
        sw.Start()
<<<<<<< HEAD
        obs <- Observable.Generate(0, Func<_,bool>(fun _ -> true), Func<int,int>(fun x -> x)
            , Func<int,_>(fun _ -> readFile()), Func<_,TimeSpan>(fun _ -> System.TimeSpan.FromMilliseconds(rate)))
=======
        obs <- Observable.Generate(0, (fun _ -> true), (fun x -> x)
            , (fun _ -> readFile()), (fun _ -> System.TimeSpan.FromMilliseconds(rate)))
>>>>>>> 81888b28557c3b5a607e00d5a3c28083e37f186a
        printfn "Observable.Generate: %A" sw.Elapsed
    member this.Obs = obs
