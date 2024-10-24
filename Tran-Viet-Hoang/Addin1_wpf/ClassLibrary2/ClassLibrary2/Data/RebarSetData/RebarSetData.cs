﻿using Autodesk.Revit.DB.Structure;

namespace ClassLibrary2.Data
{
    public class RebarSetData
    {
        public RebarBarType Rebartype { get; set; }
        public RebarLayoutData LayoutData { get; set; }
        public RebarShapeData ShapeData { get; set; }

        public RebarLocationData LocationData { get; set; }
        public Rebar Rebars { get; set; }
        public RebarStyle Style { get; set; }

        public RebarSetData()
        {
            LocationData = new RebarLocationData();
            LayoutData = new RebarLayoutData();
            ShapeData = new RebarShapeData();
        }
    }
}