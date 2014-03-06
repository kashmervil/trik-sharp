namespace Trik.Observable
open Trik

type Accelerometer(min, max, deviceFilePath, ?rate) =
    //TODO: change config accelerometer path to symbol link (not implemented in driver)
    inherit    Sensor3d(min, max, deviceFilePath, defaultArg rate <|Helpers.milliseconds 20)
    
