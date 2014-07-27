open Lesson1

let robot = new Robot()

robot.Say("Привет, Трик!")

robot.MotorM1 <- 100
robot.MotorM2 <- 100

robot.Sleep(4.)

robot.MotorM1 <- 0
robot.MotorM2 <- 0