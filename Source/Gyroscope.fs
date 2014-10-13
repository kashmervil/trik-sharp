namespace Trik.Sensors
type Gyroscope(min, max, deviceFilePath) =
    inherit Trik.Internals.Sensor3d(min, max, if Trik.Helpers.isLinux then deviceFilePath else "..\BinaryComponents\gyroscope.sensordump")
 

