namespace Trik.Observable
open Trik

type Accelerometer(config:Config.Provider.DomainTypes.Accelerometer) =
    inherit    Sensor3d(min = config.Min,
                        max = config.Max,
                        rate = config.Rate,
                        deviceFilePath = if Helpers.isLinux then config.DeviceFile else @"log.txt")
    
