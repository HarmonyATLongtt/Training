using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vi_Du_Nap_Chong_Toan_Tu
{
    class Hop
    {
        double chieu_Dai;
        double chieu_Rong;
        double chieu_Cao;

        public double TinhTheTich()
        {
            return chieu_Dai * chieu_Rong * chieu_Cao;
        }

        public void GanChieuDai(double dai)
        {
            chieu_Dai = dai;
        }
        public void GanChieuRong(double rong)
        {
            chieu_Rong = rong;
        }
        public void GanChieuCao(double cao)
        {
            chieu_Cao = cao;
        }
        public static Hop operator +(Hop b, Hop c)
        {
            Hop hopA = new Hop();
            hopA.chieu_Dai = b.chieu_Dai + c.chieu_Dai;
            hopA.chieu_Rong = b.chieu_Rong + c.chieu_Rong;
            hopA.chieu_Cao = b.chieu_Cao + c.chieu_Cao;
            return hopA;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Hop Hop1 = new Hop();   
            Hop Hop2 = new Hop();   
            Hop Hop3 = new Hop();   
            double TheTich = 0.0;

            Hop1.GanChieuDai(6.0);
            Hop1.GanChieuRong(7.0);
            Hop1.GanChieuCao(5.0);

            Hop2.GanChieuDai(15.0);
            Hop2.GanChieuRong(6.0);
            Hop2.GanChieuCao(9.0);

            TheTich = Hop1.TinhTheTich();
            Console.WriteLine("the tich cua Hop1: {0}", TheTich);

            TheTich = Hop2.TinhTheTich();
            Console.WriteLine("the tich cua Hop2: {0}", TheTich);

            Hop3 = Hop1 + Hop2;

            TheTich = Hop3.TinhTheTich();
            Console.WriteLine("the tich cua Hop3: {0}", TheTich);
            Console.ReadKey();
        }
    }
}
