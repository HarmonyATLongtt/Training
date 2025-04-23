using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.Data;
using System.Text;
using System.IO;
using Autodesk.Revit.DB.Mechanical;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class GetElementAndParamOnSchedule : IExternalCommand
    {
        private List<string> _listValueExclude = new List<string> { "", "<varies>" };

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
                foreach (Element element in schedules)
                {
                    ViewSchedule viewSchedule = element as ViewSchedule;
                    if (viewSchedule == null)
                    {
                        TaskDialog.Show("Error", "This view is not ViewSchedule");
                        return Result.Failed;
                    }

                    List<ScheduleData> listScheduleData = GetElementIdAndRowOnSchdule(doc, viewSchedule);
                    listScheduleData.Sort((a, b) => a.Row.CompareTo(b.Row));

                    List<string> strings = new List<string>();
                    foreach (ScheduleData scheduleData in listScheduleData)
                    {
                        string rowInfo = "Row : " + scheduleData.Row.ToString() + " ,";
                        foreach (ElementId id in scheduleData.ListElementId)
                        {
                            rowInfo += " ElementId :" + id.ToString();
                        }
                        strings.Add(rowInfo);
                    }
                    string str = string.Join(Environment.NewLine, strings);
                    TaskDialog.Show(viewSchedule.Name, str);
                }
                return Result.Succeeded;
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

        private List<ScheduleData> GetElementIdAndRowOnSchdule(Document doc, ViewSchedule viewSchedule)
        {
            TableData tableData = viewSchedule.GetTableData();
            TableSectionData sectionData = tableData.GetSectionData(SectionType.Body);
            int rowCount = sectionData.NumberOfRows;
            int colCount = sectionData.NumberOfColumns;

            FilteredElementCollector collector = new FilteredElementCollector(doc, viewSchedule.Id);
            List<ElementId> elementIds = new List<ElementId>(collector.ToElementIds());

            List<ScheduleData> scheduleInfoList = new List<ScheduleData>();

            Dictionary<int, ElementId> dictParamIdsAndCol = ListParamIdAndCol(colCount, viewSchedule);
            for (int row = 1; row < rowCount; row++)
            {
                Dictionary<ElementId, string> keyValuePairs = new Dictionary<ElementId, string>();
                for (int col = 0; col < colCount; col++)
                {
                    string cellValue = viewSchedule.GetCellText(SectionType.Body, row, col);
                    if (!_listValueExclude.Contains(cellValue) && dictParamIdsAndCol.TryGetValue(col, out ElementId paramId))
                    {
                        keyValuePairs.Add(paramId, cellValue);
                    }
                }
                if (keyValuePairs.Count > 1)
                {
                    HashSet<ElementId> listElementIds = new HashSet<ElementId>(FilterElementsByParameterValue(elementIds, doc, keyValuePairs, viewSchedule));
                    if (listElementIds.Count > 0)
                    {
                        ScheduleData scheduleData = new ScheduleData();
                        scheduleData.ListElementId = listElementIds.ToList();
                        scheduleData.Row = row;
                        elementIds.RemoveAll(x => listElementIds.Contains(x));
                        scheduleInfoList.Add(scheduleData);
                    }
                }
            }

            return scheduleInfoList;
        }

        public List<ElementId> FilterElementsByParameterValue(List<ElementId> elementIds, Document doc, Dictionary<ElementId, string> keyValuePairs, ViewSchedule viewSchedule)
        {
            // Lọc tất cả elements trong model
            //IList<ElementFilter> filters = new List<ElementFilter>();
            //foreach (var keyValuePair in keyValuePairs)
            //{
            //    // viewSchedule là một đối tượng ViewSchedule
            //    var definition = viewSchedule.Definition;

            //    // Lấy danh sách tất cả các ScheduleFilter
            //    IList<ScheduleFilter> scheduleFilters = definition.GetFilters();
            //    foreach (ScheduleFilter f in scheduleFilters)
            //    {
            //        ScheduleField scheduleField = viewSchedule.Definition.GetField(2);
            //        if (f.FieldId == scheduleField.FieldId)
            //        {
            //            var id = f.GetElementIdValue();
            //        }
            //    }

            //    ParameterValueProvider provider = new ParameterValueProvider(keyValuePair.Key);

            //    FilterStringRuleEvaluator evaluator = new FilterStringEquals();
            //    FilterRule rule = new FilterStringRule(provider, evaluator, keyValuePair.Value);
            //    ElementParameterFilter filter = new ElementParameterFilter(rule);
            //    filters.Add(filter);
            //}

            //LogicalAndFilter combinedFilter = new LogicalAndFilter(filters);

            //FilteredElementCollector collector = new FilteredElementCollector(doc, viewSchedule.Id)
            //                                .WhereElementIsNotElementType()
            //                                .WherePasses(combinedFilter);

            //return collector.ToElementIds().ToList();

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

    public class ScheduleData
    {
        public int Row { get; set; }
        public List<ElementId> ListElementId { get; set; }

        public List<Parameter> ParameterList { get; set; }

        public ScheduleData()
        {
            ListElementId = new List<ElementId>();
            ParameterList = new List<Parameter>();
        }
    }
}