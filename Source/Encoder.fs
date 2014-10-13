namespace Trik.Sensors
open System
open Trik
type Encoder(i2cCommandNumber) =
    inherit Internals.PollingSensor<int>()
    override self.Read() = Helpers.I2C.Receive i2cCommandNumber
    member self.Reset() = Helpers.I2C.Send i2cCommandNumber 0 1
    interface IDisposable with
        member x.Dispose() = ()