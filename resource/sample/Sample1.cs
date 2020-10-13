/*
 * C# Program to Demonstrate Multilevel Inheritance
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inherit
{
    class Sample : vehicle
    {
        public void Noise()
        {
            Console.WriteLine("All Vehicles Creates Noise !! ");
        }

        static void Main(string[] args)
        {
            inheri obj = new inheri();
            obj.mode();
            obj.feature();
            obj.Noise();
            Console.Read();
        }
    }

    class Mode
    {
        public void mode()
        {
            Console.WriteLine("There are Many Modes of Transport !!");
        }
    }

    class vehicle : Mode
    {
        public void feature()
        {
            Console.WriteLine("They Mainly Help in Travelling !!");
        }
    }
}
