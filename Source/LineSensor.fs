namespace Trik.Sensors
open System
open Trik

[<Sealed>]
type LineSensor(scriptPath, commandPath: string, sensorPath) = 
    inherit Internals.VideoSensor<LineLocation>(scriptPath, commandPath, sensorPath)

    override self.Parse text =
            let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) 
            match parsedLines with
                | [| "loc:"; x; y; z |] -> Some (LineLocation(x, y, z))
                | [| "hsv:"; _; _; _; _; _; _ |] -> let command = text.Replace(':', ' ') in base.SendCommand command; base.SendCommand command
                                                    None
                | z -> printfn "none %A" z; None
    
    new (videoSource) = 
        let script = 
            match videoSource with
            | Ports.VideoSource.USB -> "/etc/init.d/line-sensor.sh"
            | _                     -> "/etc/init.d/line-sensor-ov7670.sh"

        new LineSensor(script, "/run/line-sensor.in.fifo", "/run/line-sensor.out.fifo")
