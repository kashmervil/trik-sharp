open Trik
open Trik.Junior
open Trik.Ports
open Trik.Junior.Parallel

printfn "Starting"
let flicker = task { for i=1 to 100 do
                        for color in [LedColor.Green; LedColor.Orange; LedColor.Red] do
                            robot.Led.SetColor color
                            robot.Sleep(500)
                    }

let k = 3 
let drive = task { while true do
                        let u = k * (robot.Sensor.[A2].Read() - 500) / 10
                        robot.Motor.[M1].SetPower(70 + u)
                        robot.Motor.[M2].SetPower(70 - u)
                        if robot.Sensor.[A1].Read() > 600 then 
                            do! BREAK
                        robot.Sleep(300)
                    done
                    }

let upside = task { while true do 
                        let d = robot.Gyro.Read().Z
                        printfn "z = %d" d
                        if robot.Gyro.Read().Z > 50 then 
                            printfn "It's likely that I'm upside down"
                        robot.Sleep(100)
                  }

printfn "Executing"

let group = flicker <+> drive
let anotherGroup = upside <+> group

anotherGroup.StartAndWait(30* 1000)