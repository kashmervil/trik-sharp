open Trik
open Trik.Junior
open Trik.Junior.Parallel

printfn "Starting"

let flicker = task { for i=1 to 100 do
                        for color in [LedColor.Green; LedColor.Orange; LedColor.Red] do
                            robot.Led <- color
                            robot.Sleep(500)   
                    }

let k = 3 
let drive = task { while true do
                        let u = k * (robot.SensorA2 - 500) / 10
                        robot.MotorM1 <- 70 + u
                        robot.MotorM2 <- 70 - u
                        if robot.SensorA1 > 600 then 
                            do! BREAK
                        robot.Sleep(300)
                    done
                    }

let upside = task { while true do 
                        let d = robot.GyroRead().z
                        printfn "z = %d" d
                        if robot.GyroRead().z < -100 then 
                            printfn "It's likely that I'm upside down"
                        robot.Sleep(100)
                  }

printfn "Executing"

let group = flicker <+> drive
let anotherGroup = upside <+> group

anotherGroup.StartAndWait(30* 1000)