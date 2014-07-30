namespace Trik

open System
open Trik
open Helpers.Measures
type Encoder(i2cCommandNumber) as sens =
    inherit Helpers.PollingSensor<int>()
    do sens.ReadFunc <- fun () ->
        Helpers.I2C.Receive i2cCommandNumber
    member x.Reset() = 
        Helpers.I2C.Send i2cCommandNumber
    member x.Read() = sens.Read()
    interface IDisposable with
        member x.Dispose() = ()