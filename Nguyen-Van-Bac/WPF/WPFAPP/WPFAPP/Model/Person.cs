using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Text;

namespace WPFAPP.Model
{
    public class Person
    {
        private int _id;

        public int ID
        {
            get { return _id; }
            set 
            {
                if (value is int && value != null)
                {
                    _id = value;
                }
            }
        }

        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }

    }

}
