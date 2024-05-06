using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Bt_thuc_hanh
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class CreateCollums : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            ICollection<Element> symbols = collector.OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsElementType().ToElements();
            FamilySymbol columnType = null;
            foreach (Element e in symbols)
            {
                FamilySymbol symbol = e as FamilySymbol;
                if (symbol.Name == "UC305x305x97")
                {
                    columnType = symbol;
                    break;
                }
            }
            FilteredElementCollector collecter = new FilteredElementCollector(doc);
            Level level = collecter.OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .First(x => x.Name == "Level 1");

            try
            {
                using (Transaction trans = new Transaction(doc, "Place Family"))
                {
                    trans.Start();
                    XYZ position = new XYZ(0, 0, 0);
                    //FamilyInstance column = doc.Create.NewFamilyInstance(position, columnType, StructuralType.NonStructural);
                    FamilyInstance column = doc.Create.NewFamilyInstance(position, columnType, level, StructuralType.Column);

                    //foreach (Parameter para in column.Parameters)
                    //{
                    //    string parameterName = para.Definition.Name;
                    //    string parameterValue = para.AsValueString();
                    //    TaskDialog.Show("Info Parameter", "parameterName :" + parameterName + "\n" + "parameterValue :" + parameterValue);
                    //}

                    //set new para
                    Parameter parameter = column.LookupParameter("Height");
                    if (parameter != null)
                    {
                        // Thiết lập giá trị mới cho tham số
                        parameter.Set(5);
                    }
                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}