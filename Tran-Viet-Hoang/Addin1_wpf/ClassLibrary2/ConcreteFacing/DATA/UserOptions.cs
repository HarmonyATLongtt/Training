﻿using Autodesk.Revit.DB;
using ConcreteFacing.UI.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace ConcreteFacing.DATA
{
    public class UserOptions
    {
        public double Thickness { get; set; }
        public List<Face> FaceIsCheck { get; set; }

        public UserOptions Check(List<Face> faces, MainViewModel vm, Element elem)
        {
            UserOptions usersetting = new UserOptions();

            List<Face> result = new List<Face>();
            FamilyInstance ins = elem as FamilyInstance;
            XYZ topnorm = ins.HandOrientation.CrossProduct(ins.FacingOrientation);
            XYZ botnorm = topnorm.Negate();

            var catedata = vm.SourceCatelv.ToList();

            foreach (var cate in catedata)
            {
                if (cate.CateIsChecked && cate.CateName == "Structural Framing" && elem.Category.Name == "Structural Framing")
                {
                    usersetting.Thickness = cate.Thickness;
                    var allface = cate.TemplateCoverFaceViewModels.ToList();
                    if (allface.Where(i => i.CoverFaceContent == "Top").First().CoverFaceIsCheck)
                    {
                        var plannarFace = faces.OfType<PlanarFace>().Where(m => m.FaceNormal.IsAlmostEqualTo(topnorm, 10E-5)).Cast<Face>().First();
                        result.Add(plannarFace);
                    }
                    if (allface.Where(i => i.CoverFaceContent == "Bottom").First().CoverFaceIsCheck)
                    {
                        var plannarFace = faces.OfType<PlanarFace>().Where(m => m.FaceNormal.IsAlmostEqualTo(botnorm, 10E-5)).Cast<Face>().First();
                        result.Add(plannarFace);
                    }
                    if (allface.Where(i => i.CoverFaceContent == "Front").First().CoverFaceIsCheck)
                    {
                        var plannarFace = faces.OfType<PlanarFace>().Where(m => m.FaceNormal.IsAlmostEqualTo(ins.FacingOrientation.Negate(), 10E-5)).Cast<Face>().First();
                        result.Add(plannarFace);
                    }
                    if (allface.Where(i => i.CoverFaceContent == "Back").First().CoverFaceIsCheck)
                    {
                        var plannarFace = faces.OfType<PlanarFace>().Where(m => m.FaceNormal.IsAlmostEqualTo(ins.FacingOrientation, 10E-5)).Cast<Face>().First();
                        result.Add(plannarFace);
                    }
                }
                if (cate.CateIsChecked && cate.CateName == "Structural Columns" && elem.Category.Name == "Structural Columns")
                {
                    usersetting.Thickness = cate.Thickness;
                    var allface = cate.TemplateCoverFaceViewModels.ToList();

                    if (allface.Where(i => i.CoverFaceContent == "Right").First().CoverFaceIsCheck)
                    {
                        var plannarFace = faces.OfType<PlanarFace>().Where(m => m.FaceNormal.IsAlmostEqualTo(ins.HandOrientation, 10E-5)).Cast<Face>().First();
                        result.Add(plannarFace);
                    }
                    if (allface.Where(i => i.CoverFaceContent == "Left").First().CoverFaceIsCheck)
                    {
                        var plannarFace = faces.OfType<PlanarFace>().Where(m => m.FaceNormal.IsAlmostEqualTo(ins.HandOrientation.Negate(), 10E-5)).Cast<Face>().First();
                        result.Add(plannarFace);
                    }
                    if (allface.Where(i => i.CoverFaceContent == "Front").First().CoverFaceIsCheck)
                    {
                        var plannarFace = faces.OfType<PlanarFace>().Where(m => m.FaceNormal.IsAlmostEqualTo(ins.FacingOrientation.Negate(), 10E-5)).Cast<Face>().First();
                        result.Add(plannarFace);
                    }
                    if (allface.Where(i => i.CoverFaceContent == "Back").First().CoverFaceIsCheck)
                    {
                        var plannarFace = faces.OfType<PlanarFace>().Where(m => m.FaceNormal.IsAlmostEqualTo(ins.FacingOrientation, 10E-5)).Cast<Face>().First();
                        result.Add(plannarFace);
                    }
                }
            }
            usersetting.FaceIsCheck = result;
            return usersetting;
        }
    }
}