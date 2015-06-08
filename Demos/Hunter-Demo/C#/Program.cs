using System;
using System.Threading;
using Trik;
using Trik.Helpers;
using Trik.Collections;
using Trik.Reactive;

namespace TRIK_Hunter
{
    class Program
    {
        private static int ScaleXValue(int value)
        {
            return Calculations.Limit(-Constants.MaxAngleX, Constants.MaxAngleX, value)/Constants.ScaleConstX;
        }

        private static int ScaleYValue(int value)
        {
            return Calculations.Limit(-Constants.MaxAngleY, Constants.MaxAngleY, value)/Constants.ScaleConstY;
        }

        static void Main()
        {
            var model = new Model{ ObjectSensorConfig = VideoSource.USB };
            model.ServosConfig[ServoPort.C1] = Defaults.Servo3;
            model.ServosConfig[ServoPort.C2] = Defaults.Servo3;

            var sensor = model.ObjectSensor;
            var sensorOutput = sensor.ToObservable();

            var locationStream = from data in sensorOutput
                                 where data.IsLocation
                                 let loc = data.GetLocation
                                 where loc.Mass > Constants.MinMass
                                 select loc;
            //var locationStream = sensor.ToObservable()
            //                    .Where(o => o.IsLocation).Select(o => o.GetLocation)
            //                    .Where(loc => loc.Mass > Constants.MinMass);

            var xPowerSetter = locationStream
                .Select(loc => -loc.X)
                .Scan(0, (acc, x) => Calculations.Limit(-90, 90, acc) + ScaleXValue(x))
                .Subscribe(model.Servos[ServoPort.C1]);

            var yPowerSetter = locationStream
                .Select(loc => loc.Y)
                .Scan(0, (acc, y) => Calculations.Limit(-90, 90, acc) + ScaleYValue(y))
                .Subscribe(model.Servos[ServoPort.C2]);
            
            
            var buttons = model.Buttons;
            var exit = new EventWaitHandle(false, EventResetMode.AutoReset);

            var targetStream = from data in sensorOutput
                               where data.IsTarget
                               select data.GetTarget;

            var setterDisposable = targetStream.Subscribe(sensor.SetDetectTarget);

            var observableButtons = buttons.ToObservable();

            var downButtonDetect =
                observableButtons
                    .Where(x => ButtonEventCode.Down == x.Button)
                    .Subscribe(x => sensor.Detect());

            var upButtonDispose =
                observableButtons
                .Where(x => ButtonEventCode.Up == x.Button)
                .Subscribe(x => exit.Set());

            buttons.Start();
            sensor.Start();
            Shell.Send(@"v4l2-ctl -d ""/dev/video2"" --set-ctrl white_balance_temperature_auto=1");

            exit.WaitOne();
        }
    }
}
