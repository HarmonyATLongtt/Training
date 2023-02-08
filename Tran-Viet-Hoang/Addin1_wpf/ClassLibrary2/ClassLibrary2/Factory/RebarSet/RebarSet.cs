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
        private const double STIRRUP_DIAMETER = 0.0311679790026247;
        private const string STIRRUP_SHAPE_NAME = "M_T1";
        private const string STANDARD_SHAPE_NAME = "M_00";

        public RebarSet(Document doc)
        {
            _doc = doc;
        }

        #region beam

        public List<RebarSetData> CalculateBeamRebar(ConcreteBeamData beam)
        {
            List<RebarBarType> allBarTypes = GetRebarBarTypes();
            List<RebarShape> allBarShapes = GetRebarBarShapes();

            var stirrupSet = CalculateBeamStirrupsLayout(beam, allBarTypes, allBarShapes);
            var standardSet = CalculateBeamStandardLayout(beam, allBarTypes, allBarShapes, stirrupSet);

            List<RebarSetData> rebarSets = new List<RebarSetData>();
            rebarSets.Add(stirrupSet);
            rebarSets.AddRange(standardSet);
            return rebarSets;
        }

        private RebarSetData CalculateBeamStirrupsLayout(ConcreteBeamData beam, List<RebarBarType> allBarTypes, List<RebarShape> shapes)
        {
            if (beam?.Host != null && allBarTypes?.Count > 0 && shapes?.Count > 0)
            {
                var barType = allBarTypes.FirstOrDefault(x => Math.Abs(x.BarDiameter - STIRRUP_DIAMETER) <= (0.05 / 304.8));
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
                    rebarSet.ShapeData = CalculateBeamStirrupShape(beam, shapes);
                    rebarSet.Style = RebarStyle.StirrupTie;
                    rebarSet.LocationData.RebarOrigin = BeamStirrupOrigin(beam);
                    return rebarSet;
                }
            }
            return null;
        }

        private RebarShapeData CalculateBeamStirrupShape(ConcreteBeamData beam, List<RebarShape> shapes)
        {
            var shape = shapes.FirstOrDefault(x => x.Name == STIRRUP_SHAPE_NAME);
            if (shape != null)
            {
                Dictionary<string, double> segments = new Dictionary<string, double>();
                // tinh toan segment
                var b_d = beam.Dimensions.b - 2 * beam.Covers.Side;
                var c_e = beam.Dimensions.h - 2 * beam.Covers.Top;
                segments.Add("B", b_d);
                segments.Add("C", c_e);
                segments.Add("D", b_d);
                segments.Add("E", c_e);

                RebarShapeData shapeData = new RebarShapeData(shape, segments);
                return shapeData;
            }
            return null;
        }

        private List<RebarSetData> CalculateBeamStandardLayout(ConcreteBeamData beam,
                                                          List<RebarBarType> barTypes,
                                                          List<RebarShape> shapes,
                                                          RebarSetData stirrupSet)
        {
            double stirrupDiameter = stirrupSet.Rebartype.BarDiameter;
            double length = beam.Dimensions.b - 2 * beam.Covers.Side - 2 * stirrupDiameter;
            double beamsectionArea = beam.Dimensions.b * beam.Dimensions.h;
            var asDatas = barTypes.ConvertAll(x => new ASData(x, length, MIN_REBAR_SPACING));

            RebarSetData topData = CaculateBeamTopBotStandard(beam, beam.Reinforcing.AsTop, asDatas, shapes, stirrupSet);
            RebarSetData botData = CaculateBeamTopBotStandard(beam, beam.Reinforcing.AsBot, asDatas, shapes, stirrupSet);
            topData.LocationData.RebarOrigin = TopBeamStandardOrigin(beam.Host as FamilyInstance, beam, topData, stirrupDiameter);
            botData.LocationData.RebarOrigin = BotBeamStandardOrigin(beam.Host as FamilyInstance, beam, botData, stirrupDiameter);
            return new List<RebarSetData>()
            {
                topData,
                botData
            };
        }

        private RebarSetData CaculateBeamTopBotStandard(ConcreteBeamData beam,
                                                     double AS,
                                                     List<ASData> asDatas,
                                                     List<RebarShape> shapes,
                                                     RebarSetData stirrupSet)
        {
            var minAS = asDatas.FirstOrDefault(x => x.AS >= AS && x.AS >= beam.Dimensions.b * beam.Dimensions.h * MIN_STEEL_AREA_RATIO);
            if (minAS != null)
            {
                double stirrup = stirrupSet.Rebartype.BarDiameter;
                double standard = minAS.BarType.BarDiameter;
                double standardnumber = minAS.Quantity;
                var layout = new RebarLayoutData()
                {
                    Spacing = (beam.Dimensions.b - 2 * (beam.Covers.Side + stirrup) - standard) / standardnumber + standard,
                    Number = minAS.Quantity,
                    MinSpacing = MIN_REBAR_SPACING,
                };

                RebarSetData data = new RebarSetData();
                data.LayoutData = layout;
                data.Rebartype = minAS.BarType;
                data.ShapeData = CalculateBeamStandardShape(beam, shapes);
                data.Style = RebarStyle.Standard;

                return data;
            }
            return null;
        }

        private RebarShapeData CalculateBeamStandardShape(ConcreteBeamData beam, List<RebarShape> shapes)
        {
            var shape = shapes.FirstOrDefault(x => x.Name == STANDARD_SHAPE_NAME);
            if (shape != null)
            {
                Dictionary<string, double> segments = new Dictionary<string, double>();
                // tinh toan segment
                var b_d = beam.Dimensions.b - 2 * beam.Covers.Side;
                segments.Add("B", b_d);

                RebarShapeData shapeData = new RebarShapeData(shape, segments);
                return shapeData;
            }
            return null;
        }

        public XYZ BotBeamStandardOrigin(FamilyInstance elem, ConcreteBeamData beam, RebarSetData rebar, double stirrup)
        {
            double sidespacing = beam.Covers.Side + stirrup + rebar.Rebartype.BarDiameter / 2;
            double topspacing = beam.Covers.Top + stirrup + rebar.Rebartype.BarDiameter / 2;
            double botspacing = beam.Covers.Bottom + stirrup + rebar.Rebartype.BarDiameter / 2;
            BoundingBoxXYZ boundingbox = elem.get_BoundingBox(null);
            XYZ origin = XYZ.Zero;
            XYZ xVec = xVecBeam(elem);
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + sidespacing, boundingbox.Max.Y - sidespacing, boundingbox.Min.Z + botspacing);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + sidespacing, boundingbox.Min.Y + sidespacing, boundingbox.Min.Z + botspacing);
            }

            return origin;
        }

        public XYZ TopBeamStandardOrigin(FamilyInstance elem, ConcreteBeamData beam, RebarSetData rebar, double stirrup)
        {
            double sidespacing = beam.Covers.Side + stirrup + rebar.Rebartype.BarDiameter / 2;
            double topspacing = beam.Covers.Top + stirrup + rebar.Rebartype.BarDiameter / 2;
            double botspacing = beam.Covers.Bottom + stirrup + rebar.Rebartype.BarDiameter / 2;

            BoundingBoxXYZ boundingbox = elem.get_BoundingBox(null);
            XYZ origin = XYZ.Zero;
            // lấy về phương của thép dọc nằm trong elem
            XYZ xVec = xVecBeam(elem);

            // điều kiện kiểm tra nếu thép đặt theo phương X
            // khoảng cách của thép với bề mặt cấu kiện theo phương X hay Y luôn luôn là cách theo cover side
            // chỉ có khoảng cách thép với bề mặt cấu kiện theo phương Z mới theo cover top bot, thứ mà etabs đầu ra cung cấp
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + sidespacing, boundingbox.Max.Y - sidespacing, boundingbox.Max.Z - topspacing);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + sidespacing, boundingbox.Min.Y + sidespacing, boundingbox.Max.Z - topspacing);
            }

            return origin;
        }

        #endregion beam

        #region column

        public List<RebarSetData> CalculateColumnRebar(ConcreteColumnData col)
        {
            List<RebarBarType> allBarTypes = GetRebarBarTypes();
            List<RebarShape> allBarShapes = GetRebarBarShapes();

            var stirrupSet = CalculateColumnStirrupsLayout(col, allBarTypes, allBarShapes);

            List<RebarSetData> rebarSets = new List<RebarSetData>();
            rebarSets.Add(stirrupSet);
            return rebarSets;
        }

        private RebarSetData CalculateColumnStirrupsLayout(ConcreteColumnData col, List<RebarBarType> allBarTypes, List<RebarShape> shapes)
        {
            if (col?.Host != null && allBarTypes?.Count > 0 && shapes?.Count > 0)
            {
                var barType = allBarTypes.FirstOrDefault(x => Math.Abs(x.BarDiameter - STIRRUP_DIAMETER) <= (0.05 / 304.8));
                if (barType != null)
                {
                    BoundingBoxXYZ boundingbox = (col.Host as FamilyInstance).get_BoundingBox(null);
                    double cover = col.Covers.Side;
                    double length = col.Length - col.Covers.Side * 2 - barType.BarDiameter;
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
                    rebarSet.ShapeData = CalculateColumnStirrupShape(col, shapes);
                    rebarSet.Style = RebarStyle.StirrupTie;
                    rebarSet.LocationData.RebarOrigin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover); ;
                    return rebarSet;
                }
            }
            return null;
        }

        private RebarShapeData CalculateColumnStirrupShape(ConcreteColumnData col, List<RebarShape> shapes)
        {
            var shape = shapes.FirstOrDefault(x => x.Name == STIRRUP_SHAPE_NAME);
            if (shape != null)
            {
                Dictionary<string, double> segments = new Dictionary<string, double>();
                // tinh toan segment
                var b_d = col.Dimensions.b - 2 * col.Covers.Side;
                var c_e = col.Dimensions.h - 2 * col.Covers.Side;
                segments.Add("B", b_d);
                segments.Add("C", c_e);
                segments.Add("D", b_d);
                segments.Add("E", c_e);

                RebarShapeData shapeData = new RebarShapeData(shape, segments);
                return shapeData;
            }
            return null;
        }

        public XYZ BeamStirrupOrigin(ConcreteBeamData beametabs)
        {
            double coverbot = beametabs.Covers.Top;
            double coverside = beametabs.Covers.Side;
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y

            XYZ xVec = beametabs.drawdirection; // để lấy được chiều vẽ của dầm
            FamilyInstance ins = beametabs.Host as FamilyInstance;
            BoundingBoxXYZ boundingbox = ins.get_BoundingBox(null);

            XYZ origin = XYZ.Zero;

            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương X, thì phương X của family thép đai sẽ map vào phương Y
                origin = new XYZ(boundingbox.Min.X + coverside, boundingbox.Min.Y + coverside, boundingbox.Min.Z + coverbot);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương Y, thì phương X của family thép đai sẽ map vào phương X
                origin = new XYZ(boundingbox.Min.X + coverside, boundingbox.Max.Y - coverside, boundingbox.Min.Z + coverbot);
            }
            return origin;
        }

        #endregion column

        // Get all type of rebar existing in revit
        private List<RebarBarType> GetRebarBarTypes()
        {
            return new FilteredElementCollector(_doc)
                        .WhereElementIsElementType()
                        .OfClass(typeof(RebarBarType))
                        .Cast<RebarBarType>()
                        .ToList();
        }

        // Get all kind of shape existing in revit
        private List<RebarShape> GetRebarBarShapes()
        {
            return new FilteredElementCollector(_doc)
                        .WhereElementIsElementType()
                        .OfClass(typeof(RebarShape))
                        .Cast<RebarShape>()
                        .ToList();
        }

        public XYZ xVecBeam(FamilyInstance elem)
        {
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = elem.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;

            //gán vector phương X trong definition của shape thép với vector vẽ cấu kiện
            XYZ xVec = locline.Direction;
            return xVec;
        }
    }
}