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
    
    member self.Power 
        with get() = lastCommand
        and  set command =  
            let v = Helpers.limit -100 100 command 
            let range = if v < 0 then kind.zero - kind.min else kind.max - kind.zero                            
            let duty = (kind.zero + range * v / 100)     
            fd.Write(duty)
            
    member self.Zero() = fd.Write(0)
            
    interface IObserver<int> with
        member self.OnNext(command) = 
            if Math.Abs(lastCommand - command) > ServoMotor.observerEps
            then lastCommand <- command; self.Power <- command
            
        member self.OnError e = self.Power <- kind.stop
        member self.OnCompleted () = self.Power <- kind.stop

    interface IDisposable with
        member self.Dispose() =
            self.Zero()
            (fd :> IDisposable).Dispose()
            setOption "request" "0"
    