open Trik
open Trik.Junior
open Trik.Junior.Parallel

let flicker = task { while true do
                        for x in [LedColor.Green; LedColor.Orange; LedColor.Red] do
                        robot.Led <- x
                        robot.Sleep(500)
                    }

let d = task { let rec loop() = robot.Sleep(1000); loop()
               loop()    
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

flicker.Execute()
drive.StartAndWait()



