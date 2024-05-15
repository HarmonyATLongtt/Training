using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using ClassLibrary2.Utils;
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

            RebarSetData topData = CaculateBeamTopBotStandard(beam, beam.Reinforcing.AsTop, asDatas, shapes, stirrupSet, true);
            RebarSetData botData = CaculateBeamTopBotStandard(beam, beam.Reinforcing.AsBot, asDatas, shapes, stirrupSet, false);
            // rebar origin phụ thuộc vào thép lớp trên hay là thép lớp dưới, nên khi có dữ liệu riêng của topdata và botdata thì mới add thêm origin data vào sau
            return new List<RebarSetData>()
            {
                topData,
                botData,
            };
        }

        private RebarSetData CaculateBeamTopBotStandard(ConcreteBeamData beam,
                                                     double calculatedAS,
                                                     List<ASData> asDatas,
                                                     List<RebarShape> shapes,
                                                     RebarSetData stirrupSet,
                                                     bool istopRebar)
        {
            var standardAS = beam.Dimensions.b * beam.Dimensions.h * MIN_STEEL_AREA_RATIO;
            var minAS = asDatas.FirstOrDefault(x => x.AS >= calculatedAS && x.AS >= standardAS);
            if (minAS != null)
            {
                double stirrup = stirrupSet.Rebartype.BarDiameter;
                double standard = minAS.BarType.BarDiameter;
                double standardnumber = minAS.Quantity;
                var layout = new RebarLayoutData()
                {
                    Spacing = (beam.Dimensions.b - 2 * (beam.Covers.Side + stirrup) - standard * standardnumber) / (standardnumber - 1) + standard,
                    Number = minAS.Quantity,
                };

                RebarSetData data = new RebarSetData();
                data.LayoutData = layout;
                data.Rebartype = minAS.BarType;
                data.ShapeData = CalculateBeamStandardShape(beam, shapes);
                data.Style = RebarStyle.Standard;
                data.LocationData.RebarOrigin = BotBeamStandardOrigin(beam.Host as FamilyInstance, beam, data, stirrup, istopRebar);

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

        public XYZ BotBeamStandardOrigin(FamilyInstance elem, ConcreteBeamData beam, RebarSetData rebar, double stirrup, bool isTopRebar)
        {
            double sidespacing = beam.Covers.Side + stirrup + rebar.Rebartype.BarDiameter / 2;
            // isTopRebar : true => return: beam.Covers.Top, false => Bottom
            double verticalCocer = isTopRebar ? beam.Covers.Top : beam.Covers.Bottom;
            double vertcalSpacing = verticalCocer + stirrup + rebar.Rebartype.BarDiameter / 2;

            BoundingBoxXYZ boundingbox = elem.get_BoundingBox(null);
            XYZ origin = XYZ.Zero;
            XYZ xVec = xVecBeam(elem);


            // isTopRebar: true => Max - spacing; false => Min + spacing
            double elev = isTopRebar
                       ? boundingbox.Max.Z - vertcalSpacing
                       : boundingbox.Min.Z + vertcalSpacing;

            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + beam.Covers.Side, boundingbox.Max.Y - sidespacing, elev);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + sidespacing, boundingbox.Min.Y + beam.Covers.Side, elev);
            }

            return origin;
        }

        public XYZ BeamStirrupOrigin(ConcreteBeamData beametabs)
        {
            double covertopbot = Common.Max(beametabs.Covers.Top, beametabs.Covers.Bottom);
            double coverside = beametabs.Covers.Side;
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y

            XYZ xVec = beametabs.drawdirection; // để lấy được chiều vẽ của dầm
            FamilyInstance ins = beametabs.Host as FamilyInstance;
            BoundingBoxXYZ boundingbox = ins.get_BoundingBox(null);

            XYZ origin = XYZ.Zero;

            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương X, thì phương X của family thép đai sẽ map vào phương Y
                origin = new XYZ(boundingbox.Min.X + coverside, boundingbox.Min.Y + coverside, boundingbox.Min.Z + covertopbot);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương Y, thì phương X của family thép đai sẽ map vào phương X
                origin = new XYZ(boundingbox.Min.X + coverside, boundingbox.Max.Y - coverside, boundingbox.Min.Z + covertopbot);
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

        private RebarSetData CalculateColumnStirrupsLayout(ConcreteColumnData col,
                                                           List<RebarBarType> allBarTypes,
                                                           List<RebarShape> shapes)
        {
            if (col?.Host != null && allBarTypes?.Count > 0 && shapes?.Count > 0)
            {
                var barType = allBarTypes.FirstOrDefault(x => Math.Abs(x.BarDiameter - STIRRUP_DIAMETER) <= (0.05 / 304.8));
                if (barType != null)
                {
                    BoundingBoxXYZ boundingbox = (col.Host as FamilyInstance).get_BoundingBox(null);

                    double length = col.Length - col.Covers.Side * 2 - barType.BarDiameter;
                    int quantity = (int)(Math.Floor(length / STIRRUP_SPACING));
                    double spacing = length / quantity;

                    var rebarSet = new RebarSetData();
                    rebarSet.LayoutData = new RebarLayoutData()
                    {
                        Spacing = spacing,
                        Number = quantity,
                    };
                    rebarSet.Rebartype = barType;
                    rebarSet.ShapeData = CalculateColumnStirrupShape(col, shapes);
                    rebarSet.Style = RebarStyle.StirrupTie;

                    double coverside = col.Covers.Side;
                    double covertopbot = Common.Max(col.Covers.Top, col.Covers.Bottom);
                    rebarSet.LocationData.RebarOrigin = new XYZ(boundingbox.Min.X + coverside, boundingbox.Min.Y + coverside, boundingbox.Min.Z + covertopbot); ;
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
            LocationCurve locCur = elem.Location as LocationCurve;
            Line locline = locCur.Curve as Line;

            //gán vector phương X trong definition của shape thép với vector vẽ cấu kiện
            if (locline != null)
                return locline.Direction;
            return XYZ.Zero;
        }
    }
}