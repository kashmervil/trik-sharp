namespace Trik
open System
open Trik.Helpers

[<Sealed>]
type LineSensor(scriptPath, commandPath: string, sensorPath) as sensor = 
    inherit StringFifoSensor<LineSensorOutput>(sensorPath)
    let mutable stream = null
    let mutable commandFifo: IO.StreamWriter = null
    do sensor.ParseFunc <- fun  text  ->
            let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) 
            match parsedLines with
                | [| "loc:"; x; y; z |] -> Some <| (LOC <| new Location(x, y, z))
                | [| "hsv:"; q1; q2; q3; q4; q5; q6 |] -> printfn " %s" text
                                                          commandFifo.WriteLine(text.Replace(':', ' '))
                                                          None
                | z -> printfn "none %A" z; None

    let script cmd = Helpers.SyscallShell <| scriptPath + " " + cmd
    let mutable videoOut = false
    member self.Stop() = base.Stop(); commandFifo.Close(); script "stop"
    
    //member self.Restart() = script "restart"
    
    member self.DetectAndSet() = 
        if commandFifo = null then invalidOp "missing Start() before call"
        commandFifo.WriteLine("detect")
    member self.VideoOut
        with get() = videoOut
        and set command = 
            if command <> videoOut then
                commandFifo.WriteLine("video_out {0}", if command then 1 else 0) 

    member self.Start() = 
        script "start"; base.Start()
        stream <- new IO.FileStream(commandPath, IO.FileMode.Open, IO.FileAccess.Write)
        commandFifo <- new IO.StreamWriter(stream, Text.Encoding.UTF8, AutoFlush = true)
        
    
    override self.Dispose() = 
        self.Stop()
        commandFifo.Dispose()
        stream.Dispose()
        base.Dispose()
            