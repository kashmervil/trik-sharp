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
                    robot.Led.SetColor((LedColor) i);
                    motor.SetPower(i*20);
                    Thread.Sleep(500);
                }
                counter++;
            }
                
        }
    }
}
