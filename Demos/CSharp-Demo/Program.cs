using System.Reactive.Linq;
using Trik;

namespace Demo_cs
{
    class Program
    {
        static void Main()
        {
            var model = new Model();
            var rWheel = model.Motors[MotorPort.M1];
            var lWheel = model.Motors[MotorPort.M2];
            var wheelStream = model.AnalogSensors[SensorPort.A1]
                              .ToObservable().Select( x => (x < 350) ? 0 : 100)
                              .DistinctUntilChanged();
            wheelStream.Subscribe(rWheel);
            wheelStream.Subscribe(lWheel);
        }
    }
}
