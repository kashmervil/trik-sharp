namespace Trik.Sensors
open System
open Trik
open Trik.Helpers
type Encoder(i2cCommandNumber) =
    inherit Internals.PollingSensor<int>()
    override self.Read() = int(int16(I2C.receive i2cCommandNumber))
    member self.Reset() = I2C.send i2cCommandNumber 0 2
    interface IDisposable with
        member x.Dispose() = ()