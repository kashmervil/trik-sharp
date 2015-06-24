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
      member Gyro : Sensors.Gyroscope
      member Led : Devices.Led
      member LedStripe : Devices.LedStripe
      member LedStripeConfig : LedStripePorts with get, set
      member LineSensor : Sensors.LineSensor
      member LineSensorConfig : VideoSource with get, set
      member MXNSensor : Sensors.MXNSensor
      member MXNSensorConfig : VideoSource with get, set
      /// Use Motors[Motor.M1] for accessing M1 motor
      member Motors : IDictionary<IMotorPort,Devices.PowerMotor>
      member ObjectSensor : Sensors.ObjectSensor
      member ObjectSensorConfig : VideoSource with get, set
      member Pad : Network.PadServer
      member PadConfigPort : int with get, set
      /// Use Servos[Servo.E1] for accessing E1 servo motor
      member Servos : IDictionary<IServoPort,Devices.ServoMotor>
      member ServosConfig : IDictionary<IServoPort,ServoKind> with get, set
      static member RegisterResource : d:System.IDisposable -> unit
    end