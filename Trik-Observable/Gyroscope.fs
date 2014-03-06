namespace Trik.Observable
open Trik
type Gyroscope(min, max, deviceFilePath, rate) =
    inherit    Sensor3d(min, max, deviceFilePath, Helpers.milliseconds rate)
 

