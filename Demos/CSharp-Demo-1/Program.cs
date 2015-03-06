using System;
using Trik;
using Trik.Collections;
using System.Threading;

namespace CSharp_Demo_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var robot = new Trik.Junior.Robot();
            int counter = 0;
            Console.WriteLine(robot.Sensor[Sensor.A1].Read()); 
            var motor = new Trik.Devices.PowerMotor(Motor.M1);
            while (counter < 10)
            {
                for (int i = 1; i < 4; i++)
                {
                    robot.Led.SetColor((LedColor) i);
                    motor.SetPower(i*20);
                    Thread.Sleep(500);
                }
                counter++;
            }
                
        }
    }
}
