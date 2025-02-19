using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace FirstCommand
{
    [Transaction(TransactionMode.Manual)]
    public class CreateDim : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            Reference pickRef = uiapp.ActiveUIDocument.Selection.PickObject(ObjectType.Element, "Select wall to dimension");
            Element selectedElem = doc.GetElement(pickRef);

            if (selectedElem is Wall)
            {
                Wall selectedWall = selectedElem as Wall;
                ReferenceArray referenceArray = new ReferenceArray();
                Reference r1 = null, r2 = null;

                Face wallFace = GetFace(selectedWall, selectedWall.Orientation);
                EdgeArrayArray edgeArrays = wallFace.EdgeLoops;
                EdgeArray edges = edgeArrays.get_Item(0);

                List<Edge> edgeList = new List<Edge>();
                foreach (Edge edge in edges)
                {
                    Line line = edge.AsCurve() as Line;

                    if (IsLineVertical(line) == true)
                    {
                        edgeList.Add(edge);
                    }
                }

                List<Edge> sortedEdges = edgeList.OrderByDescending(e => e.AsCurve().Length).ToList();

                r1 = sortedEdges[0].Reference;
                r2 = sortedEdges[1].Reference;

                referenceArray.Append(r1);
                referenceArray.Append(r2);
                // create dimension line
                LocationCurve wallLoc = selectedWall.Location as LocationCurve;
                Line wallLine = wallLoc.Curve as Line;

                XYZ offset1 = GetOffsetByWallOrientation(wallLine.GetEndPoint(0), selectedWall.Orientation, 5);
                XYZ offset2 = GetOffsetByWallOrientation(wallLine.GetEndPoint(1), selectedWall.Orientation, 5);

                Line dimLine = Line.CreateBound(offset1, offset2);

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Create new dimension");

                    Dimension newDim = doc.Create.NewDimension(doc.ActiveView, dimLine, referenceArray);

                    t.Commit();
                }
            }
            else
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }
        //Hàm GetOffsetByWallOrientation được sử dụng để tính toán một điểm mới(returnPoint)
        //bằng cách dịch chuyển một điểm ban đầu(point) theo một hướng(orientation)
        //với khoảng cách được xác định bởi một giá trị(value).
        private XYZ GetOffsetByWallOrientation(XYZ point, XYZ orientation, int value)
        {
            XYZ newVector = orientation.Multiply(value);
            XYZ returnPoint = point.Add(newVector);

            return returnPoint;
        }

        //Hàm kiểm tra:
        //Đường thẳng đứng: Hướng của đường phải trùng với trục Z(lên hoặc xuống).
        //Kết quả trả về:
        //true: Nếu đường là thẳng đứng.
        //false: Nếu không.
        private bool IsLineVertical(Line line)
        {
            if (line.Direction.IsAlmostEqualTo(XYZ.BasisZ) || line.Direction.IsAlmostEqualTo(-XYZ.BasisZ))
                return true;
            else
                return false;
        }

        //Tìm tất cả các Solid từ phần tử bằng cách gọi hàm GetSolids.
        //Lặp qua các Solid, và sau đó là các mặt của từng Solid.
        //Kiểm tra mặt phẳng (PlanarFace):
        //Nếu mặt là PlanarFace, kiểm tra hướng pháp tuyến của nó.
        //Nếu pháp tuyến gần như khớp với hướng được cung cấp, chọn mặt này làm kết quả.
        //Trả về mặt phẳng phù hợp hoặc null nếu không tìm thấy.
        private Face GetFace(Element selectedElem, XYZ orientation)
        {
            PlanarFace returnFace = null;
            List<Solid> solids = GetSolids(selectedElem);

            foreach (Solid solid in solids)
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace)
                    {
                        PlanarFace pf = face as PlanarFace;

                        if (pf.FaceNormal.IsAlmostEqualTo(orientation))
                            returnFace = pf;
                    }
                }
            }

            return returnFace;
        }

        //Tóm lược quy trình
        //Tạo danh sách Solid rỗng.
        //Thiết lập tùy chọn hình học để truy xuất chi tiết và tham chiếu.
        //Lấy hình học từ phần tử sử dụng get_Geometry.
        //Duyệt qua các hình học và kiểm tra xem có phải là Solid.
        //Kiểm tra tính hợp lệ của Solid (có mặt và thể tích > 0).
        //Thêm vào danh sách nếu hợp lệ.
        //Trả về danh sách Solid.
        private List<Solid> GetSolids(Element selectedElem)
        {
            List<Solid> returnList = new List<Solid>();

            Options options = new Options();
            options.ComputeReferences = true;
            options.DetailLevel = ViewDetailLevel.Fine;

            GeometryElement geomElem = selectedElem.get_Geometry(options);

            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        returnList.Add(solid);
                    }
                }
            }

            return returnList;
        }
    }
}