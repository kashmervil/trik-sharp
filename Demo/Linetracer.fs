module Linetracer

open Trik
open System
open System.IO
open System.Collections
open System.Text
open System.Diagnostics

let logFifoPath = @"/tmp/dsp-detector.out.fifo"
let cmdFifoPath = @"/tmp/dsp-detector.in.fifo"

let speed = 100;
let stopK = 1;
let PK = 0.42;
let IK = 0.006;
let DK = -0.009;
let encC = 1.0 / (334.0 * 34.0); //1 : (num of points of encoder wheel * reductor ratio)

let (=^.^=) a b = 
    if a > b then sprintf "%A %s %A" a " is cuter than " b 
    else sprintf "%A %s %A" b " is cuter than " a

let x = 5 =^.^= 3

type Encoder(i2cCommandNumber) as sens =
    inherit Helpers.PollingSensor<float32>()
    [<Literal>]
    let parToRad = 0.03272492f
    do sens.ReadFunc <- fun () ->
        let data = Helpers.I2C.receive i2cCommandNumber
        parToRad * float32 (data)
    member x.Reset() = 
        Helpers.I2C.send i2cCommandNumber
    interface IDisposable with
        member x.Dispose() = ()

type LogFifo(path) = //, _lineTargetDataParsed, _lineColorDataParsed) = 
    [<Literal>]
    let max_fifo_input_size = 4000
    let fd = File.Open(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
    let mutable rest = new StringBuilder ""
    let mutable loopDone = false
    let buf = Array.zeroCreate max_fifo_input_size
    let lineTargetDataParsed = new Event<int*int*int>()
    let lineColorDataParsed = new Event<int*int*int*int*int*int>()
    let rec loop() = 
        while not loopDone do
            let cnt = fd.Read(buf, 0, max_fifo_input_size)
            let part = Encoding.ASCII.GetString(buf, 0, cnt)
            rest <- rest.Append(part)
            if part.IndexOf('\n') <> -1 then
                let lines = part.Split([| '\n' |], StringSplitOptions.RemoveEmptyEntries)
                let mutable i = Array.length lines - 2
                let mutable wasLoc = false
                let mutable wasHsv = false
                while (not (wasHsv && wasLoc) ) && i >= 0 do
                    let logStruct = lines.[i].Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries)
                    if not wasLoc && logStruct.[0] = "loc:" then 
                        let x     = Helpers.fastInt32Parse logStruct.[1]
                        let angle = Helpers.fastInt32Parse logStruct.[2]
                        let mass  = Helpers.fastInt32Parse logStruct.[3]
                        wasLoc <- true
                        lineTargetDataParsed.Trigger(x, angle, mass)
                    elif not wasHsv && logStruct.[0] = "hsv:" then
                        let hue    = Helpers.fastInt32Parse logStruct.[1]
                        let hueTol = Helpers.fastInt32Parse logStruct.[2]
                        let sat    = Helpers.fastInt32Parse logStruct.[3]
                        let satTol = Helpers.fastInt32Parse logStruct.[4]
                        let _val   = Helpers.fastInt32Parse logStruct.[5]
                        let valTol = Helpers.fastInt32Parse logStruct.[6]
                        wasHsv <- true
                        lineColorDataParsed.Trigger(hue, hueTol, sat, satTol, _val, valTol)
                    else ()
                    i <- i - 1
        loop()
    do Async.Start <| async { loop() } 
    member x.LineTargetDataParsed = lineTargetDataParsed.Publish
    member x.LineColorDataParsed = lineColorDataParsed.Publish

type CmdFifo(path:string) = 
    let fd = new IO.StreamWriter(path, AutoFlush = true)
    member x.Write(cmd:string) = 
        fd.WriteLine(cmd)
    interface IDisposable with
        member x.Dispose() = fd.Dispose()


type MotorControler (motor: Trik.PowerMotor, enc: Encoder) = 
    [<Literal>]
    let max_ppms = 23.833f
    let mutable actualSpeed = 0
    let mutable currentSpeed = 0.0f
    let mutable encPoints_prev = 0.0f
    let sw = new Stopwatch()
    do sw.Start()
    let mutable time_prev = sw.ElapsedMilliseconds
    let countSpeed() = 
        let encPoints = enc.Read()
        let time = sw.ElapsedMilliseconds
        

        let period = sw.ElapsedMilliseconds |> float32
        let speed = (encPoints - encPoints_prev) * 100.0f / (period * max_ppms)

        if currentSpeed <> speed then ()
            //qDebug() << "current speed: " << speed << " period: " << period
      
  
        currentSpeed <- speed
        time_prev <- time
        encPoints_prev <- encPoints

        //speedometerDone()
        //speed
    let setLineTargetDataParsed x angle mass = ()
    let setLineColorDataParsed hue hueTol sat satTol _val valTol = ()    
    member x.doStep() = 
        motor.SetPower(actualSpeed)    

let run (model:Model) = 
    let log_fifo = new LogFifo(logFifoPath)
    let cmd_fifo = new CmdFifo(cmdFifoPath)
    cmd_fifo.Detect <| fun ->
    )
    ()