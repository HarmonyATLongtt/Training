using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.VisualStudio.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                //GetAllWalls(doc);
                //InTersectsFilterSample(doc);
                //FindNotSectedWalls(uidoc);
                //MakeLoadFilter(doc);
                //MakeRoomFilter(doc);
                //FindRooms(doc);
                //FindWoodFamilies(doc);
                //FindDoors(doc);
                //var tr = new Transaction(doc, "Delete");
                //if (tr.Start() == TransactionStatus.Started)
                //{
                //    ICollection<ElementId> ids = uidoc.Selection.GetElementIds();
                //    doc.Delete(ids);
                //    tr.Commit();
                //}
                //else
                //{
                //    throw new UserException("Transaction can not be started.");
                //}
                //TaskDialog taskDialog = new TaskDialog("Revit");
                //taskDialog.MainContent = ("Click Yes to return Succeeded. Selected members will be deleted.\n" +
                //                        "Click No to return Failed.  Selected members will not be deleted.\n" +
                //                        "Click Cancel to return Cancelled.  Selected members will not be deleted.");
                //TaskDialogCommonButtons buttons = TaskDialogCommonButtons.Yes |
                //    TaskDialogCommonButtons.No | TaskDialogCommonButtons.Cancel;
                //taskDialog.CommonButtons = buttons;
                //TaskDialogResult taskDialogResult = taskDialog.Show();
                //if (taskDialogResult == TaskDialogResult.Yes)
                //{
                //    return Result.Succeeded;
                //}
                //else if (taskDialogResult == TaskDialogResult.No)
                //{
                //    ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();
                //    foreach (ElementId id in selectedElementIds)
                //    {
                //        elements.Insert(doc.GetElement(id));
                //    }
                //    message = "Failed to delete selection.";
                //    return Autodesk.Revit.UI.Result.Failed;
                //}
                //else
                //{
                //    return Result.Cancelled;
                //}
                //GetElement(doc);
                //GetSelectedElements(uidoc, doc);
                //SetElementProperty(doc, uidoc);

                using (Transaction tx = new Transaction(doc, "create door"))
                {
                    tx.Start();
                    Wall wall = new CreateWall().CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, new XYZ(0, 0, 0), new XYZ(100, 0, 0));
                    CreateDoorInWall(doc, wall);
                    tx.Commit();
                }

                TaskDialog.Show("Ok", "Hello World");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                TaskDialog.Show("lỗi", message);
                return Autodesk.Revit.UI.Result.Failed;
            }
        }

        public ICollection<Element> CreateLogicAndFilter(Autodesk.Revit.DB.Document document)
        {
            // Find all door instances in the project by finding all elements that both belong to the door
            // category and are family instances.
            ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));

            // Create a category filter for Doors
            ElementCategoryFilter doorsCategoryfilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);

            // Create a logic And filter for all Door FamilyInstances
            LogicalAndFilter doorInstancesFilter = new LogicalAndFilter(familyInstanceFilter, doorsCategoryfilter);

            // Apply the filter to the elements in the active document
            FilteredElementCollector collector = new FilteredElementCollector(document);
            IList<Element> doors = collector.WherePasses(doorInstancesFilter).ToElements();

            return doors;
        }

        public static void GetAllWalls(Document document)
        {
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            FilteredElementCollector collector = new FilteredElementCollector(document);
            IList<Element> walls = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();
            String prompt = "The walls in the current document are:\n";
            foreach (Element e in walls)
            {
                prompt += e.Name + "\n";
            }
            TaskDialog.Show("Revit", prompt);
        }

        public void InTersectsFilterSample(Document document)
        {
            Outline myOutLn = new Outline(new XYZ(0, 0, 0), new XYZ(100, 100, 100));
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);
            FilteredElementCollector collector = new FilteredElementCollector(document);
            IList<Element> elements = collector.WherePasses(filter).ToElements();
            BoundingBoxIntersectsFilter invertFilter = new BoundingBoxIntersectsFilter(myOutLn, true);
            collector = new FilteredElementCollector(document);
            IList<Element> notIntersectWalls = collector.OfClass(typeof(Wall)).WherePasses(invertFilter).ToElements();
        }

        public Wall CreateWallUsingCurve2(Document document, Level level, WallType wallType)
        {
            // Build a location line for the wall creation
            XYZ start = new XYZ(0, 0, 0);
            XYZ end = new XYZ(10, 10, 0);
            Line geomLine = Line.CreateBound(start, end);

            // Determine the other parameters
            double height = 15;
            double offset = 3;

            // Create a wall using the location line and wall type
            return Wall.Create(document, geomLine, wallType.Id, level.Id, height, offset, true, true);
        }

        public void FindNotSectedWalls(UIDocument uIDocument)
        {
            ICollection<ElementId> elementIds = uIDocument.Selection.GetElementIds();
            ExclusionFilter filter = new ExclusionFilter(elementIds);
            FilteredElementCollector collector = new FilteredElementCollector(uIDocument.Document);
            IList<Element> walls = collector.WherePasses(filter).OfClass(typeof(Wall)).ToElements();
        }

        public void MakeLoadFilter(Document document)
        {
            ElementClassFilter filter = new ElementClassFilter(typeof(Wall));
            FilteredElementCollector collector = new FilteredElementCollector(document);
            ICollection<Element> allLoads = collector.WherePasses(filter).ToElements();
        }

        public void MakeRoomFilter(Document document)
        {
            RoomFilter filter = new RoomFilter();

            FilteredElementCollector collector = new FilteredElementCollector(document);
            IList<Element> rooms = collector.WherePasses(filter).ToElements();
        }

        public void FindRooms(Document document)
        {
            BuiltInParameter arenaParam = BuiltInParameter.ROOM_AREA;
            ParameterValueProvider pvp = new ParameterValueProvider(new ElementId((int)arenaParam));
            FilterNumericRuleEvaluator fnvr = new FilterNumericGreater();
            double ruleVale = 100.0f;
            FilterRule fRule = new FilterDoubleRule(pvp, fnvr, ruleVale, 1E-6);
            ElementParameterFilter filter = new ElementParameterFilter(fRule);
            FilteredElementCollector collector = new FilteredElementCollector(document);
            IList<Element> rooms = collector.WherePasses(filter).ToElements();

            ElementParameterFilter lessOrEqualFilter = new ElementParameterFilter(fRule, true);
            collector = new FilteredElementCollector(document);
            IList<Element> lessOrEqualFounds = collector.WherePasses(lessOrEqualFilter).ToElements();
        }

        public void FindWoodFamilies(Document document)
        {
            FamilyStructuralMaterialTypeFilter filter = new FamilyStructuralMaterialTypeFilter(StructuralMaterialType.Wood);

            FilteredElementCollector collector = new FilteredElementCollector(document);
            ICollection<Element> woodFamiles = collector.WherePasses(filter).ToElements();

            FamilyStructuralMaterialTypeFilter notWoodFilter =
                    new FamilyStructuralMaterialTypeFilter(StructuralMaterialType.Wood, true);
            collector = new FilteredElementCollector(document);
            ICollection<Element> notWoodFamilies = collector.WherePasses(notWoodFilter).ToElements();
        }

        public void FindDoors(Document document)
        {
            ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));

            ElementCategoryFilter doorsCategoryfilter =
                    new ElementCategoryFilter(BuiltInCategory.OST_Doors);

            LogicalAndFilter doorInstancesFilter = new LogicalAndFilter(familyInstanceFilter,
                    doorsCategoryfilter);

            FilteredElementCollector collector = new FilteredElementCollector(document);
            IList<Element> doors = collector.WherePasses(doorInstancesFilter).ToElements();
        }

        public void GetElement(Document doc)
        {
            ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));

            ElementCategoryFilter beamCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);

            LogicalAndFilter beamIntanceFilter = new LogicalAndFilter(beamCategoryFilter, familyInstanceFilter);

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            collector.WherePasses(beamIntanceFilter);

            ICollection<Element> allBeam = collector.ToElements();

            string promt = "Thông tin của Dầm là: \n";

            foreach (Element beam in allBeam)
            {
                promt += $"Id là:{beam.Id} \n Name là: {beam.Name} \n  {beam.Parameters}";
                // Ép kiểu Element về kiểu FamilyInstance
                //FamilyInstance ft = beam as FamilyInstance;
                //promt += ft.GetType().Name + "\n";
            }
            TaskDialog.Show("Revit", promt);
        }

        public void GetSelectedElements(UIDocument uidoc, Document doc)
        {
            // Lấy danh sách các phần tử được chọn trong tài liệu
            ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();

            // Kiểm tra xem có phần tử nào được chọn không
            if (selectedElementIds.Count > 0)
            {
                foreach (ElementId elementId in selectedElementIds)
                {
                    Element element = doc.GetElement(elementId);
                    if (element != null)
                    {
                        // Lấy thông tin về phần tử
                        string elementName = element.Name;
                        string elementType = element.GetType().ToString();
                        TaskDialog.Show("Selected Element", $"ID: {element.Id} \nName: {elementName}\nType: {elementType} \nParameter: {element.Parameters}");
                    }
                }
            }
            else
            {
                TaskDialog.Show("No Elements Selected", "No elements are currently selected.");
            }
        }

        public void SetElementProperties(Document doc)
        {
            // Lấy một phần tử trong tài liệu (ví dụ: phần tử đầu tiên trong tài liệu)
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            Element element = collector.OfCategory(BuiltInCategory.OST_Walls).FirstElement();

            if (element != null)
            {
                // Thiết lập một thuộc tính cho phần tử (ví dụ: đặt tên cho phần tử)
                using (Transaction transaction = new Transaction(doc))
                {
                    transaction.Start("Set Element Property");

                    // Đặt tên mới cho phần tử
                    element.LookupParameter("URL").Set("Giá trị mới");

                    transaction.Commit();
                }

                TaskDialog.Show("Success", "Element property has been set successfully.");
            }
            else
            {
                TaskDialog.Show("No Elements", "No elements found.");
            }
        }

        public void SetElementProperty(Document doc, UIDocument uidoc)
        {
            // Lấy một phần tử trong tài liệu (ví dụ: phần tử đầu tiên trong tài liệu)
            //FilteredElementCollector collector = new FilteredElementCollector(doc);
            //Element element = collector.OfCategory(BuiltInCategory.OST_Walls).FirstElement();
            // Select element
            ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();
            foreach (ElementId elementId in selectedElementIds)
            {
                Element element = doc.GetElement(elementId);
                if (element != null)
                {
                    using (Transaction transaction = new Transaction(doc))
                    {
                        transaction.Start("Set Element Property");

                        // Gán giá trị cho thuộc tính của phần tử
                        Parameter parameter = element.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                        if (parameter != null && parameter.IsReadOnly == false) // Kiểm tra xem thuộc tính có thể gán giá trị không
                        {
                            var newValue = 100; // Giá trị mới muốn gán cho thuộc tính
                            parameter.Set(newValue);
                            TaskDialog.Show("Success", "Element property has been set successfully.");
                        }
                        else
                        {
                            TaskDialog.Show("Error", "The selected element does not have a settable base offset parameter.");
                        }

                        transaction.Commit();
                    }
                }
                else
                {
                    TaskDialog.Show("No Elements", "No elements found.");
                }
            }
        }

        public void CreateDoorInWall(Document doc, Wall wall)
        {
            Level level = doc.GetElement(wall.LevelId) as Level;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> collection = collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Doors).ToElements();
            IEnumerator<Element> symbolItor = collection.GetEnumerator();
            double x = 0, y = 0, z = 0;
            while (symbolItor.MoveNext())
            {
                FamilySymbol symbol = symbolItor.Current as FamilySymbol;
                XYZ location = new XYZ(x, y, z);
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                }
                FamilyInstance instance = doc.Create.NewFamilyInstance(location, symbol, wall, level, StructuralType.NonStructural);
                x += 10;
                y += 10;
                z += 1.5;
            }
        }
    }
}