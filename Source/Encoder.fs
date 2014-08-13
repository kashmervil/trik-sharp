namespace Trik
open System
open Trik
type Encoder(i2cCommandNumber) =
    inherit Helpers.PollingSensor<int>()
    override self.Read() = Helpers.I2C.Receive i2cCommandNumber
    member self.Reset() = Helpers.I2C.Send i2cCommandNumber
    interface IDisposable with
        member x.Dispose() = ()