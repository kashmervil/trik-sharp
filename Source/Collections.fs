namespace Trik.Collections
open System
open Trik
open Trik.Helpers

[<Struct>]
type Point(x: int, y: int, z: int) =
    member self.X = x
    member self.Y = y
    member self.Z = z
    override self.ToString() = sprintf "X= %d Y= %d Z= %d" x y z

    static member Zero = new Point(0,0,0)
    static member (+) (c1: Point, c2: Point) = new Point(c1.X + c2.X, c1.Y + c2.Y, c1.Z + c2.Z)
    static member (-) (c1: Point, c2: Point) = new Point(c1.X - c2.X, c1.Y - c2.Y, c1.Z - c2.Z)
    static member (*) (c1: Point, c2: Point) = new Point(c1.X * c2.X, c1.Y * c2.Y, c1.Z * c2.Z)

type ButtonEventCode  = 
    | Sync  = 0
    | Esc   = 1
    | Enter = 28 
    | Up    = 103 
    | Left  = 105 
    | Right = 106
    | Down  = 108
    | Power = 116
    //| Menu  = 139
      
[<Struct>]
type ButtonEvent(button: ButtonEventCode, isPressed: bool) = //, timeStamp: double) = 
    member self.Button = button
    member self.IsPressed = isPressed
    //member self.TimeStamp = timeStamp
    member self.AsTuple = button, isPressed//, timeStamp

    new (code: int, isPressed: bool) = //, timeStamp: double) = 
        ButtonEvent(enum<ButtonEventCode> code, isPressed)//, timeStamp)
    new (code: uint16, isPressed: bool) = //, timeStamp: double) = 
        ButtonEvent(int code, isPressed) //, timeStamp)
    override self.ToString() = button.ToString() + " " + isPressed.ToString()// + " " + timeStamp.ToString() 

type ServoKind = {
    min: int 
    max: int
    zero: int
    stop: int
    period:int
    }

/// Numbers of I2C ports associated with Stripe  
type LedStripePorts = {
    Red: int
    Green: int
    Blue: int
    Ground: int
    }

[<AbstractClass;Sealed>]
type Defaults() =
    let observerEps = 100
    /// {stop = 0; zero = 1500000; min = 1200000; max = 1800000; period = 20000000}
    static member val Servo1 = {stop = 0; zero = 1500000; min = 1200000; max = 1800000; period = 20000000} 
    /// {stop = 1; zero = 1500000; min = 1200000; max = 1800000; period = 20000000}
    static member val Servo2 = {Defaults.Servo1 with stop =1}
    // { stop = 0; zero = 1600000; min = 800000; max = 2400000; period = 20000000 }
    static member val Servo3 = { stop = 0; zero = 1600000; min = 800000; max = 2400000; period = 20000000 }
    /// { stop = 0; zero = 0; min = 0; max = 2000000; period = 2000000 }
    static member val Servo4 = { stop = 0; zero = 0; min = 0; max = 2000000; period = 2000000 }
    // { stop = 0; zero = 1310000; min = 1200000; max = 1420000; period = 20000000 }
    static member val Servo5 = { stop = 0; zero = 1310000; min = 1200000; max = 1420000; period = 20000000 }
    /// { stop = 0; zero = 1550000; min =  800000; max = 2250000; period = 20000000 }
    static member val Servo6 = { stop = 0; zero = 1550000; min =  800000; max = 2250000; period = 20000000 }
    /// { stop = 0; zero = 1500000; min = 800000; max = 2400000; period = 20000000 }
    static member val Servo7 = { stop = 0; zero = 1500000; min = 800000; max = 2400000; period = 20000000 }

    /// { Red = 0x14; Green = 0x15; Blue = 0x17; Ground = 0x16 }
    static member val LedSripe = { Red = 0x14; Green = 0x15; Blue = 0x17; Ground = 0x16 }

    static member val ServoEps = 5
    static member val EncoderPorts = [| B1; B2; B3; B4 |]
    static member val MotorPorts = [| M1; M2; M3; M4 |]
    static member val SensorPorts = [| A1; A2; A3; A4; A5; A6 |]


    

[<RequireQualifiedAccess>]
type PadEvent = 
    | Pad of int * ( int * int ) option
    | Button of int
    | Wheel of int
    | Stop
    
[<Flags>]
type LedColor = Green = 1 | Red = 2 | Orange = 3 | Off = 0

[<Struct>]
type LineLocation(x: int, crossroad: int, mass: int) = 
    member self.X = x
    member self.Crossroad = crossroad
    member self.Mass = mass
    new(x: string, c: string, m: string) = new LineLocation(fastInt32Parse x, fastInt32Parse c, fastInt32Parse m)
    override self.ToString() = sprintf "loc: %d %d %d\n\n" self.X self.Crossroad self.Mass

[<Struct>]
type ObjectLocation(x: int, y: int, mass: int) = 
    member self.X = x
    member self.Y = y
    member self.Mass = mass
    new(x: string, y: string, m: string) = new ObjectLocation(fastInt32Parse x, fastInt32Parse y, fastInt32Parse m)
    override self.ToString() = sprintf "loc: %d %d %d\n\n" self.X self.Y self.Mass

[<Struct>]
type DetectTarget(hue: int, hueTolerance: int, saturation: int, saturationTolerance: int, value: int, valueTolerance: int) =
    member self.Hue = hue
    member self.HueTolerance = hueTolerance
    member self.Saturation = saturation
    member self.SaturationTolerance = saturationTolerance
    member self.Value = value
    member self.ValueTolerance = valueTolerance
    new (hue: string, hueTolerance: string
        , saturation: string, saturationTolerance: string
        , value: string, valueTolerance: string) = new DetectTarget(fastInt32Parse hue
                                                                    , fastInt32Parse hueTolerance
                                                                    , fastInt32Parse saturation
                                                                    , fastInt32Parse saturationTolerance
                                                                    , fastInt32Parse value
                                                                    , fastInt32Parse valueTolerance)

    override self.ToString() = String.Format("hsv {0} {1} {2} {3} {4} {5}", hue, hueTolerance, saturation, saturationTolerance, value, valueTolerance)

type VideoSensorOutput<'Location> = 
        Location of 'Location
        | Target of DetectTarget with 
    ///Safe location getter
    member self.TryGetLocation = match self with
                                    | Location l -> Some l
                                    | Target _ -> None
    ///Safe target getter
    member self.TryGetTarget = match self with
                                | Location _ -> None
                                | Target t -> Some t
    ///Consider what property throws an exception when SensorOutput is Location
    member self.GetTarget = 
        let t = self.TryGetTarget
        if t.IsSome then t.Value else invalidOp "SensorOutput is not a Target"
    ///Consider what property throws an exception when SensorOutput is Target
    member self.GetLocation = 
        let t = self.TryGetLocation
        if t.IsSome then t.Value else invalidOp "SensorOutput is not a Location"