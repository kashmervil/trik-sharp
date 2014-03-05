namespace Trik.Observable

open System
open System.Reactive.Linq
open Trik

type AnalogSensor(register, rate) = 
    let read _ = Helpers.I2C.receive register |> Helpers.percent 0 1024 
    member val Observable = Observable.Generate(read(), Helpers.konst true, read, id, Helpers.konst <| System.TimeSpan.FromMilliseconds (float rate))

    