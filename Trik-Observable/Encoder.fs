namespace Trik

open System
open Trik

type Encoder(i2cCommandNumber) as sens =
    inherit Helpers.PollingSensor<float32>()
    [<Literal>]
    let parToRad = 0.03272492f
    do sens.ReadFunc <- fun () ->
        let data = Helpers.I2C.receive i2cCommandNumber
        parToRad * float32 (data)
    member x.Reset() = 
        Helpers.I2C.send i2cCommandNumber
    interface IDisposable with
        member x.Dispose() = ()