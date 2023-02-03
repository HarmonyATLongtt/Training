using ClassLibrary2.Data;
using System;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_CaculateRebar
    {
        //hàm tính lượng thép cần bố trí
        public RebarSetData BeamStandard(ConcreteBeamData beam, double coverside, double stirrup, double Astinhtoan)
        {
            //khoảng cách thông thủy tối thiểu giữa các thanh thép lớp dưới
            double kc = beam.HostRebar.LayoutData.MinSpacing;
            int[] duongkinhcautao = beam.HostRebar.DiameterData.RebarDiameterS;
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
                    rebarset.LayoutData.Number = sothanh[i];
                    rebarset.DiameterData.Type = duongkinhcautao[i];
                    rebarset.LayoutData.RebarCrossSectionArea = Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i];
                    rebarset.LayoutData.CrossSectionWidth = elemb;
                    rebarset.LayoutData.Spacing = ((rebarset.LayoutData.CrossSectionWidth - 2 * (coverside + stirrup)) * 304.8 - rebarset.DiameterData.Type) / (rebarset.LayoutData.Number - 1);
                    rebarsets = rebarset;
                    break;
                }
            }
            return rebarsets;
        }
    }
}