namespace Trik.Observable
open System
open System.IO
open System.Reactive.Linq
open System.Diagnostics
open Trik
open Trik.Helpers


type Sensor3d (min, max, deviceFilePath, rate:int<ms>) = 
    [<Literal>]
    let event_size = 16
    [<Literal>]
    let ev_abs = 3us

    let stream = File.Open(deviceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read) 
    let mutable last = Array.zeroCreate 3
    let bytes = Array.zeroCreate event_size     
    let readFile _ =  
        let readCnt = stream.Read(bytes, 0, bytes.Length)
        if readCnt <> event_size then
            failwith "event reading error\n"
        else
            let evType = BitConverter.ToUInt16(bytes, 8)
            let evCode = BitConverter.ToUInt16(bytes, 10)
            let evValue = BitConverter.ToInt32(bytes, 12)
            //printfn "evType: %A" evType
            if evType = ev_abs && evCode < 3us then 
                last.[int evCode] <- percent min max evValue 
            (last.[0], last.[1], last.[2])
        
    member val Observable = Observable.Generate(readFile(), konst true, readFile, id, 
                                                Trik.Helpers.konst <| System.TimeSpan.FromMilliseconds (float rate))

    interface IDisposable with
        member x.Dispose() = stream.Dispose()
