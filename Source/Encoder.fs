namespace Trik

open System
open Trik
open Helpers.Measures
type Encoder(i2cCommandNumber) as sens =
    inherit Helpers.PollingSensor<int<tick>>()
    [<Literal>]
    let parToRad = 0.03272492f<rad>
    do sens.ReadFunc <- fun () ->
        Helpers.I2C.Receive i2cCommandNumber * 1<tick>
    member x.Reset() = 
        Helpers.I2C.Send i2cCommandNumber
    member x.ReadRadian() = parToRad * float32 (sens.Read())
    interface IDisposable with
        member x.Dispose() = ()