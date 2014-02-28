namespace Trik.Observable
open Trik

type Accelerometer(config:Config.Provider.DomainTypes.Config) =
    let sensor = new Sensor3d(min = config.Sensors.Accelerometer.Min,
                              max = config.Sensors.Accelerometer.Max,
                              rate = config.Sensors.Accelerometer.Rate,
                              deviceFilePath = if Helpers.isLinux then config.Sensors.Accelerometer.DeviceFile else @"log.txt")
    do sensor.Start()
    member this.Obs = sensor.Obs
    
