using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using ClassLibrary1.UI.ViewModel;
using ClassLibrary1.UI.Views;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Windows;
using ClassLibrary1;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Creation;

namespace ClassLibrary1
{
   // internal class FilterShow400WidthConcreteBeam 
   // {
   //     FilteredElementCollector collector = new FilteredElementCollector(document);
   //     ICollection<Element> stairs = collector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Stairs).ToElements();
   //foreach (Element stair in stairs)
   // {
   //     if (null == stair.GetTypeId())
   //     {
   //         TaskDialog.Show("Revit","No symbol found in stair element: " + stair.Name);
   //     }
   //     else
   //     {
   //         Element elemType = document.GetElement(stair.GetTypeId());
   // string info = "Stair type is: " + elemType.Name;
   // TaskDialog.Show("Revit",info);
   //     }
   // }

   // }

}
