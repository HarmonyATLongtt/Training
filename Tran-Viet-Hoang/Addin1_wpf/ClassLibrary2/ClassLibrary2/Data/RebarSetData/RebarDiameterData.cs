using Autodesk.Revit.DB.Structure;

namespace ClassLibrary2.Data
{
    public class RebarDiameterData
    {
        public int[] RebarDiameterS { get; set; }
        public double Type { get; set; } //diameter : đường kính

        public RebarBarType Rebartype { get; set; }
        public RebarDiameterData()
        {
            int[] duongkinhcautao = { 16, 19, 22, 25 };
            RebarDiameterS = duongkinhcautao;
          
        }
    }
}