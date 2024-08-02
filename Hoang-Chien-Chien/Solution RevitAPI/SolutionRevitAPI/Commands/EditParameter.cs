using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.WPF.Model;
using SolutionRevitAPI.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class EditParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                List<Parameter> lstPara = new List<Parameter>();
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (reference != null)
                {
                    string resultPara = string.Empty;
                    Element element = doc.GetElement(reference);
                    foreach (Parameter para in element.Parameters)
                    {
                        if (para.StorageType == StorageType.Double && para.IsReadOnly == false && !para.CanBeAssociatedWithGlobalParameters())
                        {
                            lstPara.Add(para);
                        }
                    }

                    ObservableCollection<ParameterEle> parameterElements = new ObservableCollection<ParameterEle>();
                    foreach (Parameter item in lstPara)
                    {
                        double value = 0;
                        try
                        {
                            value = UnitUtils.ConvertFromInternalUnits(item.AsDouble(), item.GetUnitTypeId());
                        }
                        catch
                        {
                            value = item.AsDouble();
                        }
                        parameterElements.Add(new ParameterEle() { Name = item.Definition.Name, ValuePara = value });
                    }
                    WPF.Views.EditParameter window = new WPF.Views.EditParameter();
                    EditParemeterVM viewModel = new EditParemeterVM() { ParameterElements = parameterElements };
                    window.DataContext = viewModel;
                    window.ShowDialog();
                    if (viewModel.IsSave)
                    {
                        foreach (ParameterEle item in parameterElements)
                        {
                            var para = lstPara.Find(p => p.Definition.Name == item.Name);

                            //Set value
                            using (Transaction trans = new Transaction(doc, "SetValue"))
                            {
                                trans.Start();
                                para.Set(UnitUtils.ConvertToInternalUnits(item.ValuePara, para.GetUnitTypeId()));
                                trans.Commit();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}