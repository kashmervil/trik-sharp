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
            var model =  Model.Create("config.xml");
            var obs = model.Gyro.ToObservable();
            obs.Subscribe(x => { Console.WriteLine(x.Item1); });
        }
    }
}
