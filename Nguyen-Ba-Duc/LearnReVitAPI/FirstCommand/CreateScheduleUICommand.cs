using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using Autodesk.Revit.UI;
using FirstCommand.View;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.ExtensibleStorage;
using FirstCommand.ViewModel;
using System.Windows.Controls;
using System.Data;
using System.Dynamic;
using static Autodesk.Revit.DB.SpecTypeId;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateScheduleUICommand : IExternalCommand
    {
        private double FeetToMm = 304.8;

        private List<string> _listValueExclude = new List<string> { "", "<varies>" };
        public List<CellIsReadOnly> CellIsReadOnlys { get; set; }
        public List<RowInfo> RowInfoList { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Application app = uiapp.Application;

            // Lấy tất cả các ViewSchedule trong project
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ViewSchedule));
            IList<Element> schedules = collector.ToElements();

            if (schedules.Count > 0)
            {
                ViewSchedule viewSchedule = schedules.FirstOrDefault() as ViewSchedule;
                if (viewSchedule == null)
                {
                    TaskDialog.Show("Error", "This view is not ViewSchedule");
                    return Result.Failed;
                }
                List<ScheduleInfo> scheduleInfos = GetElementIdAndRowOnSchdule(doc, viewSchedule);

                ScheduleView scheduleView = new ScheduleView(doc, viewSchedule, CellIsReadOnlys, RowInfoList);

                ScheduleViewModel viewModel = scheduleView.DataContext as ScheduleViewModel;

                if (scheduleView.ShowDialog() == true)
                {
                    List<CellInfo> cellInfos = viewModel.CellInfos;
                    UpdateScheduleData(doc, cellInfos, scheduleInfos, app);
                    return Result.Succeeded;
                }
                return Result.Failed;
            }
            return Result.Failed;
        }

        //Hàm thực hiện transaction
        public void RunTransaction(Document doc, string transactionName, Action<Transaction> action)
        {
            using (Transaction trans = new Transaction(doc, transactionName))
            {
                trans.Start();

                FailureHandlingOptions options = trans.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new WarningSuppressor());
                trans.SetFailureHandlingOptions(options);

                action(trans); // Thực thi hành động trong Transaction

                trans.Commit();
            }
        }

        private void UpdateScheduleData(Document doc, List<CellInfo> cellInfos, List<ScheduleInfo> scheduleInfos, Application app)
        {
            RunTransaction(doc, "Create Schedule", (Transaction t) =>
            {
                foreach (CellInfo cellInfo in cellInfos)
                {
                    if (cellInfo.isChanged)
                    {
                        foreach (ScheduleInfo scheduleInfo in scheduleInfos)
                        {
                            if (scheduleInfo.Row == cellInfo.rowCellSchedule)
                            {
                                foreach (Parameter param in scheduleInfo.ListParam)
                                {
                                    if (param.Id == cellInfo.paramId && !param.IsReadOnly)
                                    {
                                        if (param.StorageType == StorageType.ElementId)
                                        {
                                            SetParamForElementIdType(doc, param, cellInfo.value, app);
                                        }
                                        else if (param.StorageType == StorageType.Integer)
                                        {
                                            SetParamForIntegerType(param, cellInfo.value);
                                        }
                                        else
                                        {
                                            SetParameterValue(param, cellInfo.value);
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            });
        }

        private void SetParamForElementIdType(Document doc, Parameter param, string value, Application app)
        {
            if (!param.IsShared)
            {
                Element ele = doc.GetElement(param.AsElementId());
                if (ele != null)
                {
                    if (ele is ElementType elementType)
                    {
                        ICollection<ElementId> listElementIdtype = elementType.GetSimilarTypes();
                        foreach (ElementId elementId in listElementIdtype)
                        {
                            if (elementId != ElementId.InvalidElementId)
                            {
                                Element element = doc.GetElement(elementId);
                                if (element != null && element.Name == value)
                                {
                                    SetParameterValue(param, elementId);
                                }
                            }
                        }
                    }
                    else
                    {
                        BuiltInCategory builtInCategory = ele.Category.BuiltInCategory;

                        ElementId elementId = GetElementIdByName(doc, value, builtInCategory);
                        if (elementId != ElementId.InvalidElementId)
                        {
                            SetParameterValue(param, elementId);
                        }
                        else
                        {
                            TaskDialog.Show("Error", "Do not exist ElementId:" + value);
                        }
                    }
                }
            }
            else
            {
                Definition def = param.Definition;
                ForgeTypeId typeId = def.GetDataType();
                Dictionary<ForgeTypeId, BuiltInCategory> keyValuePairs = GetBuiltInCategoryFromSharedParam(app);
                foreach (var item in keyValuePairs)
                {
                    if (typeId == item.Key)
                    {
                        BuiltInCategory builtInCategory = item.Value;
                        if (builtInCategory != BuiltInCategory.INVALID)
                        {
                            ElementId elementId = GetElementIdByName(doc, value, builtInCategory);
                            if (elementId != ElementId.InvalidElementId)
                            {
                                SetParameterValue(param, elementId);
                            }
                            else
                            {
                                TaskDialog.Show("Error", "Do not exist ElementId:" + value);
                            }
                        }
                    }
                }
            }
        }

        private void SetParamForIntegerType(Parameter param, string value)
        {
            int? num = FindMatchingEnumValue(param, value);
            if (num != null)
            {
                SetParameterValue(param, num);
            }
            else
            {
                TaskDialog.Show("Error", "Do not exist ElementId:" + value);
            }
        }

        private int? FindMatchingEnumValue(Parameter param, string userInput)
        {
            for (int i = 0; i < 100; i++)
            {
                if (param.Set(i))
                {
                    if (param.AsValueString()?.Trim() == userInput.Trim())
                    {
                        return i;
                    }
                }
            }
            return null;
        }

        private Dictionary<ForgeTypeId, BuiltInCategory> GetBuiltInCategoryFromSharedParam(Application app)
        {
            DefinitionFile sharedParamFile = app.OpenSharedParameterFile();
            DefinitionGroups groups = sharedParamFile.Groups;
            Dictionary<ForgeTypeId, BuiltInCategory> keyValuePairs = new Dictionary<ForgeTypeId, BuiltInCategory>();
            HashSet<ForgeTypeId> keySet = new HashSet<ForgeTypeId>();
            foreach (DefinitionGroup group in groups)
            {
                foreach (Definition definition in group.Definitions)
                {
                    ForgeTypeId typeId = definition.GetDataType();
                    keySet.Add(typeId);
                }
            }
            foreach (ForgeTypeId typeId in keySet)
            {
                if (typeId.Equals(SpecTypeId.Reference.Material))
                {
                    keyValuePairs.Add(typeId, BuiltInCategory.OST_Materials);
                }
                else if (typeId.Equals(SpecTypeId.Length))
                {
                    keyValuePairs.Add(typeId, BuiltInCategory.INVALID);
                }
                else if (typeId.Equals(SpecTypeId.Reference.Image))
                {
                    keyValuePairs.Add(typeId, BuiltInCategory.INVALID);
                }
                else if (typeId.Equals(SpecTypeId.String.Text))
                {
                    keyValuePairs.Add(typeId, BuiltInCategory.INVALID);
                }
                else if (typeId.Equals(SpecTypeId.String.Url))
                {
                    keyValuePairs.Add(typeId, BuiltInCategory.INVALID);
                }
            }
            return keyValuePairs;
        }

        private double IsDouble(string input)
        {
            double result;
            if (double.TryParse(input, out result))
            {
                return result;
            }
            return 0.0;
        }

        private ElementId GetElementIdByName(Document doc, string name, BuiltInCategory builtInCategory)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .OfCategory(builtInCategory)
                .WhereElementIsNotElementType();
            Element element = collector.FirstOrDefault(e => e.Name == name);
            if (element != null)
            {
                return element.Id;
            }
            return ElementId.InvalidElementId;
        }

        private void SetParameterValue(Parameter parameter, object value)
        {
            if (parameter != null)
            {
                switch (parameter.StorageType)
                {
                    case StorageType.Integer:
                        if (value is int intValue)
                        {
                            parameter.Set(intValue);
                        }
                        break;

                    case StorageType.Double:

                        double result = IsDouble(value.ToString());
                        if (result != 0.0)
                        {
                            parameter.Set(result / FeetToMm);
                        }
                        break;

                    case StorageType.String:
                        if (value is string stringValue)
                        {
                            parameter.Set(stringValue);
                        }
                        break;

                    case StorageType.ElementId:
                        if (value is ElementId elementIdValue)
                        {
                            parameter.Set(elementIdValue);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private List<ScheduleInfo> GetElementIdAndRowOnSchdule(Document doc, ViewSchedule viewSchedule)
        {
            TableData tableData = viewSchedule.GetTableData();
            TableSectionData sectionData = tableData.GetSectionData(SectionType.Body);
            int rowCount = sectionData.NumberOfRows;
            int colCount = sectionData.NumberOfColumns;

            FilteredElementCollector collector = new FilteredElementCollector(doc, viewSchedule.Id);
            List<ElementId> elementIds = new List<ElementId>(collector.ToElementIds());

            List<ScheduleInfo> scheduleInfoList = new List<ScheduleInfo>();

            Dictionary<int, ElementId> dictParamIdsAndCol = ListParamIdAndCol(colCount, viewSchedule);

            List<RowInfo> rowInfoList = new List<RowInfo>();

            for (int row = 1; row < rowCount; row++)
            {
                Dictionary<ElementId, string> keyValuePairs = new Dictionary<ElementId, string>();

                RowInfo rowInfo = new RowInfo();
                rowInfo.Row = row;
                for (int col = 0; col < colCount; col++)
                {
                    string cellValue = viewSchedule.GetCellText(SectionType.Body, row, col);
                    if (!_listValueExclude.Contains(cellValue) && dictParamIdsAndCol.TryGetValue(col, out ElementId paramId))
                    {
                        rowInfo.columnValues.Add(col, cellValue);
                        keyValuePairs.Add(paramId, cellValue);
                    }
                }
                if (keyValuePairs.Count > 1)
                {
                    HashSet<ElementId> listElementIds = new HashSet<ElementId>(FilterElementsByParameterValue(elementIds, doc, keyValuePairs, viewSchedule));
                    if (listElementIds.Count > 0)
                    {
                        ScheduleInfo scheduleData = new ScheduleInfo();
                        scheduleData.ListElementId = listElementIds.ToList();
                        scheduleData.ListParam = GetListParam(doc, scheduleData.ListElementId, dictParamIdsAndCol);
                        scheduleData.Row = row;
                        rowInfo.ListParams = scheduleData.ListParam.ToList();
                        elementIds.RemoveAll(x => listElementIds.Contains(x));
                        scheduleInfoList.Add(scheduleData);
                    }
                }
                rowInfoList.Add(rowInfo);
            }

            AddListCellReadOnly(rowInfoList);

            RowInfoList = rowInfoList.ToList();

            return scheduleInfoList;
        }

        private HashSet<Parameter> GetListParam(Document doc, List<ElementId> elementIds, Dictionary<int, ElementId> dictParamIdsAndCol)
        {
            HashSet<Parameter> listParams = new HashSet<Parameter>();
            foreach (ElementId id in elementIds)
            {
                Element ele = doc.GetElement(id);
                ElementId typeId = ele.GetTypeId();
                Element elementType = doc.GetElement(typeId);

                foreach (Parameter p in ele.Parameters)
                {
                    if (dictParamIdsAndCol.ContainsValue(p.Id))
                    {
                        listParams.Add(p);
                    }
                }
                foreach (Parameter p in elementType.Parameters)
                {
                    if (dictParamIdsAndCol.ContainsValue(p.Id))
                    {
                        listParams.Add(p);
                    }
                }
            }
            return listParams;
        }

        private void AddListCellReadOnly(List<RowInfo> rowInfoList)
        {
            List<CellIsReadOnly> listCellIsReadOnly = new List<CellIsReadOnly>();
            foreach (var rowinfo in rowInfoList)
            {
                foreach (Parameter param in rowinfo.ListParams.Where(p => p.IsReadOnly == true))
                {
                    foreach (var keyValuePair in rowinfo.columnValues)
                    {
                        if (param.AsValueString() == keyValuePair.Value.ToString())
                        {
                            CellIsReadOnly cellIsReadOnly = new CellIsReadOnly();
                            cellIsReadOnly.Row = rowinfo.Row;
                            cellIsReadOnly.Column = keyValuePair.Key;
                            cellIsReadOnly.Value = keyValuePair.Value;
                            listCellIsReadOnly.Add(cellIsReadOnly);
                        }
                    }
                }
            }
            CellIsReadOnlys = listCellIsReadOnly.ToList();
        }

        public List<ElementId> FilterElementsByParameterValue(List<ElementId> elementIds, Document doc, Dictionary<ElementId, string> keyValuePairs, ViewSchedule viewSchedule)
        {
            List<ElementId> listElementIds = new List<ElementId>();
            foreach (ElementId id in elementIds)
            {
                int num = keyValuePairs.Count;
                int count = 0;
                Element element = doc.GetElement(id);
                if (element != null)
                {
                    count += GetCount(element, keyValuePairs);
                    ElementId typeId = element.GetTypeId();
                    Element elementType = doc.GetElement(typeId);

                    if (elementType != null)
                    {
                        count += GetCount(elementType, keyValuePairs);
                    }
                }

                if (count == num)
                {
                    listElementIds.Add(id);
                }
            }

            return listElementIds;
        }

        private int GetCount(Element element, Dictionary<ElementId, string> keyValuePairs)
        {
            int count = 0;
            foreach (Parameter parameter in element.Parameters)
            {
                if (keyValuePairs.ContainsKey(parameter.Id) && keyValuePairs.ContainsValue(parameter.AsValueString()))
                {
                    count++;
                }
            }
            return count;
        }

        private Dictionary<int, ElementId> ListParamIdAndCol(int colCount, ViewSchedule viewSchedule)
        {
            Dictionary<int, ElementId> paramIds = new Dictionary<int, ElementId>();
            for (int col = 0; col < colCount; col++)
            {
                ScheduleField scheduleField = viewSchedule.Definition.GetField(col);
                if (scheduleField.ParameterId != ElementId.InvalidElementId)
                {
                    paramIds.Add(col, scheduleField.ParameterId);
                }
            }
            return paramIds;
        }
    }

    public class RowInfo
    {
        public int Row { get; set; }
        public Dictionary<int, string> columnValues { get; set; }
        public List<Parameter> ListParams { get; set; }

        public RowInfo()
        {
            columnValues = new Dictionary<int, string>();
            ListParams = new List<Parameter>();
        }
    }

    public class CellIsReadOnly
    {
        public string Value { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public class ScheduleInfo
    {
        public int Row { get; set; }
        public List<ElementId> ListElementId { get; set; }
        public HashSet<Parameter> ListParam { get; set; }

        public ScheduleInfo()
        {
            ListParam = new HashSet<Parameter>();
            ListElementId = new List<ElementId>();
        }
    }
}