open Trik        // These namespaces and modules should be opened
open Trik.Junior
open Trik.Ports

//robot is already defined. Don't try to create another it's not allowed,
// Just use it and have fun =)
robot.Accel.Start()
printfn "%d" (robot.Accel.Read().X)

//You can pass values from -100 to 100 to Servo and Motor
robot.Motor.[M1].SetPower(100)

robot.Led.SetColor LedColor.Green 

robot.Say("Hello")
//If you want to pass successive result of function or method call
// where are several possible variants
printfn "%d" (robot.Sensor.[A1].Read())
printfn "%d" <| robot.Sensor.[A2].Read()
robot.Sensor.[A3].Read() |> printfn "%d"
//Since printfn "%d" robot.Sensor.[A1].Read() won't work

for motorPort in Ports.Motor.Values do // "for x in xs" is an alternative for "for i=start to finish do".
// xs can be Array, List or another enumerable collection. [1..12] or ["2"; "3"; "5"]
    robot.Motor.[motorPort].SetPower 70 // braces for function calls can be omitted
    robot.Sleep 100

// The ports in a robot.Sensor is an enum that can be found in Trik.Ports namespace
// You can look at all the available ports for Sensors with Trik.Ports.Sensor.Values
// It looks like this [| A1; A2; A3; A4; A5; A6 |].
// It helps to make sure you addressing a right port
// For example this won't be compiled robot.Sensor.[A7].Read()
// Servos and Motors have the same indexing structure in robot

//You can make aliases for on-robot devices just like this
let servo1 = robot.Servo.[E1]
servo1.SetPower(100)

robot.Sleep(1000)

//Gyroscope, Accellerometer and LineSensor must be Started before Reading
// for example this read will raise an exception robot.Gyro.Read()
robot.Gyro.Start()
printfn "%A" (robot.Gyro.Read())
robot.Gyro.Stop() // You can stop sensor if you don't need it anymore.
// This is not compulsory, but you can save a little cpu cycles
