namespace Trik

open System

/// General analog sensor e.g IR. Output is value form 0 up to 1024
type AnalogSensor(register) as sens = 
    inherit Helpers.PollingSensor<int>()
    do sens.ReadFunc <- fun () ->
        let value = 
            Helpers.I2C.Receive register 
            |> Helpers.limit 0 1024 
        value
    new (port : Ports.Sensor) = new AnalogSensor(port.ToI2CNumber()) 
    interface IDisposable with
        member x.Dispose() = ()
   
    