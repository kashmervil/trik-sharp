namespace Trik

open System
open System.Reactive.Linq

type AnalogSensor(register) = 
    let read _ = 
        let value = Helpers.I2C.receive register |> Helpers.limit 0 1024 // well... sensors are only 10-bit, but nevertheless ... 
        value * 100 / 1024
    let mutable rate = 50
    member x.Rate with get() = rate and set r = rate <- r
    member x.Read() = read 0
    member x.Observable = Observable.Generate(read(), Helpers.konst true, read, id
    , Helpers.konst <| System.TimeSpan.FromMilliseconds (float rate))
   
    