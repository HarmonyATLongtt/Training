using ClassLibrary2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2.Factory.RebarSet
{
    public class RebarSet
    {

        public RebarSetData TopBeamStandards (ConcreteBeamData beam) 
        {
            RebarSetData allstandard =  BeamStandard(beam, beam.Reinforcing.AsTop);
           

            return allstandard;
        }
        public RebarSetData BeamStandard (ConcreteBeamData beam, double Astinhtoan) 
        {
            //khoảng cách thông thủy tối thiểu giữa các thanh thép lớp dưới
            double kc = beam.HostRebar.MinSpacing;
            double coverside = beam.Covers.Side;
            double stirrup = beam.Stirrup_Tie.Type;
            int[] duongkinhcautao = beam.HostRebar.RebarDiameter;
            int[] sothanh = new int[duongkinhcautao.Count()];

            double elemb = beam.Dimensions.b;
            double elemh = beam.Dimensions.h;
            double Asmin = elemb * 304.8 * elemh * 304.8 * 0.05 / 100; // diện tích cốt thép tối thiểu là 0,05%
            if (Asmin < Astinhtoan / 1.1) { Asmin = Astinhtoan / 1.1; } // chọn ra giá trị mà As thiết kế bắt buộc sẽ phải lớn hơn

            RebarSetData rebarsets = new RebarSetData();
            for (int i = 0; i < duongkinhcautao.Count(); i++)
            {
                //số thanh phải nhỏ hơn hoặc bằng, nên dùng hàm Floor để lấy giá trị nguyên lớn nhất và gần kết quả nhất
                sothanh[i] = Convert.ToInt32(Math.Floor((elemb * 304.8 + kc - 2 * (coverside + stirrup) * 304.8) / (duongkinhcautao[i] + kc)));
            }
            for (int i = 0; i < sothanh.Count(); i++)
            {
                if (Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i] >= Asmin)
                {
                    RebarSetData rebarset = new RebarSetData();
                    rebarset.Number = sothanh[i];
                    rebarset.Type = duongkinhcautao[i];
                    rebarset.RebarCrossSectionArea = Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i];
                    rebarset.CrossSectionWidth = elemb;
                    rebarset.Spacing = ((rebarset.CrossSectionWidth - 2 * (coverside + stirrup)) * 304.8 - rebarset.Type) / (rebarset.Number - 1);
                    rebarsets = rebarset;
                    break;
                }
            }
            return rebarsets;
        }

        public RebarSetData BeamStirrup_Tie() 
        {
            RebarSetData stirrup = new RebarSetData();
            return stirrup;
        }
    }
}
