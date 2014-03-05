namespace Trik.Observable
open Trik

type Gyroscope(min, max, deviceFilePath, ?rate) =
    inherit    Sensor3d(min, max, deviceFilePath, defaultArg rate <| Helpers.milliseconds 20)
 

