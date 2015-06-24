open Trik
open Trik.Junior


robot.Led.PowerOff()

robot.Led.SetColor LedColor.Green

let buttons = robot.Buttons

buttons.Start()

printfn "Press any key on the controller"
printfn "You pressed %A" <| buttons.Read()

robot.LineSensor.Start()
robot.Led.SetColor LedColor.Orange

printfn "Press any key to detect"
let d = buttons.Read()

let target = robot.LineSensor.Detect()

printfn "Detected; press Down to turn on video streaming"

if buttons.Read().Button = ButtonEventCode.Down then robot.LineSensor.VideoOut <- true

printfn "Press Enter to stop evaluating"
let mutable error = 0.0

let isEnterPressed = buttons.CheckPressing ButtonEventCode.Enter  

#nowarn "25"
while not !isEnterPressed do
    let (Location current) = robot.LineSensor.Read()
    System.Console.WriteLine("{0} {1} {2}", current.X, current.Crossroad, current.Mass)
    error <- error + 0.1

robot.Motor.[M1].SetPower(100)