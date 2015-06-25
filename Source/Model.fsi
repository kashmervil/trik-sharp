namespace Trik
 open System.Collections.Generic
 type Model =
    class
      interface System.IDisposable
      new : unit -> Model
      member Accel : Sensors.Accelerometer
      /// Use AnalogSensors[Sensor.A1] for accessing A1 Analog sensor
      member AnalogSensors : IDictionary<ISensorPort,Sensors.AnalogSensor>
      member AnalogSensorsConfig : ISensorPort [] with get, set
      member Battery : Devices.Battery
      member Buttons : Devices.Buttons
      /// Use Encoders[Encoder.B1] for accessing B1 Encoder 
      member Encoders : IDictionary<IEncoderPort,Sensors.Encoder>
      member EncodersConfig : IEncoderPort [] with get, set
      
      /// 
      member Gyro : Sensors.Gyroscope
      
      ///<summary> 
      /// Class for controlling on-board led
//      /// TRIK led bulb which is placed at the top of a screen has three available colors.
//      /// You can adjust colors or turn the bulb of.
//      /// Led.SetColor(LedColor.Red)
//      /// Led.PowerOff()
      ///</summary>
      member Led : Devices.Led

      ///<summary> Class for controlling led stripe from TRIK set</summary>
      member LedStripe : Devices.LedStripe
      ///<summary> Configuration for LedStripe. Used to specify I2C ports for each color of stripe </summary>
      member LedStripeConfig : LedStripePorts with get, set
      ///<summary> Allows you to detect lines via camera </summary>
      member LineSensor : Sensors.LineSensor
      ///<summary> Determines the input source for LineSensor (e.g. USB or VP* ports) </summary>
      member LineSensorConfig : VideoSource with get, set
      ///<summary> Sensor for detecting M x N dominating colors in each of M x N areas</summary>
      member MXNSensor : Sensors.MXNSensor
      ///<summary> Determines the input source for MXNSensor (e.g. USB or VP* ports) </summary>
      member MXNSensorConfig : VideoSource with get, set
      /// Allows you to interact with a motor at a specified port. Example: Use Motors[Motor.M1] for accessing M1 motor
      member Motors : IDictionary<IMotorPort,Devices.PowerMotor>
      ///<summary> Allows you to detect 2D objects by color mask via camera</summary>
      member ObjectSensor : Sensors.ObjectSensor
      ///<summary> Determines the input source for ObjectSensor (e.g. USB or VP* ports) </summary>
      member ObjectSensorConfig : VideoSource with get, set
      ///<summary>Helper for working TRIK-gamepad mobile app </summary>
      member Pad : Network.PadServer
      /// Port number for PadServer, default port is 4444
      member PadConfig : int with get, set
      /// Use Servos[Servo.E1] for accessing E1 servo motor
      member Servos : IDictionary<IServoPort,Devices.ServoMotor>
      
      /// <summary>Pass configuration dictionary to make sure servos work fine.</summary>

      member ServosConfig : IDictionary<IServoPort,ServoKind> with get, set
      
      /// <summary>Synonym for Thread.Sleep, makes thread-execution stop for specified time</summary>
      /// <param name="ms">Delay time in milliseconds</param>
      member Sleep : ms:int -> unit
      static member RegisterResource : d:System.IDisposable -> unit
    end