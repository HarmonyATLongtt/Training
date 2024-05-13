using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            try
            {
                ///Create Wall
                //XYZ start = uidoc.Selection.PickPoint();
                //XYZ end = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, start, end);
                //XYZ poin3 = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, end, poin3);
                //XYZ poin4 = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, poin3, poin4);
                //XYZ poin5 = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, poin4, poin5);
                CreateWallTwoPoint(doc, uidoc);
            }
            catch
            {
                message = "Unexpected Exception thrown.";
                return Autodesk.Revit.UI.Result.Failed;
            }
            return Result.Succeeded;
        }

        public void CreateWallTwoPoint(Document doc, UIDocument uiDoc)
        {
            try
            {
                using (TransactionGroup tg = new TransactionGroup(doc))
                {
                    if (tg.Start() == TransactionStatus.Started)
                    {
                        XYZ startPoint = null;
                        XYZ endPoint = null;

                        while (true)
                        {
                            try
                            {
                                XYZ pickedPoint = uiDoc.Selection.PickPoint("Pick a point for the wall or press ESC to finish.");

                                if (pickedPoint == null)
                                    break;

                                if (startPoint == null)
                                {
                                    startPoint = pickedPoint;
                                }
                                else
                                {
                                    endPoint = pickedPoint;
                                    using (Transaction t = new Transaction(doc, "create wall"))
                                    {
                                        t.Start();
                                        CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uiDoc, startPoint, endPoint);
                                        t.Commit();
                                    }
                                    startPoint = endPoint;
                                    endPoint = null;
                                }
                            }
                            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                            {
                                break;
                            }
                        }
                        tg.Assimilate();
                    }
                }

                TaskDialog.Show("Đã thêm tường", "Một tường mới đã được chèn vào chế độ views 3D để xem.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "lỗi" + ex);
            }
        }

        public void CreateWallOfPickPoint(Document doc, UIDocument uidoc)
        {
            try
            {
                ///Create Wall
                Transaction t = new Transaction(doc, "Insertar wall");
                t.Start();
                //XYZ start = uidoc.Selection.PickPoint();
                //XYZ end = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, start, end);
                //XYZ poin3 = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, end, poin3);
                //XYZ poin4 = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, poin3, poin4);
                //XYZ poin5 = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, poin4, poin5);
                XYZ startPoint = null;
                XYZ endPoint = null;

                while (true)
                {
                    try
                    {
                        // Sử dụng PickPoint để lấy điểm
                        XYZ pickedPoint = uidoc.Selection.PickPoint("Pick a point for the wall or press ESC to finish.");

                        // Nếu người dùng nhấn ESC, thoát khỏi vòng lặp
                        if (pickedPoint == null)
                            break;

                        // Nếu startPoint chưa được xác định, gán pickedPoint cho startPoint
                        if (startPoint == null)
                            startPoint = pickedPoint;
                        else
                        {
                            // Ngược lại, gán pickedPoint cho endPoint và tạo tường
                            endPoint = pickedPoint;
                            CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, startPoint, endPoint);

                            // Đặt lại startPoint và endPoint để chọn điểm cho tường tiếp theo
                            startPoint = endPoint;
                            endPoint = null;
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        // Người dùng đã hủy thao tác, thoát khỏi vòng lặp
                        break;
                    }
                }
                t.Commit();
                TaskDialog.Show("Đã thêm tường", "Một tường mới đã được chèn vào chế độ views 3D để xem.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "lỗi" + ex);
            }
        }

        public Wall CreateWallUsingCurve1(Autodesk.Revit.DB.Document document, Level level, UIDocument uidoc, XYZ start, XYZ end)
        {
            // Build a location line for the wall creation

            Line geomLine = Line.CreateBound(start, end);
            // Create a wall using the location line
            return Wall.Create(document, geomLine, level.Id, true);
        }
    }
}