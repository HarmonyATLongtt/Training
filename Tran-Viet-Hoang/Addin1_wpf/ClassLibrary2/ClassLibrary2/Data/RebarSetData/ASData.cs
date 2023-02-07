using Autodesk.Revit.DB.Structure;
using System;

namespace ClassLibrary2.Data
{
    public class ASData
    {
        public RebarBarType BarType { get; set; }
        public double Length { get; set; }
        public double AS { get; set; }
        public int Quantity { get; set; }

        public ASData(RebarBarType barType, double length, double minSpacing)
        {
            BarType = barType;
            Length = length;
            Quantity = (int)(Math.Floor((Length + minSpacing) / (barType.BarDiameter + minSpacing)));
            AS = Math.Pow(barType.BarDiameter, 2) * Math.PI / 4 * Quantity;
        }
    }
}