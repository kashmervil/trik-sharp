namespace Trik
open System

type ServoMotor(servoPath: string, kind: ServoMotor.Kind) =
    let setOption target v = 
        IO.File.WriteAllText(sprintf "%s%c%s" servoPath IO.Path.DirectorySeparatorChar target, v)
    do [ ("0", "request") 
         ("1", "request")
         ("1", "run" )
         (string kind.period, "period_ns") ]
    |> List.iter (fun (v, f) -> setOption f v)
    
    let fd = new IO.StreamWriter(servoPath + "/duty_ns", AutoFlush = true)
    let mutable lastCommand = 0
    
    member self.SetPower command =  
            lock self 
            <| fun () ->
                let v = Helpers.limit -100 100 command 
                let range = if v < 0 then kind.zero - kind.min else kind.max - kind.zero                            
                let duty = (kind.zero + range * v / 100)     
                fd.Write(duty)
            
    member self.Zero() = lock self <| fun () -> fd.Write(kind.zero)
            
    interface IObserver<int> with
        member self.OnNext(command) = 
            if Math.Abs(lastCommand - command) > ServoMotor.observerEps
            then lastCommand <- command; self.SetPower command
            
        member self.OnError e = self.SetPower kind.stop
        member self.OnCompleted () = self.SetPower kind.stop

    interface IDisposable with
        member self.Dispose() =
            self.Zero()
            (fd :> IDisposable).Dispose()
            setOption "request" "0"
    