namespace Trik

type Accelerometer(min, max, deviceFilePath, ?rate) =
    //TODO: change config accelerometer path to symbol link (not implemented in driver)
    inherit    Sensor3d(min, max, deviceFilePath)
    
