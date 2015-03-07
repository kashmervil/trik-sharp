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
            model.AnalogSensors[Sensor.A1].ToObservable().Select( x =>
            {
                System.Console.WriteLine(x.ToString());
                return (x < 350) ? 0 : 100;
            }
                ).DistinctUntilChanged().Subscribe(rWheel);
            
            model.AnalogSensors[Sensor.A1].ToObservable().Select(x =>
            {
                System.Console.WriteLine(x.ToString());
                return (x < 350) ? 0 : 100;
            }
                ).DistinctUntilChanged().Subscribe(lWheel);
        }
    }
}
