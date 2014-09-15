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
            var rWheel = model.Motor["M1"];
            var lWheel = model.Motor["M2"];
            model.AnalogSensor["A1"].ToObservable().Scan((acc, x) =>
            {
                System.Console.WriteLine(x.ToString());
                return (x > 35) ? 0 : 100;
            }
                ).DistinctUntilChanged().Subscribe(rWheel);
            
            model.AnalogSensor["A1"].ToObservable().Scan((acc, x) =>
            {
                System.Console.WriteLine(x.ToString());
                return (x > 35) ? 0 : 72;
            }
                ).DistinctUntilChanged().Subscribe(lWheel);
        }
    }
}
