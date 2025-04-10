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
                                    if (param.Id == cellInfo.paramId)
                                    {
                                        if (!param.IsReadOnly)
                                        {
                                            if (param.StorageType == StorageType.ElementId)
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
                                                                if (element != null && element.Name == cellInfo.value)
                                                                {
                                                                    SetParameterValue(param, elementId);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        BuiltInCategory builtInCategory = ele.Category.BuiltInCategory;

                                                        ElementId elementId = GetElementIdByName(doc, cellInfo.value, builtInCategory);
                                                        if (elementId != ElementId.InvalidElementId)
                                                        {
                                                            SetParameterValue(param, elementId);
                                                        }
                                                        else
                                                        {
                                                            TaskDialog.Show("Lỗi", "Không tồn tại ElementId với tên là:" + cellInfo.value);
                                                        }
                                                    }
                                                }
                                                else if (param.IsShared)
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
                                                                ElementId elementId = GetElementIdByName(doc, cellInfo.value, builtInCategory);
                                                                if (elementId != ElementId.InvalidElementId)
                                                                {
                                                                    SetParameterValue(param, elementId);
                                                                }
                                                                else
                                                                {
                                                                    TaskDialog.Show("Lỗi", "Không tồn tại ElementId với tên là:" + cellInfo.value);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (param.StorageType == StorageType.Integer)
                                            {
                                                int? num = FindMatchingEnumValue(param, cellInfo.value);
                                                if (num != null)
                                                {
                                                    SetParameterValue(param, num);
                                                }
                                                else
                                                {
                                                    TaskDialog.Show("Error", "Do not exist ElementId:" + cellInfo.value);
                                                }
                                            }
                                            else
                                            {
                                                SetParameterValue(param, cellInfo.value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
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
            return null; // không tìm thấy
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
                    //if (result is double doubleValue)
                    //{
                    //    parameter.Set(doubleValue);
                    //}

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
            ICollection<ElementId> elementIds = collector.ToElementIds();

            List<ElementId> paramIds = new List<ElementId>();

            List<ParamOfElement> listElementIdsToChoose = new List<ParamOfElement>();
            for (int col = 0; col < colCount; col++)
            {
                ScheduleField scheduleField = viewSchedule.Definition.GetField(col);
                if (scheduleField.ParameterId != ElementId.InvalidElementId)
                {
                    paramIds.Add(scheduleField.ParameterId);
                }
            }
            List<ParamOfElement> ListParamOfElements = new List<ParamOfElement>();
            foreach (ElementId elementId in elementIds)
            {
                ParamOfElement paramOfElement = new ParamOfElement();
                paramOfElement.ElementId = elementId;
                Element element = doc.GetElement(elementId);
                ElementId typeId = element.GetTypeId();

                Element elementType = doc.GetElement(typeId);
                if (element != null)
                {
                    foreach (Parameter parameter in element.Parameters)
                    {
                        paramOfElement.ListParam.Add(parameter);
                    }
                }
                if (elementType != null)
                {
                    foreach (Parameter parameter in elementType.Parameters)
                    {
                        paramOfElement.ListParam.Add(parameter);
                    }
                }

                ListParamOfElements.Add(paramOfElement);
            }
            foreach (var paramOfElement in ListParamOfElements)
            {
                if (IsParamBelongElement(paramIds, paramOfElement.ListParam))
                {
                    ParamOfElement paramOfEle = new ParamOfElement();
                    paramOfEle.ElementId = paramOfElement.ElementId;
                    paramOfEle.ListParam = FilterList(paramOfElement.ListParam, paramIds);

                    listElementIdsToChoose.Add(paramOfEle);
                }
            }
            List<ScheduleInfo> scheduleInfoList = new List<ScheduleInfo>();
            List<RowInfo> rowInfoList = new List<RowInfo>();
            for (int row = 1; row < rowCount; row++)
            {
                RowInfo rowInfo = new RowInfo();
                rowInfo.Row = row;
                for (int col = 0; col < colCount; col++)
                {
                    string cellValue = viewSchedule.GetCellText(SectionType.Body, row, col);
                    if (cellValue != "")
                    {
                        rowInfo.columnValues.Add(col, cellValue);
                    }
                }
                rowInfoList.Add(rowInfo);
            }
            foreach (var paramOfElement in listElementIdsToChoose)
            {
                List<string> paramValues = new List<string>();
                foreach (Parameter param in paramOfElement.ListParam)
                {
                    if (param.AsValueString() != "" && param.AsValueString() != null)
                    {
                        paramValues.Add(param.AsValueString());
                    }
                }
                foreach (var rowinfo in rowInfoList)
                {
                    if (CompareListWithDictionaryValues(paramValues, rowinfo.columnValues))
                    {
                        ScheduleInfo scheduleInfo = new ScheduleInfo();
                        scheduleInfo.Row = rowinfo.Row;
                        rowinfo.ListParams = paramOfElement.ListParam.ToList();
                        scheduleInfo.ElementId = paramOfElement.ElementId;
                        scheduleInfo.ListParam = paramOfElement.ListParam;
                        scheduleInfoList.Add(scheduleInfo);
                    }
                }
            }
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
                            //cellIsReadOnly.paramId = param.Id;
                            listCellIsReadOnly.Add(cellIsReadOnly);
                        }
                    }
                }
            }
            CellIsReadOnlys = new List<CellIsReadOnly>();
            CellIsReadOnlys = listCellIsReadOnly.ToList();
            RowInfoList = new List<RowInfo>();
            RowInfoList = rowInfoList.ToList();
            return scheduleInfoList;
        }

        private bool CompareListWithDictionaryValues(List<string> list, Dictionary<int, string> dict)
        {
            List<string> dictValueList = new List<string>(dict.Values);
            foreach (string value in list)
            {
                if (!dictValueList.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }

        private HashSet<Parameter> FilterList(HashSet<Parameter> listParams, List<ElementId> paramIds)
        {
            return listParams.Where(p => paramIds.Contains(p.Id))
                            .ToHashSet();
        }

        private bool IsParamBelongElement(List<ElementId> paramIds, HashSet<Parameter> listParams)
        {
            bool result = true;
            List<ElementId> listParamIds = new List<ElementId>();
            foreach (Parameter param in listParams)
            {
                listParamIds.Add(param.Id);
            }
            foreach (ElementId paramId in paramIds)
            {
                if (!listParamIds.Contains(paramId))
                {
                    result = false;
                    break;
                }
            }
            return result;
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

    public class ParamOfElement
    {
        public ElementId ElementId { get; set; }

        public HashSet<Parameter> ListParam { get; set; }
        //public Dictionary<bool, Parameter> paramList { get; set; }

        public ParamOfElement()
        {
            ListParam = new HashSet<Parameter>();
            //paramList = new Dictionary<bool, Parameter>();
        }
    }

    //public class ColumnInputType
    //{
    //    public int Column { get; set; }
    //    public StorageType StorageType { get; set; }
    //    public bool IsSharedParam { get; set; }
    //    public BuiltInCategory BuiltInCategory { get; set; }
    //    public bool isElementType { get; set; }
    //}

    public class CellIsReadOnly
    {
        public string Value { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        //public ElementId paramId { get; set; }
    }

    public class ScheduleInfo
    {
        public int Row { get; set; }
        public ElementId ElementId { get; set; }
        public HashSet<Parameter> ListParam { get; set; }

        public ScheduleInfo()
        {
            ListParam = new HashSet<Parameter>();
        }
    }
}