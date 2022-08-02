using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bai_2.View;
using Bai_2.Model;

namespace Bai_2.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public Element element { get; set; }

        private List<InstanceParameterViewModel> listPara;
        public List<InstanceParameterViewModel> ListPara 
        {
            get
            {
                return listPara;
            }
            set 
            { 
                listPara = value;
                OnPropertyChanged();
            }
        }
        private InstanceInforView instanceInfor;
        public InstanceInforView InstanceInfor 
        {
            get 
            {
                if(instanceInfor == null)
                {
                    instanceInfor = new InstanceInforView() { DataContext = this };
                }
                return instanceInfor;
            }
            set 
            { 
                instanceInfor = value;
                OnPropertyChanged();
            }
        }
        public MainViewModel(Element ele, Document doc)
        {
            element = ele;
            GetInfor(ele, doc);
        }
        private void GetInfor(Element ele, Document document)
        {
            ListPara = new List<InstanceParameterViewModel>();
            foreach (Parameter parameter in ele.Parameters)
            {
                InstanceParameterViewModel instance = new InstanceParameterViewModel(new InstanceModel());
                instance.ParameterName = parameter.Definition.Name;
                instance.DataType = parameter.StorageType.ToString();
                if (parameter.HasValue)
                {
                    instance.Value = GetParameterValue(parameter, document);    
                }
                else
                {
                    instance.Value = "not assigned"; 
                }
                ListPara.Add(instance);
            }
        }
        private String GetParameterValue(Parameter para, Document doc)
        {
            string ParaValue = string.Empty;
            switch (para.StorageType)
            {
                case StorageType.Double:
                    ParaValue = para.AsValueString();
                    break;
                case StorageType.ElementId:
                    ElementId Id = para.AsElementId();
                    if(Id.IntegerValue >= 0)
                    {
                        ParaValue = doc.GetElement(Id).Name;

                    }
                    else
                    {
                        ParaValue = Id.IntegerValue.ToString();
                    }
                    break;
                case StorageType.Integer:
                    if(ParameterType.YesNo == para.Definition.ParameterType)
                    {
                        if (para.AsInteger() == 0)
                        {
                            ParaValue = "false";
                        }
                        else
                        {
                            ParaValue = "True";
                        }
                    }
                    else
                    {
                        ParaValue = para.AsInteger().ToString();
                    }
                    break ;
                case StorageType.String:
                    ParaValue = para.AsString();
                    break ;
                    default:
                    ParaValue = "....";
                    break ;
            }
            return ParaValue;
        }
        
    }
}
