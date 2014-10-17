open Trik
open Trik.Junior
open Trik.Collections
open Trik.Ports

robot.Led.PowerOff()

robot.Led.SetColor LedColor.Green

let buttonPad = robot.ButtonPad

buttonPad.Start()

printfn "Press any key on the controller"
printfn "You pressed %A" <| buttonPad.Read()

robot.LineSensor.Start()
robot.Led.SetColor LedColor.Orange

printfn "Press any key to detect"
let d = buttonPad.Read()

let target = robot.LineSensor.Detect()

printfn "Detected; press Down to turn on video streaming"

if buttonPad.Read().Button = ButtonEventCode.Down then robot.LineSensor.VideoOut <- true

printfn "Press Enter to stop evaluating"
let mutable error = 0.0

let isEnterPressed = buttonPad.CheckPressing ButtonEventCode.Enter  

#nowarn "25"
while not !isEnterPressed do
    let (Location current) = robot.LineSensor.Read()
    System.Console.WriteLine("{0} {1} {2}", current.X, current.Crossroad, current.Mass)
    error <- error + 0.1

robot.Motor.[M1].SetPower(100)