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
            var rWheel = model.Motor[Motor.M1];
            var lWheel = model.Motor[Motor.M2];
            model.AnalogSensor[Sensor.A1].ToObservable().Scan((acc, x) =>
            {
                System.Console.WriteLine(x.ToString());
                return (x < 350) ? 0 : 100;
            }
                ).DistinctUntilChanged().Subscribe(rWheel);
            
            model.AnalogSensor[Sensor.A1].ToObservable().Scan((acc, x) =>
            {
                System.Console.WriteLine(x.ToString());
                return (x < 350) ? 0 : 100;
            }
                ).DistinctUntilChanged().Subscribe(lWheel);
        }
    }
}
