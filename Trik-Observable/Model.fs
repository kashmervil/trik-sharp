namespace Trik
open System

type Model (config:Config.Schema.Config) = 
    member val Motor = 
        config.PowerMotors.GetPowerMotors() 
        |> Array.map (fun m ->  (m.Port, new PowerMotor(int m.I2cCommandNumber)))             
        |> dict
         
    //member val Servo = 
    //    config.ServoMotors.GetServoMotors() 
    //    |> Array.map (fun m ->  (m.Port, new Servomotor()))             
    //    |> dict
    
    member val AnalogSensor= 
        config.AnalogSensors.GetAnalogSensors() 
        |> Array.map (fun s -> (s.Port, new AnalogSensor("")))
        |> dict

    member val Gyro = 
                let c = config.DigitalSensors.Gyroscope 
                new Trik.Gyroscope(c.Min, c.Max, c.DeviceFile, c.Rate)
    member val Accel =
                let c =  config.DigitalSensors.Accelerometer
                new Trik.Accelerometer(c.Min, c.Max, c.DeviceFile, Helpers.milliseconds c.Rate)
    member val Led = new Trik.Led("/sys/class/leds/")

    static member Create(path:string) = new Model(Config.Create path)

    interface IDisposable with
        member x.Dispose() = () // no-no-no...
    

