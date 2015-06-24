open Trik
open Trik.Junior


let mutable button = ButtonEventCode.Down
robot.Buttons.Start()
printfn "Press any key to start"
robot.Buttons.Read() |> ignore

while button <> ButtonEventCode.Esc do
    printfn "Taking a picture"
    let name = robot.TakePicture()
    printfn "Press any key to continue or Menu to exit\n"
    button <- let key = robot.Buttons.Read() in key.Button
    
    