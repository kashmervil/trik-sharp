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
    let Servo1 = {stop = 0; zero = 1500000; min = 1200000; max = 1800000; period = 20000000} 
    let Servo2 = {Servo1 with stop =1}
    let observerEps = 100



type Servomotor(servoPath: string, kind: ServoMotor.Kind) =
    let setOption target v = 
        IO.File.WriteAllText(sprintf "%s%c%s" servoPath IO.Path.DirectorySeparatorChar target, v)
    do [ ("0", "request") 
         ("1", "request")
         ("1", "run" )
         (string kind.period, "period_ns") ]
    |> List.iter (fun (v, f) -> setOption f v)
    
    let fd = new IO.StreamWriter(servoPath + "/duty_ns", AutoFlush = true)
    let mutable lastCommand = 0
    let mutable disposed = false
    member x.SetPower command = 
            let v = Helpers.limit -100 100 command 
            let range = if v < 0 then kind.zero - kind.min else kind.max - kind.zero                            
            let duty = (kind.zero + range * v / 100)     
            fd.Write(duty)
            
    member x.Zero() = fd.Write(0)
            
    interface IObserver<int> with
        member this.OnNext(command) = 
            if Math.Abs(lastCommand - command) > ServoMotor.observerEps
            then lastCommand <- command; this.SetPower command
            
        member this.OnError e = this.SetPower kind.stop
        member this.OnCompleted () = this.SetPower kind.stop

    interface IDisposable with
        member x.Dispose() =
            x.Zero()
            (fd:>IDisposable).Dispose()
            setOption "request" "0"
    