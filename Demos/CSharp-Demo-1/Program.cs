using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trik;
using System.Threading;

namespace CSharp_Demo_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var robot = new Model();
            int counter = 0;
            //var led = new Trik.Led("/sys/class/leds/");

            while (counter < 1000)
            {
                for (int i = 0; i < 4; i++)
                {
                    robot.Led.Color = (Trik.LedColor) i;
                    Thread.Sleep(500);
                }
                counter++;
            }
                
        }
    }
}
