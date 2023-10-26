using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace RevitAPI_B1.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdToDuc1 : IExternalCommand
    {
        [Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;
                //Chọn đối tượng bằng pickobject
                IList<Reference> listRe = uiDoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
                //Lấy kiểu đơn vị độ dài
                FormatOptions formatOptions = doc.GetUnits().GetFormatOptions(UnitType.UT_Length);
                DisplayUnitType displayUnitType = formatOptions.DisplayUnits;

                using (TransactionGroup transg = new TransactionGroup(doc, "group1"))
                {
                    transg.Start();
                    //Duyệt qua từng model line
                    foreach (Reference reference in listRe)
                    {
                        ElementId elementid = reference.ElementId;
                        Element element = doc.GetElement(elementid);
                        if (element is ModelLine)
                        {
                            ModelLine line = element as ModelLine;

                            using (Transaction trs = new Transaction(doc, "Create_TextNode"))
                            {
                                trs.Start();
                                CreateText(doc, line, displayUnitType);
                                trs.Commit();
                            }

                        }
                    }
                    transg.Assimilate();
                }
            }
            catch(Exception ex)
            {
                //TaskDialog.Show("Revit", "Có lỗi xảy ra!");
                return Result.Cancelled;
            }
            
            return Result.Succeeded;
        }

        /// <summary>
        /// Tạo text note theo theo yêu cầu
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="line">Model line cần tạo text note</param>
        /// <param name="displayUnitType">Kiểu đơn vị hiển thị </param>
        /// <returns></returns>
        [Obsolete]
        public TextNote CreateText(Document doc, ModelLine line, DisplayUnitType displayUnitType)
        {
            //Lấy điểm đầu và điểm cuối của line
            Curve curve = line.GeometryCurve;
            XYZ startPoint = curve.GetEndPoint(0);
            XYZ endPoint = curve.GetEndPoint(1);
            //Trung điểm của line
            XYZ td = (startPoint + endPoint) / 2.0;
            //Độ dài của line
            double kc = Math.Sqrt((startPoint.X - endPoint.X) * (startPoint.X - endPoint.X) + (startPoint.Y - endPoint.Y) * (startPoint.Y - endPoint.Y));
            kc = Math.Round(kc, 2);
            //Chuyển đổi độ dài sang đúng kiểu dữ liệu
            double convertedLength = UnitUtils.ConvertFromInternalUnits(kc, displayUnitType);
            //Lấy điểm nào có x nhỏ hơn làm điểm ngọn và x lớn hơn làm gốc
            XYZ point1, point2;
            if (startPoint.X < endPoint.X)
            {
                point1 = startPoint;
                point2 = endPoint;
            }
            else
            {
                point1 = endPoint;
                point2 = startPoint;
            }
            //Vector chỉ phương của model line
            XYZ vector1 = point1 - point2;
            ElementId defaultTextTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
            double minWidth = TextNote.GetMinimumAllowedWidth(doc, defaultTextTypeId);
            double noteWidth = 0.2;
            double maxWidth = TextNote.GetMaximumAllowedWidth(doc, defaultTextTypeId);
            if (noteWidth < minWidth)
            {
                noteWidth = minWidth;
            }
            else if (noteWidth > maxWidth)
            {
                noteWidth = maxWidth;
            }
            //vector chỉ phương của text note lấy gốc x nhỏ hơn của 2 điểm line 
            XYZ vector2 = new XYZ(point1.X, 0, 0);
            // chuẩn hóa vector chi phuong model line
            XYZ normalizedVector1 = vector1.Normalize();
            //XYZ normalizedVector2 = vector2.Normalize(); 
            // góc giữa 2 vector
            double angle = vector2.AngleTo(normalizedVector1);
            TextNoteOptions opts = new TextNoteOptions(defaultTextTypeId);
            opts.HorizontalAlignment = HorizontalTextAlignment.Center;

            //th bên phải trục y
            if (point1.X > 0)
            {
                if (point1.X < point2.X && point1.Y > point2.Y)
                    opts.Rotation = angle;
                else
                    opts.Rotation = -angle;

            }
            //th còn lại
            else //if (point2.X <= 0)
            {
                //th trên đường thẳng nằm ngang
                if (Math.Round(point1.Y, 5) == Math.Round(point2.Y, 5))
                {
                    //TaskDialog.Show("revit", (Math.Round(point1.Y, 5) == Math.Round(point2.Y, 5)).ToString());
                    opts.Rotation = Math.PI;
                }
                else if (point1.X < point2.X && point1.Y > point2.Y)
                    opts.Rotation = -angle + Math.PI;
                else if (point1.X < point2.X && point1.Y < point2.Y)
                    opts.Rotation = angle + Math.PI;
                else
                    opts.Rotation = angle;
            }

            TextNote textNote = TextNote.Create(doc, doc.ActiveView.Id, td, noteWidth, kc.ToString(), opts);
            return textNote;
        }

    }

}
