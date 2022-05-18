using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bai_3_test.ViewModel;

namespace Bai_3.Model
{
    public class MainModel 
    {
        public IList<FamilySymbol> FamilyTypeList { get; set; }
        public FamilySymbol SelectedType { get; set; }
        public IList<Level> LevelList { get; set; }
        public Level SelectedLevel { get; set; }       
        public Family _Family { get; set; }
        public MainModel(Document doc)
        {
            _Family = OpenFamily(doc);
            if (_Family == null)
            {
                MessageBox.Show("Chưa chọn family", "Notify", MessageBoxButtons.OK);                  
            }
            else
            {
                FamilyTypeList = GetFamilyType(_Family);
                SelectedType = FamilyTypeList.FirstOrDefault();

                LevelList = GetLevels(doc);
                SelectedLevel = LevelList.FirstOrDefault();
            }              
        }  
        /// <summary>
        /// load family
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Family OpenFamily(Document doc)
        {
            Family family = null;
            using (Transaction tran = new Transaction(doc, "load a file .rfa"))
            {
                tran.Start();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Family File(* .rfa)|*.rfa";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(openFileDialog.FileName))
                    {
                        doc.LoadFamily(openFileDialog.FileName, out family);
                    }
                }
                tran.Commit();
            }
            return family;
        }
        /// <summary>
        /// get the list familysymbols of loaded family
        /// </summary>
        /// <param name="family"></param>
        /// <returns></returns>
        private IList<FamilySymbol> GetFamilyType(Family family)
        {
            ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();
            return familySymbolIds.Select(id => family.Document.GetElement(id))
                                    .Cast<FamilySymbol>()
                                    .ToList();
        }
        /// <summary>
        /// filter and get a list of elements "level" 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private IList<Level> GetLevels(Document doc)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                                                    .OfCategory(BuiltInCategory.OST_Levels)
                                                    .OfClass(typeof(Level))
                                                    .Cast<Level>()
                                                    .ToList();
        }
        /// <summary>
        /// extract a list Line 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private IList<Line> GetIntersectionGrid(Document doc)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                                                     .OfCategory(BuiltInCategory.OST_Grids)
                                                     .OfClass(typeof(Grid))
                                                     .Cast<Grid>()
                                                     .Where(x => x.IsCurved == false)
                                                     .Where(x => !IsBelongToMultisegmentGrid(x))
                                                     .Select(x => x.Curve)
                                                     .Cast<Line>()
                                                     .ToList();
        }
        /// <summary>
        /// get a list of intersection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private IList<XYZ> ListIntersectionGrid(Document doc)
        {
            if (GetIntersectionGrid(doc).Count > 0 && doc != null)
            {
                List<XYZ> IntersectionList = new List<XYZ>();
                foreach (var curve1 in GetIntersectionGrid(doc))
                {
                    foreach (var curve2 in GetIntersectionGrid(doc))
                    {
                        if (curve2 != curve1)
                        {
                            if ((curve2.Intersect(curve1, out IntersectionResultArray resultArray)) != SetComparisonResult.Equal)
                            {
                                if (resultArray != null)
                                {
                                    for (int i = 0; i < resultArray.Size; i++)
                                    {
                                        IntersectionList.Add(resultArray.get_Item(i).XYZPoint);
                                    }
                                }
                            }
                        }
                    }
                }
                return IntersectionList.Distinct(new FilterIntersection()).ToList(); ;
            }
            else
            {
                return new List<XYZ>();
            }
        }
        /// <summary>
        /// create familyinstances of loaded family
        /// set parameter for them
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="doc"></param>
        public void LoadedFamilyInstance(FamilySymbol symbol, Document doc)
        {
            if (ListIntersectionGrid(doc).Count > 0 && doc != null)
            {
                if (LevelList != null)
                {
                    using (Transaction tran = new Transaction(doc, "RevitAPI"))
                    {
                        tran.Start();
                        if (!symbol.IsActive)
                        {
                            symbol.Activate();
                        }
                            foreach (var location in ListIntersectionGrid(doc))
                            {
                                FamilyInstance familyInstance = doc.Create.NewFamilyInstance(location, symbol, SelectedLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                                familyInstance.LookupParameter("Base Offset").Set(UnitUtils.ConvertToInternalUnits(4000, UnitTypeId.Millimeters));

                                familyInstance.LookupParameter("Top Level").Set(SelectedLevel.Id); ; ;
                            }
                        tran.Commit();
                    }
                }
                else
                {
                    MessageBox.Show("level null");
                }
            }   
        }        
        /// <summary>
        /// Filter multi segment grid
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private bool IsBelongToMultisegmentGrid(Grid grid)
        {
            if (grid != null)
            {
                ElementId multiSegmentId = MultiSegmentGrid.GetMultiSegementGridId(grid);
                return multiSegmentId != null
                        && multiSegmentId != ElementId.InvalidElementId;
            }
            return false;
        }
    }
}

