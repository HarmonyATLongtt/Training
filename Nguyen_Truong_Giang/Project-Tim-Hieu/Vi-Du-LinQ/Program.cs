using System;
using System.Collections.Generic;
using System.Linq;

namespace Vi_Du_LinQ
{
    public class Product
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public double Price { set; get; }
        public string[] Colors { set; get; }
        public string Brand { set; get; }
        public Product(int id, string name, double price, string[] colors, string brand)
        {
            ID = id; Name = name; Price = price; Colors = colors; Brand = brand;
        }
        public string GetInfoProDuct()
        {
            return $"ID: {ID} \nName: {Name} \nPrice: {Price} \nBrand: {Brand} \nColor: {string.Join(",", Colors)}";
        }
    }

    public class Brand
    {
        public string Name { set; get; }
        public int ID { set; get; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var brands = new List<Brand>() {
                new Brand{ID = 1, Name = "Công ty AAA"},
                new Brand{ID = 2, Name = "Công ty BBB"},
                new Brand{ID = 4, Name = "Công ty CCC"},
            };

            var products = new List<Product>()
            {
                new Product(1, "Bàn trà",    400, new string[] {"Xám", "Xanh"},         "NOWSAIGON"),
                new Product(2, "Tranh treo", 400, new string[] {"Vàng", "Xanh"},        "COOLMATE"),
                new Product(3, "Đèn trùm",   500, new string[] {"Trắng"},               "DEGREY"),
                new Product(4, "Bàn học",    200, new string[] {"Trắng", "Xanh"},       "FREAKER"),
                new Product(5, "Túi da",     300, new string[] {"Đỏ", "Đen", "Vàng"},   "MIKENCO"),
                new Product(6, "Giường ngủ", 500, new string[] {"Trắng"},               "GORI"),
                new Product(7, "Tủ áo",      600, new string[] {"Trắng"},               "5TheWay"),
                new Product(8, "Máy tính",   300, new string[] {"Đen", "Xanh"},         "HA NOI RIOT"),
                new Product(8, "Màn hình",   600, new string[] {"Đen", "Đỏ"},           "GEARVN"),
            };

            //Tìm kết quả có giá = 300
            var ketqua = from product in products
                         where product.Price == 300
                         select product;

            //Tìm kết quả có Color màu Trắng
            var whiteProducts = from Product in products
                                from Colors in Product.Colors
                                where Colors == "Trắng"
                                select Product;

            //Sắp xếp giá theo thứ tự lớn đến đến 
            var sapXep = from Product in products
                         where Product.Price <= 600
                         orderby Product.Price descending
                         select Product;

            //Group theo từng nhóm giá
            var groupTheoGia = from Product in products
                         where Product.Price >= 400 && Product.Price <= 600
                         group Product by Product.Price;



            foreach (var product in ketqua)
            {
                Console.WriteLine(product.GetInfoProDuct());
            }

            foreach (var group in groupTheoGia)
            {
                Console.WriteLine(group.Key);
                foreach (var product in group)
                {
                    Console.WriteLine($"{product.Name} - {product.Price}");
                }
            }

            foreach (var product in sapXep)
            {
                Console.WriteLine(product.GetInfoProDuct());
            }
            Console.ReadLine();
        }
    }
}
