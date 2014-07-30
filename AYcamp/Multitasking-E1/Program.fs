open Trik
open Trik.Junior
open Trik.Junior.Parallel


//robot or brick

printfn "Starting"
let flicker = task { for i=1 to 100 do
                        for color in [LedColor.Green; LedColor.Orange; LedColor.Red] do
                            robot.Led <- color // setColor?
                            robot.Sleep(500)
                    }

let k = 3 // some coefficient
let drive = task { while true do
                        let u = k * (robot.SensorA2 - 500) / 10// Raw data
                        robot.MotorM1 <- 70 + u
                        robot.MotorM2 <- 70 - u
                        if robot.SensorA1 > 600 then 
                            do! BREAK
                        robot.Sleep(300)
                    done
                    }

printfn "Executing"

flicker.Execute() //ывфываваывыаыфываыфвыаф
drive.StartAndWait()//fsadfdsafdsasdafsdafasd



