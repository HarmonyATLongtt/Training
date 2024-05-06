using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace ExRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CheckIntersect : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //lấy đối tượng Grid
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> gridElements = collector.OfClass(typeof(Grid)).ToElements();

            int countIntersect = 0;
            XYZ saveIntersect = new XYZ(0, 0, 0);

            //var gridElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType().OfClass(typeof(Grid));
            try
            {
                for (int i = 0; i < gridElements.Count - 1; i++)
                {
                    Grid grid1 = gridElements[i] as Grid;
                    for (int j = i + 1; j < gridElements.Count; j++)
                    {
                        Grid grid2 = gridElements[j] as Grid;

                        Line line1 = grid1.Curve as Line;
                        Line line2 = grid2.Curve as Line;

                        if (line1 != null && line2 != null)
                        {
                            // Tìm giao điểm giữa hai đoạn thẳng
                            IntersectionResultArray intersectionResults;
                            SetComparisonResult result = line1.Intersect(line2, out intersectionResults);

                            if (result == SetComparisonResult.Overlap && intersectionResults.Size > 0)
                            {
                                // Có giao điểm
                                foreach (IntersectionResult intersectionResult in intersectionResults)
                                {
                                    XYZ intersectionPoint = intersectionResult.XYZPoint;
                                    saveIntersect = intersectionPoint;
                                    TaskDialog.Show("Có giao điểm", "Grid: " + gridElements[i].Name + " giao điểm với Grid: " + gridElements[j].Name
                                                    + "\n" + "Tên giao điểm: " + countIntersect
                                                    + "\n" + "Có tọa độ là: " + saveIntersect.ToString());
                                    countIntersect++;
                                }
                            }
                            else
                            {
                                TaskDialog.Show("Không có giao điểm", "Grid: " + gridElements[i].Name + " Không Có giao điểm với Grid: " + gridElements[j].Name);
                            }
                        }
                        else
                        {
                            TaskDialog.Show("Không có Grid", "Không Có Grid");
                        }
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}