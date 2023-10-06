using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Utils;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateRoom : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            XYZ p = uiDoc.Selection.PickPoint("Pick a point in closet shape to place room");

            UV point = new UV(p.X, p.Y);

            try
            {
                using (var trans = new Transaction(doc, "Create new room"))
                {
                    trans.Start();

                    var room = doc.Create.NewRoom(doc.ActiveView.GenLevel, point);

                    if (room == null)
                        TaskDialog.Show("Message", "Create room is failed...");
                    else
                    {
                        doc.Create.NewRoomTag(new LinkElementId(new ElementId(room.Id.IntegerValue)), point, doc.ActiveView.Id);
                    }

                    new SomeUtils().GetInfor(room);
                    new SomeUtils().SetComments(room, "Some comment was set in here...");

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}