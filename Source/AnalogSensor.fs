namespace Trik.Sensors
open System
open Trik
/// General analog sensor e.g IR. Output is value form 0 up to 1024
type AnalogSensor(register) = 
    inherit Internals.PollingSensor<int>()
    override self.Read() = Helpers.I2C.receive register |> Helpers.limit 0 1024 
    new (port : ISensorPort) = new AnalogSensor(port.I2CNumber) 
    interface IDisposable with
        member x.Dispose() = ()
   
    