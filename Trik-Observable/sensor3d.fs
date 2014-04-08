namespace Trik

open System
open System.IO
open System.Reactive.Linq
open System.Diagnostics

module  Sensor3dHelpers =
    let createReader () =
      let last = ()
      fun () -> ()
    

type Sensor3d (min, max, deviceFilePath) as sens = 
    inherit Helpers.FifoSensor<int array>(deviceFilePath, 16, 16)
    [<Literal>]
    let ev_abs = 3us
    let last = Array.zeroCreate 3
    do sens.ParseFunc <- fun bytes offset -> 
        let evType = BitConverter.ToUInt16(bytes, offset + 8)
        let evCode = BitConverter.ToUInt16(bytes, offset + 10)
        let evValue = BitConverter.ToInt32(bytes, offset + 12)
        //printfn "evType: %A" evType
        if evType = ev_abs && evCode < 3us then 
            last.[int evCode] <- Helpers.limit min max evValue 
            None
        else
            Some [| last.[0]; last.[1]; last.[2] |]