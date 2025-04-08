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

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

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

                ScheduleView scheduleView = new ScheduleView(doc, viewSchedule);
                ScheduleViewModel viewModel = scheduleView.DataContext as ScheduleViewModel;
                if (scheduleView.ShowDialog() == true)
                {
                    List<CellInfo> cellInfos = viewModel.CellInfos;
                    UpdateScheduleData(doc, cellInfos, scheduleInfos);
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

        private void UpdateScheduleData(Document doc, List<CellInfo> cellInfos, List<ScheduleInfo> scheduleInfos)
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
                                        Element ele = doc.GetElement(param.AsElementId());
                                        if (ele != null)
                                        {
                                            ElementId elementId = GetElementIdByName(doc, cellInfo.value);
                                            if (elementId != ElementId.InvalidElementId)
                                            {
                                                SetParameterValue(param, elementId);
                                            }
                                            else
                                            {
                                                TaskDialog.Show("Lỗi", "Không tồn tại ElementId với tên là:" + cellInfo.value);
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
            });
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

        private ElementId GetElementIdByName(Document doc, string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.WhereElementIsNotElementType();
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
                    //if (value is int intValue)
                    //{
                    //    parameter.Set(intValue);
                    //}
                    //break;
                    case StorageType.Integer:
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
                        scheduleInfo.ElementId = paramOfElement.ElementId;
                        scheduleInfo.ListParam = paramOfElement.ListParam;
                        scheduleInfoList.Add(scheduleInfo);
                    }
                }
            }
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

        public RowInfo()
        {
            columnValues = new Dictionary<int, string>();
        }
    }

    public class ParamOfElement
    {
        public ElementId ElementId { get; set; }

        public HashSet<Parameter> ListParam { get; set; }

        public ParamOfElement()
        {
            ListParam = new HashSet<Parameter>();
        }
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