namespace Trik
open System
open Trik.Observable

type Model (config:Config.Schema.Config) = 
    member val Motor = 
        config.PowerMotors.GetPowerMotors() 
        |> Array.map (fun m ->  (m.Port, new PowerMotor(int m.I2cCommandNumber)))             
        |> dict
         
    member val Servo = 
        config.ServoMotors.GetServoMotors() 
        |> Array.map (fun m ->  (m.Port, new Servomotor(m, config.ServoMotorTypes)))             
        |> dict
    
    member val AnalogSensor= 
        config.AnalogSensors.GetAnalogSensors() 
        |> Array.map (fun s -> (s.Port, new AnalogSensor(s)) )
        |> dict

    member val Gyro = new Trik.Observable.Gyroscope(config.DigitalSensors.Gyroscope)
    member val Accel = new Trik.Observable.Accelerometer(config.DigitalSensors.Accelerometer)
    member val Led = new Trik.Observable.Led(config.Led)

    static member Create(path:string) = new Model(Config.Create path)

    interface IDisposable with
        member x.Dispose() = () // no-no-no...
    

