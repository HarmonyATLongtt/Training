using System.Data;

namespace Exercise_4.Models
{
    public class Data
    {
        private DataTable _dataOfTable;

        public DataTable DataOfTable
        {
            get => _dataOfTable;
            set => _dataOfTable = value;
        }

        private string _name;

        public string Name
        {
            get => _name;
            set => _name = value;
        }
    }
}