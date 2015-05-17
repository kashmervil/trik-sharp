using System;
using System.Threading;
using Trik;
using Trik.Collections;
using Trik.Reactive;

namespace TRIK_Hunter
{
    class Program
    {
        private static int ScaleXValue(int value)
        {
            return Helpers.limit(-Constants.MaxAngleX, Constants.MaxAngleX, value)/Constants.ScaleConstX;
        }

        private static int ScaleYValue(int value)
        {
            return Helpers.limit(-Constants.MaxAngleY, Constants.MaxAngleY, value)/Constants.ScaleConstY;
        }

        static void Main()
        {
            var model = new Model{ ObjectSensorConfig = VideoSource.USB };
            model.ServosConfig[ServoKey.C1] = Defaults.Servo3;
            model.ServosConfig[ServoKey.C2] = Defaults.Servo3;

            var sensor = model.ObjectSensor;
            var sensorOutput = sensor.ToObservable();
            var buttons = model.Buttons;
            var exit = new EventWaitHandle(false, EventResetMode.AutoReset);

            var targetStream = sensorOutput
                .Where(o => o.IsTarget).Select(o => o.TryGetTarget.Value);

            var setterDisposable = targetStream.Subscribe(sensor.SetDetectTarget);

            var locationStream = sensor.ToObservable()
                .Where(o => o.IsLocation).Select(o => o.TryGetLocation.Value)
                .Where(loc => loc.Mass > Constants.MinMass);

            var xPowerSetter = locationStream
                .Select(loc => loc.X)
                .Scan(0, (acc, x) => Helpers.limit(-90, 90, acc) + ScaleXValue(x))
                .Subscribe(model.Servos[ServoKey.C1]);

            var yPowerSetter = locationStream
                .Select(loc => loc.Y)
                .Scan(0, (acc, y) => Helpers.limit(-90, 90, acc) + ScaleYValue(y))
                .Subscribe(model.Servos[ServoKey.C2]);

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
            Helpers.SendToShell(@"v4l2-ctl -d ""/dev/video2"" --set-ctrl white_balance_temperature_auto=1");

            exit.WaitOne();
        }
    }
}
