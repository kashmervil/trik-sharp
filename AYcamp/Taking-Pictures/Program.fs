open Trik.Junior
open Trik.Collections

let mutable button = ButtonEventCode.Down
robot.ButtonPad.Start()
printfn "Press any key to start"
robot.ButtonPad.Read() |> ignore

while button <> ButtonEventCode.Menu do
    printfn "Taking a picture"
    robot.TakePicture()
    printfn "Press any key to continue or Menu to exit\n"
    button <- robot.ButtonPad.Read().Button
    
    