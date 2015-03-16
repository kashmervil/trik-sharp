namespace Trik
 open System.Collections.Generic
 type Model =
    class
      interface System.IDisposable
      new : unit -> Model
      member Accel : Sensors.Accelerometer
      /// Use AnalogSensors[Sensor.A1] for accessing A1 Analog sensor
      member AnalogSensors : IDictionary<Sensor,Sensors.AnalogSensor>
      member AnalogSensorsConfig : (Sensor * int) []
      member Battery : Devices.Battery
      member Buttons : Devices.Buttons
      /// Use Encoders[Encoder.B1] for accessing B1 Encoder 
      member Encoders : IDictionary<Encoder,Sensors.Encoder>
      member EncodersConfig : (Encoder * int) []
      member Gyro : Sensors.Gyroscope
      member Led : Devices.Led
      member LedStripe : Devices.LedStripe
      member LedStripeConfig : Collections.LedStripePorts
      member LineSensor : Sensors.LineSensor
      member LineSensorConfig : VideoSource
      member MXNSensor : Sensors.MXNSensor
      member MXNSensorConfig : VideoSource
      /// Use Motors[Motor.M1] for accessing M1 motor
      member Motors : IDictionary<Motor,Devices.PowerMotor>
      member MotorsConfig : (Motor * int) []
      member ObjectSensor : Sensors.ObjectSensor
      member ObjectSensorConfig : VideoSource
      member Pad : Network.PadServer
      member PadConfigPort : int
      /// Use Servos[Servo.E1] for accessing E1 servo motor
      member Servos : IDictionary<Servo,Devices.ServoMotor>
      member
        Servos : IDictionary<Servo,Devices.ServoMotor>
      member ServosConfig : (Servo * (string * Collections.ServoKind)) []
      member AnalogSensorsConfig : (Sensor * int) [] with set
      member EncodersConfig : (Encoder * int) [] with set
      member LedStripeConfig : Collections.LedStripePorts with set
      member LineSensorConfig : VideoSource with set
      member MXNSensorConfig : VideoSource with set
      member MotorsConfig : (Motor * int) [] with set
      member ObjectSensorConfig : VideoSource with set
      member PadConfigPort : int with set
      member
        ServosConfig : (Servo * (string * Collections.ServoKind)) [] with set
      static member RegisterResource : d:System.IDisposable -> unit
    end