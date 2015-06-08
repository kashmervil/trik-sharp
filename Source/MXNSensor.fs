namespace Trik.Sensors
open System
open System.IO
open System.Threading
open Trik
open Trik.Helpers
open Trik.Internals

//This sensor is gives you an array of dominant colors in M x N zones of screen
//The returning values for each zone is an integer, from which we can easily get
//a color in RGB representation
// (x && 0xFF)         - Blue
// ((x >> 8) && 0xFF)  - Green
// ((x >> 16) && oxFF) - Red 
// where x is an element of our array

[<Sealed>]
type MXNSensor(scriptPath, commandPath: string, sensorPath) = 
    inherit StringFifoSensor<int []>(sensorPath)

    let mutable stream = null
    let mutable commandFifo: StreamWriter = null
    let mutable isDisposed = false
    let cts = new CancellationTokenSource()
    let mutable (sizeM, sizeN) = (3, 3)

    let parse x = Calculations.unsafeInt32Parse x
    let script cmd = Shell.send <| scriptPath + " " + cmd

    //This property can be used to change amount of zones
    member self.Size 
        with get () = (sizeM, sizeN)
        and set (m, n) = 
            sizeM <- m
            sizeN <- n
            if commandFifo = null then invalidOp "missing Start() before call"
            commandFifo.WriteLine("mxn " + (string m) + " " + (string n))
            commandFifo.WriteLine("mxn " + (string m) + " " + (string n))

    member self.Start() = 
        script "start"; base.Start()

        stream <- new FileStream(commandPath, FileMode.Open, FileAccess.Write)
        commandFifo <- new StreamWriter(stream, Text.Encoding.UTF8, AutoFlush = true)

    member self.Stop() = base.Stop(); commandFifo.Close(); script "stop"

    new (videoSource) = 
        let script = 
            match videoSource with
            | VideoSource.USB -> "/etc/init.d/mxn-sensor-webcam.sh"
            | _                     -> "/etc/init.d/mxn-sensor-ov7670.sh"

        new MXNSensor(script, "/run/mxn-sensor.in.fifo", "/run/mxn-sensor.out.fifo")

    override self.Parse text =
        let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
        parsedLines.[1..] |> Array.map parse |> Some

    override self.Dispose() = 
        if not isDisposed then
            cts.Cancel()
            commandFifo.Dispose()
            stream.Dispose()
            base.Dispose()
            self.Stop()
            isDisposed <- true
