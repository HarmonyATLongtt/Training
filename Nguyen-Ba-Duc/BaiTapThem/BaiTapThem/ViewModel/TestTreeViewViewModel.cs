using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaiTapThem.Model;

namespace BaiTapThem.ViewModel
{
    public class TestTreeViewViewModel : BaseViewModel
    {
        public ObservableCollection<TreeNodeModel> RootNodes { get; set; }
        public ObservableCollection<Family> Families { get; set; }

        public TestTreeViewViewModel()
        {
            RootNodes = new ObservableCollection<TreeNodeModel>();

            var root1 = new TreeNodeModel("Root 1");
            root1.Children.Add(new TreeNodeModel("Child 1.1", root1));
            root1.Children.Add(new TreeNodeModel("Child 1.2", root1));

            var root2 = new TreeNodeModel("Root 2");
            var child2 = new TreeNodeModel("Child 2.1", root2);
            child2.Children.Add(new TreeNodeModel("Grandchild 2.1.1", child2));
            child2.Children.Add(new TreeNodeModel("Grandchild 2.1.2", child2));
            root2.Children.Add(child2);

            RootNodes.Add(root1);
            RootNodes.Add(root2);

            List<Family> families = new List<Family>();

            Family family1 = new Family() { Name = "The Doe's" };
            family1.Members.Add(new FamilyMember() { Name = "John Doe", Age = 42 });
            family1.Members.Add(new FamilyMember() { Name = "Jane Doe", Age = 39 });
            family1.Members.Add(new FamilyMember() { Name = "Sammy Doe", Age = 13 });
            families.Add(family1);

            Family family2 = new Family() { Name = "The Moe's" };
            family2.Members.Add(new FamilyMember() { Name = "Mark Moe", Age = 31 });
            family2.Members.Add(new FamilyMember() { Name = "Norma Moe", Age = 28 });
            families.Add(family2);

            Families = new ObservableCollection<Family>(families);
        }
    }
}