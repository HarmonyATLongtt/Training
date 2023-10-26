using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Text;

namespace RevitAPI_B1.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    internal class CmdRetrieving_All_Levels : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Getinfo_Level(doc);
            return Result.Succeeded;
        }

        private void Getinfo_Level(Document document)
        {
            StringBuilder levelInformation = new StringBuilder();
            int levelNumber = 0;
            FilteredElementCollector collector = new FilteredElementCollector(document);
            ICollection<Element> collection = collector.OfClass(typeof(Level)).ToElements();
            foreach (Element e in collection)
            {
                Level level = e as Level;

                if (null != level)
                {
                    // keep track of number of levels
                    levelNumber++;

                    //get the name of the level
                    levelInformation.Append("\nLevel Name: " + level.Name);

                    //get the elevation of the level
                    levelInformation.Append("\n\tElevation: " + level.Elevation);

                    // get the project elevation of the level
                    levelInformation.Append("\n\tProject Elevation: " + level.ProjectElevation);
                }
            }

            //number of total levels in current document
            levelInformation.Append("\n\n There are " + levelNumber + " levels in the document!");

            //show the level information in the messagebox
            TaskDialog.Show("Revit", levelInformation.ToString());
        }
    }
}
