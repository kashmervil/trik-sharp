namespace Trik.Observable

open System
open System.Reactive.Linq
open Trik

type AnalogSensor(number, rate) = 
    let analogMin = 0
    let analogMax = 1024
    let getAnalogSensorRegister = int
    let register = getAnalogSensorRegister number
    let read _ = 
        let value = Helpers.I2C.receive register |> Helpers.limit analogMin analogMax 
        (value - analogMin)
    let mutable obs:IObservable<_> = null            
    member this.Start() = obs <- Observable.Generate(read(), Helpers.konst true, read, id, Helpers.konst <| System.TimeSpan.FromMilliseconds (float rate))

    member this.Obs = obs
    member this.Range = analogMax - analogMin