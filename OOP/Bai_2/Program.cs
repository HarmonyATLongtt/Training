using OfficeOpenXml;
using Bai_2;
using System.Xml;

class Program
{
    static List<person> personlist = new List<person> { };


    static void Init()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        FileInfo existingFile = new FileInfo(@"F:\Coding\c#\Excel_data\Bai2.xlsx");
        using (ExcelPackage package = new ExcelPackage(existingFile))
        {
            TaxData mytax = new TaxData();
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            int colCount = worksheet.Dimension.End.Column;
            int rowCount = worksheet.Dimension.End.Row;
            for (int row = 2; row <= rowCount; row++)
            {
                string id = worksheet.Cells[row, 1].Value.ToString().Trim();
                string name = worksheet.Cells[row, 2].Value.ToString().Trim();
                string age = worksheet.Cells[row, 3].Value.ToString().Trim();
                string income = worksheet.Cells[row, 4].Value.ToString().Trim();
                person myperson = new person(id, name, age, income, mytax);
                foreach (person person in personlist)
                {
                    if (person.Equals(myperson) == false)
                    {
                        return;
                    }
                }
                personlist.Add(myperson);
            }

        }
    }
    static void Output()
    {
        for (int i = 0; i < personlist.Count; i++)
        {
            Console.WriteLine("------------------");
            Console.WriteLine("Person number {0}: ", i + 1);
            Console.WriteLine("Id: {0}", personlist[i].Id);
            Console.WriteLine("Name: {0}", personlist[i].Name);
            Console.WriteLine("Tax: {0}", personlist[i].GetTax());
        }
    }
    static void Main(string[] args)
    {
        Init();
        Output();

    }

}

