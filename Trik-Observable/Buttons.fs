namespace Trik.Observable
open System
open System.IO
open System.Reactive.Linq
open System.Diagnostics
open Trik.Helpers
open Trik

type Button (config:Config.Provider.DomainTypes.Keys) = 
    [<Literal>]
    let event_size = 16
    
    let stream = File.Open(config.DeviceFile, FileMode.Open, FileAccess.Read, FileShare.Read) 
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
        
    member val Observable = Observable.Generate(readFile 0, (fun _ -> true), (fun x -> readFile x), fun x -> x)
     