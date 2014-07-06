namespace Trik

type Accelerometer(min, max, deviceFilePath) =
    //TODO: change config accelerometer path to symbol link (not implemented in driver)
    inherit    Sensor3d(min, max, if Helpers.isLinux then deviceFilePath else "..\BinaryComponents\accelerometer.sensordump")
    
