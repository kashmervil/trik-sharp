namespace Trik.Observable

open System
open System.Reactive.Linq
open Trik

type AnanalogSensor(register, rate) = 
    let read _ = 
        let value = Helpers.I2C.receive register |> Helpers.limit 0 1024 // well... sensors are only 10-bit, but nevertheless ... 
        value * 100 / 1024
    member val Observable = Observable.Generate(read(), Helpers.konst true, read, id, Helpers.konst <| System.TimeSpan.FromMilliseconds (float rate))
    member x.Read() = read 0