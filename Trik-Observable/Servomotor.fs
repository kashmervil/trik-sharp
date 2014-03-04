namespace Trik.Observable
open Trik
open System

open System
type Servomotor(servo:Config.Provider.DomainTypes.ServoMotor, types: Config.Provider.DomainTypes.ServoMotorTypes) =
    
    do using (new IO.StreamWriter(servo.PeriodFile)) <| fun f -> f.Write(servo.Period)
    let servoType = types.DefaultServo //TODO:!
    let off = servoType.Stop
    let zero = servoType.Zero
    let min = servoType.Min
    let max = servoType.Max
    let period = servo.Period
    
    let fd = new IO.StreamWriter(servo.DeviceFile)
    
    member x.SetPower power = 
            match power with 
                | None -> off 
                | Some (x:int<prcnt>) ->  
                            let range = if x < 0<prcnt> then zero - min else max - zero                            
                            let duty = (zero + range * x / 100<prcnt>) 
                            duty
            |> fd.Write
    
    interface IObserver<int<prcnt> option> with
        member this.OnNext(command) = this.SetPower command
            
        member this.OnError e = this.SetPower None
        member this.OnCompleted () = this.SetPower None

    interface IDisposable with
        member x.Dispose() =
            x.SetPower None
            (fd:>IDisposable).Dispose()
    
    