using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTrainees.Utils
{
    public class SomeUtils
    {
        public void GetInfor<T>(T obj)
        {
            string ans = "";
            foreach (Parameter p in (obj as Element).Parameters)
            {
                ans += p.Definition.Name + ": " + p.AsValueString() + "\n";
            }
            TaskDialog.Show("Parameter of " + (obj as Element).Name, ans);
        }

        public void SetComments<T>(T obj, string comment)
        {
            Parameter p = (obj as Element).get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            p.Set(comment);
            TaskDialog.Show("Message", "Comment parameter of " + (obj as Element).Name + " just had set...");
        }
    }
}