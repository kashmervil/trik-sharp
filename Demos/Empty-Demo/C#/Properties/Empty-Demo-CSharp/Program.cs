using System;
using System.Threading;
using Trik;
using Trik.Devices;
using Trik.Junior;

namespace CSharp_Demo_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var robot = new Robot();
            int counter = 0;
            Console.WriteLine(robot.Sensor[SensorPort.A1].Read());
            var motor = new PowerMotor(MotorPort.M1);
            while (counter < 10)
            {
                for (int i = 1; i < 4; i++)
                {
                    var color = (LedColor)i;
                    robot.Led.SetColor(color);
                    Console.WriteLine(color.ToString());
                    Thread.Sleep(500);
                }
                counter++;
            }

        }
    }
}
