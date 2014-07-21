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
            Helpers.I2C.Init("/dev/i2c-2", 0x48, 1);
            var model = new Model();
            var r_wheel = model.Motor["M1"];
            var l_wheel = model.Motor["M2"];
            model.AnalogSensor["A1"].ToObservable().Scan((acc, x) =>
            {
                System.Console.WriteLine(x.ToString());
                return (x > 35) ? 0 : 100;
            }
                ).DistinctUntilChanged().Subscribe(r_wheel);
            
            model.AnalogSensor["A1"].ToObservable().Scan((acc, x) =>
            {
                System.Console.WriteLine(x.ToString());
                return (x > 35) ? 0 : 72;
            }
                ).DistinctUntilChanged().Subscribe(l_wheel);
        }
    }
}
