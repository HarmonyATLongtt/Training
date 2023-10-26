using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    internal class Create_Text : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            using(Transaction trans = new Transaction(doc, "Create_Text"))
            {
                trans.Start();
                AddNewTextNote(uiDoc);
                trans.Commit();
            }
            return Result.Succeeded; 
        }

        public TextNote AddNewTextNote(UIDocument uiDoc)
        {
            Document doc = uiDoc.Document;
            XYZ textLoc = uiDoc.Selection.PickPoint("Pick a point for sample text.");
            ElementId defaultTextTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
            double noteWidth = .2;

            // make sure note width works for the text type
            double minWidth = TextNote.GetMinimumAllowedWidth(doc, defaultTextTypeId);
            double maxWidth = TextNote.GetMaximumAllowedWidth(doc, defaultTextTypeId);
            if (noteWidth < minWidth)
            {
                noteWidth = minWidth;
            }
            else if (noteWidth > maxWidth)
            {
                noteWidth = maxWidth;
            }

            TextNoteOptions opts = new TextNoteOptions(defaultTextTypeId);
            opts.HorizontalAlignment = HorizontalTextAlignment.Center;
            opts.Rotation = Math.PI / 1;

            TextNote textNote = TextNote.Create(doc, doc.ActiveView.Id, textLoc, noteWidth, "New Text", opts);

            return textNote;
        }
    }

}
