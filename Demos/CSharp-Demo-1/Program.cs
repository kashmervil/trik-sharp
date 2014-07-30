using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trik;
using Trik.Config;
using System.Threading;

namespace CSharp_Demo_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var robot = new Model();
            int counter = 0;
            var motor = new Trik.PowerMotor((int)Motor.M1);
            while (counter < 10)
            {
                for (int i = 1; i < 4; i++)
                {
                    robot.Led.Color = (Collections.LedColor) i;
                    Thread.Sleep(500);
                    //robot.Accel.BlockingRead();
                }
                counter++;
            }
                
        }
    }
}
