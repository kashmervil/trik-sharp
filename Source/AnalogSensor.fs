namespace Trik

open System

/// General analog sensor e.g IR. Output is value form 0 up to 1024
type AnalogSensor(register) = 
    inherit Helpers.PollingSensor<int>()
    override self.Read() = Helpers.I2C.Receive register |> Helpers.limit 0 1024 
    new (port : Ports.Sensor) = new AnalogSensor(port.I2CNumber) 
    interface IDisposable with
        member x.Dispose() = ()
   
    