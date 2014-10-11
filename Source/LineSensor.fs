﻿namespace Trik
open System

[<Sealed>]
type LineSensor(scriptPath, commandPath: string, sensorPath) = 
    inherit VideoSensor<LineLocation>(scriptPath, commandPath, sensorPath)

    override self.Parse text =
            let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) 
            match parsedLines with
                | [| "loc:"; x; y; z |] -> Some (LineLocation(x, y, z))
                | [| "hsv:"; _; _; _; _; _; _ |] -> let command = text.Replace(':', ' ') in base.SendCommand command; base.SendCommand command
                                                    None
                | z -> printfn "none %A" z; None
    
    new () = new LineSensor("/etc/init.d/line-sensor-ov7670.sh"
                            , "/run/line-sensor.in.fifo"
                            , "/run/line-sensor.out.fifo")