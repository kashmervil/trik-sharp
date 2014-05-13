namespace Trik

open System
open System.Reactive.Linq

type AnalogSensor(register) as sens = 
    inherit Helpers.PollingSensor<int>()
    do sens.ReadFunc <- fun () ->
        // well... sensors are only 10-bit, but nevertheless ... 
        let value = 
            Helpers.I2C.receive register 
            |> Helpers.limit 0 1024 
        value * 100 / 1024 
    interface IDisposable with
        member x.Dispose() = ()
   
    