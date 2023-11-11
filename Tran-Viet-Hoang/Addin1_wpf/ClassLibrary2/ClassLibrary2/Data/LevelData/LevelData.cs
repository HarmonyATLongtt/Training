namespace ClassLibrary2.Data
{
    public class LevelData
    {
        public double Elevation { get; set; }
        public string Name { get; set; }

        public LevelData(string name, double elev)
        {
            Name = name;
            Elevation = elev;
        }
    }
}