using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using FirstCommand.View;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Command1Filter : IExternalCommand
    {
        //private double _height { get; set; }
        //private double _width { get; set; }
        //private double _length { get; set; }

        private const double feetToMinimeter = 304.8;
        private const double epsilon = 0.01;

        private List<ParamData> listParamDatas { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            InputParamView inputParamWindow = new InputParamView(doc);
            if (inputParamWindow.ShowDialog() == true)
            {
                Level selectedLevel = inputParamWindow.Level;
                Material selectedMaterial = inputParamWindow.Material;

                listParamDatas = inputParamWindow.ListParamDatas;

                if (selectedLevel != null && selectedMaterial != null)
                {
                    //_length = TryParseToDouble(inputParamWindow.Length);

                    //_width = TryParseToDouble(inputParamWindow.Width);

                    //_height = TryParseToDouble(inputParamWindow.Height);

                    //if (_length == 0.0 || _width == 0.0 || _height == 0.0)
                    //{
                    //    TaskDialog.Show("Lỗi", "Giá trị nhập không hợp lệ");
                    //    return Result.Failed;
                    //}

                    //Dictionary<string, object> paramDatas = new Dictionary<string, object>
                    //{
                    //    { "Chieu Dai",_length }
                    //    ,{ "Chieu Rong",_width }
                    //    ,{ "Chieu Cao",_height }
                    //    ,{ "Nguyen Lieu", selectedMaterial}
                    //};
                    try
                    {
                        using (Transaction trans = new Transaction(doc, "Filter Intance Family"))
                        {
                            trans.Start();
                            ICollection<ElementId> elementIds = new List<ElementId>();

                            //List<Element> listElements = FilterElements(doc, paramDatas, selectedLevel);
                            List<Element> listElements = FilterElements(doc, listParamDatas, selectedLevel);
                            if (listElements.Count > 0)
                            {
                                foreach (Element element in listElements)
                                {
                                    elementIds.Add(element.Id);
                                }
                                uiDoc.Selection.SetElementIds(elementIds);
                                //uiDoc.ShowElements(elementIds);
                            }

                            trans.Commit();
                        }
                        return Result.Succeeded;
                    }
                    catch (Exception)
                    {
                        return Result.Failed;
                    }
                }
                return Result.Failed;
            }
            return Result.Failed;
        }

        private double TryParseToDouble(string value)
        {
            if (double.TryParse((value), out double doubleValue))
            {
                return doubleValue;
            }
            return 0.0;
        }

        //private List<Element> FilterElements(Document doc, Dictionary<string, object> paramDatas, Level level)
        private List<Element> FilterElements(Document doc, List<ParamData> paramDatas, Level level)
        {
            var filters = CreateFilter(paramDatas, doc);
            var elems = new FilteredElementCollector(doc)
                        .WherePasses(filters)
                        //.Where(p => p.LevelId == level.Id)
                        .ToList();
            if (elems.Count == 0)
            {
                TaskDialog.Show("Thông báo", "Không tìm thấy instance");
                return null;
            }
            return elems;
        }

        //private ElementFilter CreateFilter(Dictionary<string, object> paramDatas, Document doc)
        private ElementFilter CreateFilter(List<ParamData> paramDatas, Document doc)
        {
            List<Element> familyInstances = new FilteredElementCollector(doc)
                                .OfClass(typeof(FamilyInstance))
                                .ToList();
            if (familyInstances.Count == 0)
            {
                TaskDialog.Show("Lỗi", "Không tìm thấy các Family Instance");
                return null;
            }

            Dictionary<Parameter, object> parameters = new Dictionary<Parameter, object>();

            foreach (Element instance in familyInstances)
            {
                foreach (var param in paramDatas)
                {
                    Parameter familyParam = instance.LookupParameter(param.ParaName);
                    if (familyParam == null)
                    {
                        break;
                    }
                    parameters.Add(familyParam, param.ParamValue);
                }
                if (parameters.Count == paramDatas.Count)
                {
                    break;
                }
            }

            //Dictionary<Parameter, object> parameters = new Dictionary<Parameter, object>();

            //foreach (Element instance in familyInstances)
            //{
            //    foreach (var paramDict in paramDatas)
            //    {
            //        Parameter familyParam = instance.LookupParameter(paramDict.Key);
            //        if (familyParam == null)
            //        {
            //            break;
            //        }
            //        parameters.Add(familyParam, paramDict.Value);
            //    }
            //    if (parameters.Count == paramDatas.Count)
            //    {
            //        break;
            //    }
            //}
            var listElementFilters = ListElementFilters(parameters);
            if (listElementFilters.Count != 0)
            {
                LogicalAndFilter combinedFilter = new LogicalAndFilter(listElementFilters);
                return combinedFilter;
            }

            return null;
        }

        private List<ElementFilter> ListElementFilters(Dictionary<Parameter, object> parameters)
        {
            List<ElementFilter> listElementFilters = new List<ElementFilter>();
            foreach (var param in parameters)
            {
                ParameterValueProvider providerParam = new ParameterValueProvider(param.Key.Id);

                switch (param.Key.StorageType)
                {
                    case StorageType.Double:
                        FilterRule ruleDouble = new FilterDoubleRule(providerParam, new FilterNumericEquals(), TryParseToDouble(param.Value.ToString()) / feetToMinimeter, epsilon);
                        listElementFilters.Add(new ElementParameterFilter(ruleDouble));
                        break;

                    case StorageType.ElementId:
                        if (param.Value is Element element)
                        {
                            FilterRule ruleElementId = new FilterElementIdRule(providerParam, new FilterNumericEquals(), element.Id);
                            listElementFilters.Add(new ElementParameterFilter(ruleElementId));
                            break;
                        }
                        break;

                    case StorageType.Integer:
                        //FilterRule ruleInterger = new FilterDoubleRule(providerParam, new FilterNumericEquals(), (double)param.Value, epsilon);
                        FilterRule ruleInterger = new FilterDoubleRule(providerParam, new FilterNumericEquals(), TryParseToDouble(param.Value.ToString()), epsilon);
                        listElementFilters.Add(new ElementParameterFilter(ruleInterger));
                        break;

                    case StorageType.String:

                        FilterRule ruleString = new FilterStringRule(providerParam, new FilterStringEquals(), param.Value.ToString());
                        listElementFilters.Add(new ElementParameterFilter(ruleString));
                        break;
                }
            }

            return listElementFilters;
        }

        //private ElementFilter CreateFilter(Dictionary<string, object> paramDatas, Document doc)
        //{
        //    List<Element> familyInstances = new FilteredElementCollector(doc)
        //                        .OfClass(typeof(FamilyInstance))
        //                        .ToList();
        //    if (familyInstances.Count == 0)
        //    {
        //        TaskDialog.Show("Lỗi", "Không tìm thấy các Family Instance");
        //        return null;
        //    }

        //    foreach (Element instance in familyInstances)
        //    {
        //        Dictionary<Parameter, object> parameters = new Dictionary<Parameter, object>();
        //        foreach (var paramDict in paramDatas)
        //        {
        //            Parameter familyParam = instance.LookupParameter(paramDict.Key);
        //            if (familyParam == null)
        //            {
        //                break;
        //            }
        //            parameters.Add(familyParam, paramDict.Value);
        //        }
        //        if (parameters.Count == paramDatas.Count)
        //        {
        //            List<ElementFilter> listElementFilters = new List<ElementFilter>();
        //            foreach (var param in parameters)
        //            {
        //                ParameterValueProvider providerParam = new ParameterValueProvider(param.Key.Id);
        //                if (param.Key.StorageType == StorageType.Double && param.Value is double)
        //                {
        //                    FilterRule ruleDouble = new FilterDoubleRule(providerParam, new FilterNumericEquals(), (double)param.Value / feetToMinimeter, epsilon);
        //                    ElementParameterFilter paramFilter = new ElementParameterFilter(ruleDouble);
        //                    listElementFilters.Add(paramFilter);
        //                }
        //                else if (param.Key.StorageType == StorageType.ElementId && param.Value is Element element)
        //                {
        //                    FilterRule ruleElementId = new FilterElementIdRule(providerParam, new FilterNumericEquals(), element.Id);
        //                    ElementParameterFilter paramFilter = new ElementParameterFilter(ruleElementId);
        //                    listElementFilters.Add(paramFilter);
        //                }
        //                else if (param.Key.StorageType == StorageType.Integer)
        //                {
        //                    FilterRule ruleInterger = new FilterDoubleRule(providerParam, new FilterNumericEquals(), (double)param.Value, epsilon);
        //                    ElementParameterFilter paramFilter = new ElementParameterFilter(ruleInterger);
        //                    listElementFilters.Add(paramFilter);
        //                }
        //                else if (param.Key.StorageType == StorageType.String && param.Value is String)
        //                {
        //                    FilterRule ruleString = new FilterStringRule(providerParam, new FilterStringEquals(), param.Value.ToString());
        //                    ElementParameterFilter paramFilter = new ElementParameterFilter(ruleString);
        //                    listElementFilters.Add(paramFilter);
        //                }

        //                else
        //                {
        //                    break;
        //                }
        //            }
        //            if (listElementFilters.Count != 0)
        //            {
        //                LogicalAndFilter combinedFilter = new LogicalAndFilter(listElementFilters);
        //                return combinedFilter;
        //            }
        //            return null;
        //        }
        //    }
        //    return null;
        //}

        //private ElementFilter CreateFiler(string name, object val)
        //{
        //    ElementFilter filter;
        //    return filter;
        //}

        //public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        //{
        //    UIDocument uiDoc = commandData.Application.ActiveUIDocument;
        //    Document doc = uiDoc.Document;

        //    InputParamView inputParamWindow = new InputParamView(doc);
        //    if (inputParamWindow.ShowDialog() == true)
        //    {
        //        Level selectedLevel = inputParamWindow.Level;
        //        Material selectedMaterial = inputParamWindow.Material;

        //        if (selectedLevel != null && selectedMaterial != null)
        //        {
        //            if (double.TryParse((inputParamWindow.Length), out double length))
        //            {
        //                _length = length;
        //            }
        //            if (double.TryParse((inputParamWindow.Width), out double width))
        //            {
        //                _width = width;
        //            }
        //            if (double.TryParse((inputParamWindow.Height), out double height))
        //            {
        //                _height = height;
        //            }

        //            if (_length != default && _width != default && _height != default)
        //            {
        //                List<Element> listInstances = new FilteredElementCollector(doc)
        //                        .OfClass(typeof(FamilyInstance))
        //                        .Where(p => p.LevelId == selectedLevel.Id)
        //                        .ToList();

        //                if (listInstances.Count == 0)
        //                {
        //                    TaskDialog.Show("Lỗi", "Không tìm thấy các Family Instance");
        //                    return Result.Failed;
        //                }

        //                List<Element> listIntanceToFilter = new List<Element>();
        //                foreach (Element instance in listInstances)
        //                {
        //                    Parameter lengthParam = instance.LookupParameter("Chieu Dai");
        //                    Parameter widthParam = instance.LookupParameter("Chieu Rong");
        //                    Parameter heightParam = instance.LookupParameter("Chieu Cao");
        //                    Parameter materialParam = instance.LookupParameter("Nguyen Lieu");

        //                    var lengthValue = lengthParam.AsDouble() * 304.8;
        //                    var widthValue = widthParam.AsDouble() * 304.8;
        //                    var heightValue = heightParam.AsDouble() * 304.8;
        //                    ElementId materialId = materialParam.AsElementId();
        //                    //Material material = doc.GetElement(materialId) as Material;
        //                    //string materialName = material.Name;

        //                    if (Math.Abs(lengthValue - _length) < 0.01 && Math.Abs(widthValue - _width) < 0.01
        //                        && Math.Abs(heightValue - _height) < 0.01 && materialId == selectedMaterial.Id)
        //                    {
        //                        listIntanceToFilter.Add(instance);
        //                    }
        //                }

        //                if (listIntanceToFilter.Count == 0)
        //                {
        //                    TaskDialog.Show("Lỗi", "Không tìm thấy các Family Instance cần thiết");
        //                    return Result.Failed;
        //                }

        //                try
        //                {
        //                    using (Transaction trans = new Transaction(doc, "Filter Family"))
        //                    {
        //                        trans.Start();
        //                        ICollection<ElementId> elementIds = new List<ElementId>();
        //                        foreach (Element element in listIntanceToFilter)
        //                        {
        //                            //TaskDialog.Show("Info", element.Name);
        //                            elementIds.Add(element.Id);
        //                        }
        //                        uiDoc.Selection.SetElementIds(elementIds);
        //                        uiDoc.ShowElements(elementIds);
        //                        trans.Commit();
        //                    }
        //                    return Result.Succeeded;
        //                }
        //                catch (Exception ex)
        //                {
        //                    message = ex.Message;
        //                    return Result.Failed;
        //                }
        //            }
        //            return Result.Failed;
        //        }
        //        return Result.Failed;
        //    }
        //    return Result.Failed;
        //}
    }

    public class ParamData
    {
        public string ParaName { get; set; }

        public object ParamValue { get; set; }

        public Type ParamType { get; set; }

        public ParamData(string name, object value)
        {
            ParaName = name;
            ParamValue = value;
        }
    }
}