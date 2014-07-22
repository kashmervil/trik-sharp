open Trik
open Trik.Junior
open Trik.Tasking

let robot = new Robot()

let flicker = task { while true do
                        robot.Led <- LedColor.Green
                        robot.Sleep(0.5)
                        robot.Led <- LedColor.Orange
                        robot.Sleep(0.5)
                        robot.Led <- LedColor.Red
                        robot.Sleep(0.5)
                   }

let k = 1 // some coefficient
let drive = task { while true do
                      let u = k * (robot.SensorA2 - 50)
                      robot.MotorM1 <- 70 + u
                      robot.MotorM2 <- 70 - u
                      if robot.SensorA1 > 60 then 
                        do! BREAK
                      robot.Sleep(300)
                   done
                 }


do flicker.Start()
do drive.StartAndWait()
