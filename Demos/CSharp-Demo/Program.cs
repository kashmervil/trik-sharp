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
        static void Main(string[] args)
        {
            Helpers.I2C.init("/dev/i2c-2", 0x48, 1);
            var model = new Model();//.Create("config.xml");
            var r_wheel = model.Motor["JM1"];
            var l_wheel = model.Motor["JM2"];
            model.AnalogSensor["JA1"].ToObservable().Scan((acc, x) =>
            {
                System.Console.WriteLine(x.ToString());
                return (x > 35) ? 0 : 100;
            }
                ).DistinctUntilChanged().Subscribe(r_wheel);
            
            model.AnalogSensor["JA1"].ToObservable().Scan((acc, x) =>
            {
                System.Console.WriteLine(x.ToString());
                return (x > 35) ? 0 : 72;
            }
                ).DistinctUntilChanged().Subscribe(l_wheel);
            System.Console.ReadKey();

            //obs.Subscribe(x => { Console.WriteLine(x.Item1); });
        }
    }
}
