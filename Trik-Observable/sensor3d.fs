namespace Trik

open System
open System.IO
open System.Reactive.Linq
open System.Diagnostics

type Sensor3d (min, max, deviceFilePath) as sens = 
    inherit Helpers.AbstractSensor<int*int*int>()
    [<Literal>] 
    let event_size = 16
    [<Literal>] 
    let ev_abs = 3us
    let mutable last = Array.zeroCreate 3
    let stream = File.Open(deviceFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read) 
    let bytes = Array.zeroCreate event_size     
    do sens.Read <- fun () -> 
        let readCnt = stream.Read(bytes, 0, bytes.Length)
        if readCnt <> event_size then
            failwith "event reading error\n"
        else
            let evType = BitConverter.ToUInt16(bytes, 8)
            let evCode = BitConverter.ToUInt16(bytes, 10)
            let evValue = BitConverter.ToInt32(bytes, 12)
            //printfn "evType: %A" evType
            if evType = ev_abs && evCode < 3us then 
                last.[int evCode] <- Helpers.limit min max evValue 
            (last.[0], last.[1], last.[2])
    interface IDisposable with
        member x.Dispose() = stream.Dispose()