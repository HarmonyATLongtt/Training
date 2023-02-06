using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Factory.RebarSet
{
    public class RebarSet
    {
        private Document _doc;
        private const double MIN_REBAR_SPACING = 25 / 304.8;
        private const double MIN_STEEL_AREA_RATIO = 1e-3;
        private const double STIRRUP_SPACING = 250 / 304.8;
        private const double STIRRUP_DIAMETER = 8 / 304.8;
        private const string STIRRUP_SHAPE_NAME = "M_T1";

        public RebarSet(Document doc)
        {
            _doc = doc;
        }

        public RebarSetData TopBeamStandards(ConcreteBeamData beam)
        {
            RebarSetData allstandard = BeamStandard(beam, beam.Reinforcing.AsTop);

            return allstandard;
        }

        #region beam

        public List<RebarSetData> CalculateBeamRebar(ConcreteBeamData beam)
        {
            List<RebarBarType> allBarTypes = GetRebarBarTypes();
            List<RebarShape> allBarShapes = GetRebarBarShapes();

            var stirrupSet = CalculateStirrupsLayout(beam, allBarTypes, allBarShapes);
            var standardSet = CalculateStandardRebar();

            List<RebarSetData> rebarSets = new List<RebarSetData>();
            rebarSets.Add(stirrupSet);
            rebarSets.AddRange(standardSet);
            return rebarSets;

            ////khoảng cách thông thủy tối thiểu giữa các thanh thép lớp dưới
            //double kc = beam.HostRebar.LayoutData.MinSpacing;
            //double coverside = beam.Covers.Side;
            //double stirrup = beam.Stirrup_Tie.DiameterData.Type;
            //int[] duongkinhcautao = beam.HostRebar.DiameterData.RebarDiameterS;
            //int[] sothanh = new int[duongkinhcautao.Count()];

            //double elemb = beam.Dimensions.b;
            //double elemh = beam.Dimensions.h;
            //double Asmin = elemb * 304.8 * elemh * 304.8 * 0.05 / 100; // diện tích cốt thép tối thiểu là 0,05%
            //if (Asmin < requiredSteelArea / 1.1) { Asmin = requiredSteelArea / 1.1; } // chọn ra giá trị mà As thiết kế bắt buộc sẽ phải lớn hơn

            //RebarSetData rebarsets = new RebarSetData();
            //for (int i = 0; i < duongkinhcautao.Count(); i++)
            //{
            //    //số thanh phải nhỏ hơn hoặc bằng, nên dùng hàm Floor để lấy giá trị nguyên lớn nhất và gần kết quả nhất
            //    sothanh[i] = Convert.ToInt32(Math.Floor((elemb * 304.8 + kc - 2 * (coverside + stirrup) * 304.8) / (duongkinhcautao[i] + kc)));
            //}
            //for (int i = 0; i < sothanh.Count(); i++)
            //{
            //    if (Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i] >= Asmin)
            //    {
            //        RebarSetData rebarset = new RebarSetData();
            //        rebarset.LayoutData.Number = sothanh[i];
            //        rebarset.DiameterData.Type = duongkinhcautao[i];
            //        rebarset.LayoutData.RebarCrossSectionArea = Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i];
            //        rebarset.LayoutData.CrossSectionWidth = elemb;
            //        rebarset.LayoutData.Spacing = ((rebarset.LayoutData.CrossSectionWidth - 2 * (coverside + stirrup)) * 304.8 - rebarset.DiameterData.Type) / (rebarset.LayoutData.Number - 1);
            //        rebarsets = rebarset;
            //        break;
            //    }
            //}
            //return rebarsets;
        }

        private RebarSetData CalculateStirrupsLayout(ConcreteBeamData beam, List<RebarBarType> allBarTypes, List<RebarShape> shapes)
        {
            if (beam?.Host != null && allBarTypes?.Count > 0 && shapes?.Count > 0)
            {
                var barType = allBarTypes.FirstOrDefault(x => Math.Abs(x.BarDiameter - STIRRUP_DIAMETER) <= (0.5 / 304.8));
                if (barType != null)
                {
                    LocationCurve lc = beam.Host.Location as LocationCurve;
                    double length = lc.Curve.Length - beam.Covers.Side * 2 - barType.BarDiameter;
                    int quantity = (int)(Math.Floor(length / STIRRUP_SPACING));
                    double spacing = length / quantity;

                    var rebarSet = new RebarSetData();
                    rebarSet.LayoutData = new RebarLayoutData()
                    {
                        MinSpacing = MIN_REBAR_SPACING,
                        Spacing = spacing,
                        Number = quantity,
                    };
                    rebarSet.Rebartype = barType;
                    rebarSet.ShapeData = CalculateStirrupShape(beam, shapes);
                    rebarSet.Style = RebarStyle.StirrupTie;
                    return rebarSet;
                }
            }
            return null;
        }

        private RebarShapeData CalculateStirrupShape(ConcreteBeamData beam, List<RebarShape> shapes)
        {
            var shape = shapes.FirstOrDefault(x => x.Name == STIRRUP_SHAPE_NAME);
            if (shape != null)
            {
                Dictionary<string, double> segments = new Dictionary<string, double>();
                // tinh toan segment
                // var b = ...
                // segments.Add("B", b);

                // var b = ...
                // var b = ...
                // var b = ...

                RebarShapeData shapeData = new RebarShapeData(shape, segments);
                return shapeData;
            }
            return null;
        }

        private List<RebarSetData> CalculateStandardRebar(ConcreteBeamData beam, double requiredSteelArea, List<RebarBarType> allBarTypes)
        {
            double minSteelArea = beam.Dimensions.b * beam.Dimensions.h * MIN_STEEL_AREA_RATIO;

            // tinh as cho top va bot
        }

        private List<RebarBarType> GetRebarBarTypes()
        {
            return new FilteredElementCollector(_doc)
                        .WhereElementIsElementType()
                        .OfClass(typeof(RebarBarType))
                        .Cast<RebarBarType>()
                        .ToList();
        }

        private List<RebarShape> GetRebarBarShapes()
        {
            return new FilteredElementCollector(_doc)
                        .WhereElementIsElementType()
                        .OfClass(typeof(RebarShape))
                        .Cast<RebarShape>()
                        .ToList();
        }

        #endregion beam

        #region column
        public List<RebarSetData> CalculateColumnRebar(ConcreteColumnData col)
        {
            return null;
        }

        #endregion

        public RebarSetData BeamStirrup_Tie()
        {
            RebarSetData stirrup = new RebarSetData();
            return stirrup;
        }
    }
}