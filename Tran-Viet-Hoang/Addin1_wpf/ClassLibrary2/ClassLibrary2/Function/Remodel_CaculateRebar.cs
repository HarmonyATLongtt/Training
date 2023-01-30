using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using System;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_CaculateRebar
    {
        //hàm tính lượng thép cần bố trí
        public RebarSetData BeamStandard(FamilyInstance beam, double cover, double stirrup, double Astinhtoan)
        {
            double kc = 25; //khoảng cách thông thủy tối thiểu giữa các thanh thép lớp dưới
            int[] duongkinhcautao = { 16, 19, 22, 25 };
            int[] sothanh = new int[4];

            Parameter elemb = beam.Symbol.LookupParameter("b");
            Parameter elemh = beam.Symbol.LookupParameter("h");
            double Asmin = elemb.AsDouble() * 304.8 * elemh.AsDouble() * 304.8 * 0.05 / 100; // diện tích cốt thép tối thiểu là 0,05%
            if (Asmin < Astinhtoan / 1.1) { Asmin = Astinhtoan / 1.1; } // chọn ra giá trị mà As thiết kế bắt buộc sẽ phải lớn hơn

            RebarSetData rebarsets = new RebarSetData();
            for (int i = 0; i < duongkinhcautao.Count(); i++)
            {
                //số thanh phải nhỏ hơn hoặc bằng, nên dùng hàm Floor để lấy giá trị nguyên lớn nhất và gần kết quả nhất
                sothanh[i] = Convert.ToInt32(Math.Floor((elemb.AsDouble() * 304.8 + kc - 2 * (cover + stirrup) * 304.8) / (duongkinhcautao[i] + kc)));
            }
            for (int i = 0; i < sothanh.Count(); i++)
            {
                if (Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i] >= Asmin)
                {
                    RebarSetData rebarset = new RebarSetData();
                    rebarset.Number = sothanh[i];
                    rebarset.Type = duongkinhcautao[i];
                    rebarset.RebarCrossSectionArea = Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i];
                    rebarset.CrossSectionWidth = elemb.AsDouble();
                    rebarset.Spacing = ((rebarset.CrossSectionWidth - 2 * (cover + stirrup)) * 304.8 - rebarset.Type) / (rebarset.Number - 1);
                    rebarsets = rebarset;
                    break;
                }
            }
            return rebarsets;
        }
    }
}
