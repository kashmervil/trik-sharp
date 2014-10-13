namespace Trik.Sensors
open Trik

type Accelerometer(min, max, deviceFilePath) =
    inherit Internals.Sensor3d(min, max, if Helpers.isLinux then deviceFilePath else "..\BinaryComponents\accelerometer.sensordump")
    
