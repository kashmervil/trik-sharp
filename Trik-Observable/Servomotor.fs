namespace Trik

open System
module ServoMotor =
    type Kind = {
        min: int 
        max: int
        zero: int
        stop: int
        period:int
        }
    let Servo1 = {stop = 0; zero = 15000000; min = 0; max = 0; period = 20000000} 
    let Servo2 = {Servo1 with stop =1}

type Servomotor(servoPath: string, kind: ServoMotor.Kind) =
    
    do using (new IO.StreamWriter(servoPath + "/period_ns")) <| fun f -> f.Write(kind.period)
    
    let mutable lastCommand = 0
    let fd = new IO.StreamWriter(servoPath + "/duty_ns")
    
    member x.SetPower command = 
            let v = Helpers.limit -100 100 command 
            let range = if v < 0 then kind.zero - kind.min else kind.max - kind.zero                            
            let duty = (kind.zero + range * v / 100) 
            duty
            |> fd.Write
    
    interface IObserver<int> with
        member this.OnNext(command) = 
            if (lastCommand - command) > 2
            then lastCommand <- command; this.SetPower command
            
        member this.OnError e = this.SetPower kind.stop
        member this.OnCompleted () = this.SetPower kind.stop

    interface IDisposable with
        member x.Dispose() =
            x.SetPower kind.stop
            (fd:>IDisposable).Dispose()
    
    