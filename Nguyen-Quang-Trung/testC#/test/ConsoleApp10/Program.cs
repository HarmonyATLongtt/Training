using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsoleApp10
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //tạo file xml
            using (XmlWriter writer = XmlWriter.Create("books.xml"))
            {
                // Root element - start tag
                writer.WriteStartElement("book");
                // Write ISBN attribute
                writer.WriteAttributeString("ISBN", "9831123212");
                // Write year attribute
                writer.WriteAttributeString("yearpublished", "2002");
                // Write title
                writer.WriteElementString("author", "Mahesh Chand");
                // Write author
                writer.WriteElementString("title", "Visual C# Programming");
                // Write price
                writer.WriteElementString("price", "44.95");
                // Root element - end tag
                writer.WriteEndElement();
                // End Documentd
                writer.WriteEndDocument();
                // Flush it
                writer.Flush();
            }
        }