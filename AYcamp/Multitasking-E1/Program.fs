open Trik
open Trik.Junior
open Trik.Junior.Parallel

printfn "Starting"
let flicker = task { while true do
                        for x in [LedColor.Green; LedColor.Orange; LedColor.Red] do
                        robot.Led <- x
                        robot.Sleep(500)
                    }

let k = 0.3 // some coefficient
let drive = task { while true do
                        let u = k * (float <| robot.SensorA2 - 50)
                        robot.MotorM1 <- 70 + int u
                        robot.MotorM2 <- 70 - int u
                        if robot.SensorA1 > 60 then 
                            do! BREAK
                        System.Console.WriteLine("Senson 1 {0} \n Sensor 2 {0}", robot.SensorA1, robot.SensorA2)
                        robot.Sleep(300)
                    done
                    }

printfn "Executing"
let d = flicker.Start()
drive.StartAndWait()
System.Console.Read() |> ignore



