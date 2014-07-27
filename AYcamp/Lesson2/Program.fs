open System
open Trik
open Trik.Junior
open Trik.Tasking

[<EntryPoint>]
let main _ = 
    use robot = new Robot()

    let flicker = task { while true do
                            for x in [1..3] do
                            printfn "current %d" x
                            Threading.Thread.Sleep(1000)//robot.Sleep(500)
                       }

    let k = 0.3 // some coefficient

    let drive = task { while true do
                          let u = k * (float <| robot.SensorA2 - 50)
                          robot.MotorM1 <- 70 + int u
                          robot.MotorM2 <- 70 - int u
                          if robot.SensorA1 > 60 then 
                            do! BREAK
                          robot.Sleep(300)
                       done
                     }


    do flicker.Start()
    do drive.StartAndWait()
    //(robot :> IDisposable).Dispose()
    0



