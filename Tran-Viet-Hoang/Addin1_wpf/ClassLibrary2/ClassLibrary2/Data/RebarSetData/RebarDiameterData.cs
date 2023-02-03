namespace ClassLibrary2.Data
{
    public class RebarDiameterData
    {
        public int[] RebarDiameterS { get; set; }
        public double Type { get; set; } //diameter : đường kính

        public RebarDiameterData()
        {
            int[] duongkinhcautao = { 16, 19, 22, 25 };
            RebarDiameterS = duongkinhcautao;
        }
    }
}