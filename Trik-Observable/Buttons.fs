namespace Trik
open System
open System.IO
open System.Reactive.Linq
open System.Diagnostics
open Trik.Helpers

type Button (deviceFilePath) = 
    [<Literal>]
    let event_size = 16
    
    let stream = File.Open(deviceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read) 
    let mutable last = Array.create 3 0
    let bytes = Array.create event_size (byte 0)    
    let rec readFile _ =  
        let readCnt = stream.Read(bytes, 0, bytes.Length)
        if readCnt <> event_size then
            failwith "event reading error\n"
        else
            let evType = BitConverter.ToUInt16(bytes, 8)
            let evCode = BitConverter.ToUInt16(bytes, 10)
            let evValue = BitConverter.ToInt32(bytes, 12)
            printfn "evType: %A" evType
            if evType = 0us && evCode = 1us 
            then evValue
            else readFile 0
        
    member val ToObservable = Observable.Generate(readFile 0, Func<_,_> (konst true), Func<_,_> readFile , Func<_,_> id)
     