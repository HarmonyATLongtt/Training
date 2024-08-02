using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.WPF.Model;
using SolutionRevitAPI.WPF.ViewModels;
using SolutionRevitAPI.WPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CreatTag : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ViewType viewType = doc.ActiveView.ViewType;
            if (viewType != ViewType.ThreeD)
            {
                try
                {
                    TagModeV window = new TagModeV();
                    TagModeVM viewModel = new TagModeVM();
                    window.DataContext = viewModel;
                    window.ShowDialog();
                    if (viewModel.TagAllMode)
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
                        // Lọc để lấy các FamilySymbol thuộc loại tag
                        List<FamilySymbol> tagSymbols = (from FamilySymbol symbol in collector
                                                         let category = symbol.Category
                                                         where category != null && category.Name.Contains("Tag") && category.IsTagCategory
                                                         select symbol).ToList();
                        var a = tagSymbols;
                        var groupedTagSymbols = tagSymbols.GroupBy(ts => ts.Category.Name)
                                                .ToDictionary(g => g.Key, g => g.ToList());
                        ObservableCollection<CategoryTag> tagGroups = new ObservableCollection<CategoryTag>();
                        foreach (var group in groupedTagSymbols)
                        {
                            var tagGroup = new CategoryTag
                            {
                                IsSelected = false,
                                Cat = doc.Settings.Categories.Cast<Category>().FirstOrDefault(c => c.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase)),
                                LoadedTags = group.Value,
                                SelectedMember = group.Value.Count() > 0 ? group.Value[0] : null,
                            };
                            tagGroups.Add(tagGroup);
                        }
                        TagAll TagAllWindow = new TagAll();
                        TagAllVM TagAllVM = new TagAllVM() { LstCategoryTag = tagGroups };
                        TagAllWindow.DataContext = TagAllVM;
                        TagAllWindow.ShowDialog();

                        tagGroups = TagAllVM.LstCategoryTag;
                        foreach (var tagGroup in tagGroups)
                        {
                            if (!tagGroup.IsSelected) continue;
                            var ac = tagGroup.Cat.BuiltInCategory.ToString();
                            string categoryString = ac.Substring(0, ac.Length - 4); // Loại bỏ Đuôi Tags trong BuiltInCategory của Tag
                            string categoryString2 = categoryString + "s"; //Trường hợp BuiltInCategory có dạng OST_XXXXs
                            // Chỉ định loại enum của BuiltInCategory
                            if (Enum.TryParse<BuiltInCategory>(categoryString, out BuiltInCategory builtInCat) || Enum.TryParse<BuiltInCategory>(categoryString2, out builtInCat))
                            {
                                List<Element> lstEle = new FilteredElementCollector(doc).OfCategory(builtInCat).WhereElementIsNotElementType().ToElements().ToList();
                                if (lstEle.Count() <= 0) continue;
                                using (Transaction trans = new Transaction(doc, "Tag Element by Category"))
                                {
                                    trans.Start();
                                    TagOrientation tagOrientation = TagOrientation.Horizontal;
                                    foreach (var ele in lstEle)
                                    {
                                        try
                                        {
                                            XYZ point;
                                            Reference reference = new Reference(ele);
                                            if (ele.Location is LocationPoint)
                                            {
                                                point = (ele.Location as LocationPoint).Point;
                                            }
                                            else
                                            {
                                                XYZ startPonit = (ele.Location as LocationCurve).Curve.GetEndPoint(0);
                                                XYZ endPoint = (ele.Location as LocationCurve).Curve.GetEndPoint(1);
                                                point = new XYZ((startPonit.X + endPoint.X) / 2, (startPonit.Y + endPoint.Y) / 2, (startPonit.Z + endPoint.Z) / 2);
                                            }
                                            IndependentTag tag = IndependentTag.Create(doc, tagGroup.SelectedMember.Id, doc.ActiveView.Id, reference, true, tagOrientation, point);
                                        }
                                        catch (Exception ex)
                                        {
                                            TaskDialog.Show("Error", ex.ToString());
                                            continue;
                                        }
                                    }
                                    trans.Commit();
                                }
                            }
                        }
                    }
                    else
                    {
                        TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
                        TagOrientation tagOrientation = TagOrientation.Horizontal;
                        try
                        {
                            Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                            if (reference != null)
                            {
                                using (Transaction trans = new Transaction(doc, "Tag an Element"))
                                {
                                    Element element = doc.GetElement(reference);
                                    trans.Start();
                                    XYZ point;
                                    if (element.Location is LocationPoint)
                                    {
                                        point = (element.Location as LocationPoint).Point;
                                    }
                                    else
                                    {
                                        XYZ startPonit = (element.Location as LocationCurve).Curve.GetEndPoint(0);
                                        XYZ endPoint = (element.Location as LocationCurve).Curve.GetEndPoint(1);
                                        point = new XYZ((startPonit.X + endPoint.X) / 2, (startPonit.Y + endPoint.Y) / 2, (startPonit.Z + endPoint.Z) / 2);
                                    }
                                    IndependentTag tag = IndependentTag.Create(doc, doc.ActiveView.Id, reference, true, tagMode, tagOrientation, point);
                                    trans.Commit();
                                }
                            }
                        }
                        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                        {
                            return Result.Cancelled;
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    return Result.Cancelled;
                }
            }
            else
            {
                TaskDialog.Show("Warning", "The orientation of the 3D view must be locked before you can add tags or keynotes");
            }
            return Result.Succeeded;
        }
    }
}