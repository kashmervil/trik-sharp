namespace Trik.Observable
open Trik

type Gyroscope(config:Config.Provider.DomainTypes.Gyroscope) =
    inherit    Sensor3d(min = config.Min,
                        max = config.Max,
                        rate = config.Rate,
                        deviceFilePath = if Helpers.isLinux then config.DeviceFile else @"log.txt")
 

