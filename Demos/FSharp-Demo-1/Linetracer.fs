module Linetracer

open System
open System.IO
open System.Collections
open System.Diagnostics
open System.Threading
open Trik
open Trik.Helpers


let logFifoPath = @"/tmp/dsp-detector.out.fifo"
let cmdFifoPath = @"/tmp/dsp-detector.in.fifo"

let speed = 100;
let stopK = 1;
let PK = 0.42;
let IK = 0.006;
let DK = -0.009;
let encC = 1.0 / (334.0 * 34.0); //1 : (num of points of encoder wheel * reductor ratio)
let max_fifo_input_size = 4000

type LogFifo(path:string) = 
    let sr = new StreamReader(path)

    let mutable loopDone = false
    let lineTargetDataParsed = new Event<_>()
    let lineColorDataParsed = new Event<_>()

    let checkLines (lines:string[]) last = 
        let mutable i = last
        let mutable wasLoc = false
        let mutable wasHsv = false
        while (not (wasHsv && wasLoc) ) && i >= 0 do
            let logStruct = lines.[i].Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries)
            //printfn "%s" logStruct.[0]
            if not wasLoc && logStruct.[0] = "loc:" then 
                let x     = Helpers.fastInt32Parse logStruct.[1]
                let angle = Helpers.fastInt32Parse logStruct.[2]
                let mass  = Helpers.fastInt32Parse logStruct.[3]
                wasLoc <- true
                lineTargetDataParsed.Trigger(x, angle, mass)
                //printfn "ltparsed"
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
    let rec loop() = 
        let ln = sr.ReadLine()
        //eprintfn "%s" ln
        checkLines [| ln |] 0
        if not loopDone then loop() 
    do Async.Start <| async { loop() } 
    member x.LineTargetDataParsed = lineTargetDataParsed.Publish
    member x.LineColorDataParsed = lineColorDataParsed.Publish
    interface IDisposable with
        member x.Dispose() = loopDone <- true

type CmdFifo(path:string) as self = 
    let fd = new IO.FileStream(path, FileMode.Truncate)
    let mutable h = null
    let detectTimer = new System.Timers.Timer(200.0, Enabled = false)
    do detectTimer.Elapsed.Add(fun _ -> self.Write("detect") )
    member x.Write(cmd:string) = 
        let buf = Text.Encoding.ASCII.GetBytes(cmd + "\n")
        fd.Write(buf, 0, buf.Length)
        fd.Flush()
    member x.Detect (logFifo:LogFifo) f = 
        h <- new Handler<_> (fun _ a -> 
            logFifo.LineColorDataParsed.RemoveHandler h
            detectTimer.Stop()
            eprintfn "x.Detect Handler"
            f a )
        logFifo.LineColorDataParsed.AddHandler(h)
        x.Write("detect")
        detectTimer.Start()
        eprintfn "x.Detect"


    interface IDisposable with
        member x.Dispose() = fd.Dispose()


type MotorControler (motor: Trik.PowerMotor, enc: Encoder, sign) = 
    [<Literal>]
    let max_ppms = 23.833f
    [<DefaultValue>]
    val mutable ActualSpeed : int
    [<DefaultValue>]
    val mutable PowerAddition : int
    [<DefaultValue>]
    val mutable PowerBase : int
    let mutable currentSpeed = 0.0f<rad>
    let mutable encPoints_prev = 0.0f<rad>
    let sw = new Stopwatch()
    do sw.Start()
    let mutable time_prev = sw.ElapsedMilliseconds
    let countSpeed() = 
        let encPoints = enc.ReadRadian()
        let time = sw.ElapsedMilliseconds
        let period = time - time_prev |> float32
        let speed = (encPoints - encPoints_prev) * 100.0f / (period * max_ppms)

        if currentSpeed <> speed then ()

        currentSpeed <- speed
        time_prev <- time
        encPoints_prev <- encPoints
        speed
    member x.doStep() = 
        x.ActualSpeed <- x.PowerBase + x.PowerAddition 
        motor.SetPower(sign * x.ActualSpeed)    

type Linetracer (model: Model) = 
    let sw = new Stopwatch()
    do eprintfn "Linetracer ctor"
    let cmd_fifo = new CmdFifo(cmdFifoPath)
    let log_fifo = new LogFifo(logFifoPath)
    //do System.Console.ReadKey |> ignore
    do sw.Restart()
    let localConfPath = "ltc.ini"
    let conf = Helpers.loadIniConf localConfPath
    let elapsed = sw.ElapsedMilliseconds
    do eprintfn "Linetracer ctor: config parsed: %A ms" elapsed 
    let min_mass = conf.["min_mass"] |> Helpers.fastInt32Parse
    let power_base = conf.["power_base"] |> Helpers.fastInt32Parse
    let motor_sign = conf.["motor_sign"] |> Helpers.fastInt32Parse
    let rl_sign = conf.["rl_sign"] |> Helpers.fastInt32Parse
    let div_coefL = conf.["div_coefL"] |> Double.Parse
    let div_coefR = conf.["div_coefR"] |> Double.Parse
    let on_lost_coef = conf.["on_lost_coef"] |> Double.Parse
    let turn_mode_coef = conf.["turn_mode_coef"] |> Helpers.fastInt32Parse
    let elapsed = sw.ElapsedMilliseconds
    do eprintfn "Linetracer ctor: config parsed: %A ms" elapsed 

    let motorL = new MotorControler(model.Motor.["JM1"], model.Encoder.["JB2"], motor_sign, PowerBase = power_base)
    let motorR = new MotorControler(model.Motor.["JM3"], model.Encoder.["JB3"], motor_sign * -1, PowerBase = power_base)
    let frontSensor = model.AnalogSensor.["JA2"]
    do eprintfn "Linetracer ctor: Motor controllers created"
    let mutable last_pow_add = 0
    let mutable (stopAutoMode: IDisposable) = null
    let mutable (frontSensorObs: IDisposable) = null
    let mutable (sideSensorObs: IDisposable) = null
    let startAutoMode(stm) = 
        (*frontSensorObs <- 
            frontSensor.ToObservable()
            |> Observable.subscribe (fun x -> if x > 60 then frontSensorObs.Dispose(); stm() ) *)
        stopAutoMode <-
            log_fifo.LineTargetDataParsed
            //|> Observable.subscribe(fun (x, angle, mass) -> eprintfn "lt: %d %d" x mass )
            //|> Observable.filter(fun (x, angle, mass) -> mass > min_mass )
            |> Observable.subscribe (fun (x, angle, mass) -> 
                if mass = 0 then
                    motorL.PowerAddition <- int <| float(last_pow_add) * div_coefL * on_lost_coef
                    motorR.PowerAddition <- int <| float(-last_pow_add) * div_coefR * on_lost_coef
                else 
                    last_pow_add <- x * rl_sign
                    motorL.PowerAddition <- int <| float(last_pow_add) * div_coefL
                    motorR.PowerAddition <- int <| float(-last_pow_add) * div_coefR
                //eprintfn "lt: %d %d" motorL.PowerAddition motorR.PowerAddition
                motorL.doStep()
                motorR.doStep()
                () )      
    let rec startTurnMode() = 
        eprintfn "startTurnMode"
        if stopAutoMode <> null then stopAutoMode.Dispose(); stopAutoMode <- null
        motorL.PowerAddition <- turn_mode_coef
        motorR.PowerAddition <- -turn_mode_coef
        Thread.Sleep 500
        motorL.PowerAddition <- -turn_mode_coef
        motorR.PowerAddition <- turn_mode_coef
        Thread.Sleep 500
        motorL.PowerAddition <- 0
        motorR.PowerAddition <- 0
        startAutoMode(startTurnMode)
    member x.Run() = 
        eprintfn "Linetracer.Run"
        cmd_fifo.Detect log_fifo <| fun (hue, hueTol, sat, satTol, _val, valTol) ->
            //printfn "detected: %d %d " hue hueTol
            printfn "detected"
            cmd_fifo.Write <| sprintf "hsv %d %d %d %d %d %d" hue hueTol sat satTol _val valTol
            startAutoMode(startTurnMode)
        eprintfn "Ready (any key to finish)"
        System.Console.ReadKey() |> ignore