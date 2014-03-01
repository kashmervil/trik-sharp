namespace Trik.Observable
open System
open System.IO
open System.Reactive.Linq
open System.Diagnostics
open Trik.Helpers


type Sensor3d (min, max, deviceFilePath, rate:int) = 
    [<Literal>]
    let event_size = 16
    [<Literal>]
    let ev_abs = 3us

    let stream = File.Open(deviceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read) 
    let mutable last = Array.create 3 0
    let bytes = Array.create event_size (byte 0)    
    let readFile _ =  
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
            (limit min max last.[0], limit min max last.[1], limit min max last.[2])
        
    member val Observable = Observable.Generate(readFile(), Trik.Helpers.konst true, readFile, id, 
                                                Trik.Helpers.konst <| System.TimeSpan.FromMilliseconds (float rate))
