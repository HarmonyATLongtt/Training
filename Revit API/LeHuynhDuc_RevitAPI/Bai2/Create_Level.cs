using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Document = Autodesk.Revit.DB.Document;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    internal class Create_Level : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> collection = collector.OfClass(typeof(Level)).ToElements();
            Level level = null;
            foreach (Element e in collection)
            {
                level = e as Level;
            }
            // The elevation to apply to the new level
            double elevation = level.Elevation + 10.0;

            using(Transaction trans = new Transaction(doc, "Create_Level"))
            {
                trans.Start();
                // Begin to create a level
                Level level1 = Level.Create(doc, elevation);
                if (null == level1)
                {
                    throw new Exception("Create a new level failed.");
                }

                // Change the level name
                //level.Name = "New level";
                trans.Commit();
            }
            


            return Result.Succeeded;
        }
    }
}
