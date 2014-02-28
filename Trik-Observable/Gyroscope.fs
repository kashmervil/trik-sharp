namespace Trik.Observable
open Trik

type Gyroscope(config:Config.Provider.DomainTypes.Config) =
    let sensor = new Sensor3d(min = config.Sensors.Gyroscope.Min,
                              max = config.Sensors.Gyroscope.Max,
                              rate = config.Sensors.Gyroscope.Rate,
                              deviceFilePath = if Helpers.isLinux then config.Sensors.Gyroscope.DeviceFile else @"log.txt")
    do sensor.Start()
    member this.Obs = sensor.Obs

