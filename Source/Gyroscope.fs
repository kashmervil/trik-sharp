namespace Trik
type Gyroscope(min, max, deviceFilePath) =
    inherit    Sensor3d(min, max, if Helpers.isLinux then deviceFilePath else "..\BinaryComponents\gyroscope.sensordump")
 

