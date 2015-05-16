using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trik;

namespace Demo_cs
{
    class Program
    {
        static void Main()
        {
            var model = new Model();
            var rWheel = model.Motors[Motor.M1];
            var lWheel = model.Motors[Motor.M2];
            var wheelStream = model.AnalogSensors[Sensor.A1]
                              .ToObservable().Select( x => (x < 350) ? 0 : 100)
                              .DistinctUntilChanged();
            wheelStream.Subscribe(rWheel);
            wheelStream.Subscribe(lWheel);
        }
    }
}
