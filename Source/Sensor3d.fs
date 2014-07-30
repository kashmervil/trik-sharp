namespace Trik

open System
open System.IO
open System.Diagnostics

type Sensor3d (min, max, deviceFilePath) as sens = 
    inherit FifoSensor<Point>(deviceFilePath, 16, 16)
    [<Literal>]
    let ev_abs = 3us
    let last = Array.zeroCreate 3
    do sens.ParseFunc <- fun bytes offset -> 
        let evType = BitConverter.ToUInt16(bytes, offset + 8)
        let evCode = BitConverter.ToUInt16(bytes, offset + 10)
        let evValue = BitConverter.ToInt32(bytes, offset + 12)
        if evType = ev_abs && evCode < 3us then 
            last.[int evCode] <- Helpers.limit min max evValue 
            None
        else
            Some {x = last.[0]; y = last.[1]; z = last.[2]}