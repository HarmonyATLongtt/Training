using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Ex_RevitAPI.Bai_1.View;
using Bai_1.Bai_1.Model;

namespace Ex_RevitAPI.Bai_1.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private MainModel Model { get; set; }

        private InstanceFilterView filterView;
        public InstanceFilterView FilterView 
        { 
            get 
            {
                if (filterView == null)
                {
                    filterView = new InstanceFilterView() { DataContext = this};
                }
                return filterView;
            }
            set
            {
                filterView = value;
                OnPropertyChanged();
            }
        }
        public int CategoryNumber
        {
            get { return Model.CategoryNumber; }
            set { Model.CategoryNumber = value; OnPropertyChanged(nameof(CategoryNumber)); }
        }
        public int FamilyNumber
        {
            get { return Model.FamilyNumber; }
            set { Model.FamilyNumber = value; OnPropertyChanged(nameof(FamilyNumber)); }
        }
        public int TypeNumber
        {
            get { return Model.TypeNumber; }
            set { Model.TypeNumber = value; OnPropertyChanged(nameof(TypeNumber)); }
        }
        private int instanceNumber;
        public int InstanceNumber
        {
            get { return instanceNumber; }
            set { instanceNumber = value; OnPropertyChanged(); }
        }
        public MainViewModel(Document document)
        {
            InstanceNumber = GetInstance(document).Count();
            TypeNumber = GetTypeNumber(document);
            CategoryNumber = GetCategoryNumber(document);
            FamilyNumber = GetFamilyNumber(document);
        }

        private IList<Element> GetInstance(Document doc)
        { 
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var instancenumber = collector.WhereElementIsElementType().OfCategory(BuiltInCategory.INVALID)
                                          .OfClass(typeof(FamilyInstance)).ToElements();

            return instancenumber;
        }

        private int GetTypeNumber(Document doc)
        {
            IList<ElementType> ListEle = new List<ElementType>();

            foreach (var member in GetInstance(doc))
            {
                if (member != null)
                {
                    ListEle.Add(doc.GetElement(member.GetTypeId()) as ElementType);
                }
            }
            List<string> list = ListEle.Select(x => x.Name).Distinct().ToList();
            return list.Count();
        }

        private int GetFamilyNumber(Document doc)
        {
            IList<ElementType> ListEle = new List<ElementType>();

            foreach (var member in GetInstance(doc))
            {
                if (member != null)
                {
                    ListEle.Add(doc.GetElement(member.GetTypeId()) as ElementType);
                }
            }
            List<string> list = ListEle.Select(x => x.FamilyName).Distinct().ToList();
            return list.Count();


        }
        private int GetCategoryNumber(Document doc)
        {
            IList<Category> ListEle = new List<Category>();
            foreach (var member in GetInstance(doc))
            {
                if (member != null)
                {
                    ListEle.Add(member.Category);
                }
            }
            List<string> list = ListEle.Select(x => x.Name).Distinct().ToList();
            return list.Count();
        }
    }
}