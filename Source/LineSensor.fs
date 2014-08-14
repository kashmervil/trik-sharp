namespace Trik
open System

[<Sealed>]
type LineSensor(scriptPath, commandPath: string, sensorPath) = 
    inherit StringFifoSensor<Location>(sensorPath)
    let mutable stream = null
    let mutable commandFifo: IO.StreamWriter = null
    let script cmd = Helpers.SyscallShell <| scriptPath + " " + cmd
    let mutable videoOut = true
    
    override self.Parse text =
            let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) 
            match parsedLines with
                | [| "loc:"; x; y; z |] -> Some (Location(x, y, z))
                | [| "hsv:"; _; _; _; _; _; _ |] -> commandFifo.WriteLine(text.Replace(':', ' '));commandFifo.WriteLine(text.Replace(':', ' '))
                                                    None
                | z -> printfn "none %A" z; None

    member self.Start() = 
        script "start"; base.Start()

        stream <- new IO.FileStream(commandPath, IO.FileMode.Open, IO.FileAccess.Write)
        commandFifo <- new IO.StreamWriter(stream, Text.Encoding.UTF8, AutoFlush = true)
    
    //member self.Restart() = script "restart"

    member self.Stop() = base.Stop(); commandFifo.Close(); script "stop"
    
    member self.DetectAndSet() = 
        if commandFifo = null then invalidOp "missing Start() before call"
        commandFifo.WriteLine("detect")

    member self.VideoOut
        with get() = videoOut
        and set command = 
            if command <> videoOut then
                commandFifo.WriteLine("video_out {0}", if command then 1 else 0) 
                commandFifo.WriteLine("video_out {0}", if command then 1 else 0) 
                videoOut <- command
            
    override self.Dispose() = 
        self.Stop()
        commandFifo.Dispose()
        stream.Dispose()
        base.Dispose()
            