namespace Trik

open System
open System.Reactive.Linq

type AnalogSensor(register) as sens = 
    inherit Helpers.AbstractSensor<int>()
    do sens.Read <- fun () ->
        let value = Helpers.I2C.receive register |> Helpers.limit 0 1024 // well... sensors are only 10-bit, but nevertheless ... 
        value * 100 / 1024
   
    