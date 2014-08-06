﻿namespace Trik
open System

[<AutoOpen>]
module Collections = 
    [<Struct>]
    type Point(x: int, y: int, z: int) =
        member self.X = x
        member self.Y = y
        member self.Z = z
    
        static member Zero = new Point(0,0,0)
        static member (+) (c1: Point, c2: Point) = new Point(c1.X + c2.X, c1.Y + c2.Y, c1.Z + c2.Z)
        static member (-) (c1: Point, c2: Point) = new Point(c1.X - c2.X, c1.Y - c2.Y, c1.Z - c2.Z)
        static member (*) (c1: Point, c2: Point) = new Point(c1.X * c2.X, c1.Y * c2.Y, c1.Z * c2.Z)
         

    type ButtonEventCode  = 
     | Sync  = 0
     | Enter = 28 
     | Up    = 103 
     | Left  = 105 
     | Right = 106
     | Down  = 108
     | Power = 116
     | Menu  = 139
      
    /// Button event type in a form of (Button Code, Pressed/Released) 
    type ButtonEvent = ButtonEventCode * bool * double

    [<AutoOpen>]
    module ServoMotor =
        type Kind = {
            min: int 
            max: int
            zero: int
            stop: int
            period:int
            }
        let Servo1 = {stop = 0; zero = 1500000; min = 1200000; max = 1800000; period = 20000000} 
        let Servo2 = {Servo1 with stop =1}
        let observerEps = 100
    
    /// Numbers of I2C ports associated with Stripe  
    type LedStripePorts = {
        Red: int
        Green: int
        Blue: int
        Ground: int
        }
    [<RequireQualifiedAccess>]
    type PadEvent = 
        | Pad of int * ( int * int ) option
        | Button of int
        | Wheel of int
        | Stop
    
    [<Flags>]
    type LedColor = Green = 1 | Red = 2 | Orange = 3 | Off = 0
    
    let inline private parse x = Trik.Helpers.fastInt32Parse x
    [<Struct>]
    type Location(x: int, crossroad: int, mass: int) = 
        member self.X = x
        member self.Crossroad = crossroad
        member self.Mass = mass
        new(x: string, c: string, m: string) = new Location(parse x, parse c, parse m)

    [<Struct>]
    type HSV(hue: int, hueTolerance: int, saturation: int, saturationTolerance: int, value: int, valueTolerance: int) =
        member self.Hue = hue
        member self.HueTolerance = hueTolerance
        member self.Saturation = saturation
        member self.SaturationTolerance = saturationTolerance
        member self.Value = value
        member self.ValueTolerance = valueTolerance
        new (hue: string, hueTolerance: string
            , saturation: string, saturationTolerance: string
            , value: string, valueTolerance: string) = new HSV(parse hue
                                                        , parse hueTolerance, parse saturation
                                                        , parse saturationTolerance, parse value
                                                        , parse valueTolerance)
 
    type LineSensorOutput = LOC of Location | HSV of HSV with
        override self.ToString() = 
            match self with
            | LOC loc -> sprintf "loc: %d %d %d\n\n" loc.X loc.Crossroad loc.Mass
            | HSV hsv -> sprintf "hsv: %d %d %d %d %d %d" hsv.Hue hsv.HueTolerance hsv.Saturation hsv.SaturationTolerance hsv.Value hsv.ValueTolerance