using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8
{
    internal class Car
    {
        public string color;
        public string model;
        public int maxSpeed;

        public Car(string carModel, string carColor, int carSpeed)
        {
            model = carModel;
            color = carColor;
            maxSpeed = carSpeed;
        }

        internal string ToString(string v)
        {
            throw new NotImplementedException();
        }

        public void Infor()
        {
            //Console.WriteLine("Mau: {0}, nam san xuat: {1}, toc do toi da: {2}", color,, maxSpeed);
        }
    }
}