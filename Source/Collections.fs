namespace Trik
open System

[<AutoOpen>]
module Collections = 
    type Point = {x: int; y: int; z: int} with
        static member Zero = {x=0; y=0; z=0}
        static member (+) (c1: Point, c2: Point) = {x=c1.x + c2.x; y=c1.y + c2.y; z=c1.z + c2.z} 
        static member (-) (c1: Point, c2: Point) = {x=c1.x - c2.x; y=c1.y - c2.y; z=c1.z - c2.z}
        static member (*) (c1: Point, c2: Point) = {x=c1.x * c2.x; y=c1.y * c2.y; z=c1.z * c2.z} 

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




    
