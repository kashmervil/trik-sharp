namespace Trik.Sensors
open System
open Trik

[<Sealed>]
type ObjectSensor(scriptPath, commandPath: string, sensorPath) = 
    inherit Internals.VideoSensor<ObjectLocation>(scriptPath, commandPath, sensorPath)
    let mutable isDisposed = false

    new (videoSource) = 
        let script = 
            match videoSource with
            | Ports.VideoSource.USB -> "/etc/init.d/object-sensor.sh"
            | _                     -> "/etc/init.d/object-sensor-ov7670.sh"

        new ObjectSensor(script, "/run/object-sensor.in.fifo", "/run/object-sensor.out.fifo")

    override self.Parse text =
            let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) 
            match parsedLines with
                | [| "loc:"; x; y; z |] -> Some (ObjectLocation(x, y, z))
                | [| "hsv:"; _; _; _; _; _; _ |] -> let command = text.Replace(':', ' ') in base.SendCommand command; base.SendCommand command
                                                    None
                | z -> printfn "none %A" z; None

    override self.Dispose() = base.Dispose()    
    
    override self.Finalize() = self.Dispose()

    

    