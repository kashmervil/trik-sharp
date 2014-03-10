namespace Trik

open System
open System.Reactive.Linq

type AnalogSensor(register) = 
    let read _ = 
        let value = Helpers.I2C.receive register |> Helpers.limit 0 1024 // well... sensors are only 10-bit, but nevertheless ... 
        value * 100 / 1024
    let mutable rate = 50
    member x.Read() = read 0
    member x.ToObservable(refreshRate: TimeSpan) = Observable.Generate(read(), Helpers.konst true, read, id
    , Helpers.konst refreshRate)
    member x.ToObservable() = x.ToObservable(TimeSpan.FromMilliseconds 50.)
   
    