namespace Trik
open System

[<Sealed>]
type ObjectSensor(scriptPath, commandPath: string, sensorPath) = 
    inherit VideoSensor<ObjectLocation>(scriptPath, commandPath, sensorPath)
    
    override self.Parse text =
            let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) 
            match parsedLines with
                | [| "loc:"; x; y; z |] -> Some (ObjectLocation(x, y, z))
                | [| "hsv:"; _; _; _; _; _; _ |] -> let command = text.Replace(':', ' ') in base.SendCommand command; base.SendCommand command
                                                    None
                | z -> printfn "none %A" z; None
    
    new () = new ObjectSensor("/etc/init.d/object-sensor-ov7670.sh"
                              , "/run/object-sensor.in.fifo"
                              , "/run/object-sensor.out.fifo")