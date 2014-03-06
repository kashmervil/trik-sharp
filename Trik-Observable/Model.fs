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
        |> Array.map (fun s -> (s.Port, new AnalogSensor(s.I2cCommandNumber, s.Rate)) )
        |> dict

    member val Gyro = 
                let c = config.DigitalSensors.Gyroscope 
                new Trik.Observable.Gyroscope(c.Min, c.Max, c.DeviceFile, c.Rate)
    member val Accel =
                let c =  config.DigitalSensors.Accelerometer
                new Trik.Observable.Accelerometer(c.Min, c.Max, c.DeviceFile, Helpers.milliseconds c.Rate)
    member val Led = new Trik.Observable.Led(config.Led)

    static member Create(path:string) = new Model(Config.Create path)

    interface IDisposable with
        member x.Dispose() = () // no-no-no...
    

