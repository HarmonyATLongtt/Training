﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using CreateFamily.Models;
using CreateFamily.ViewModel;

namespace CreateFamily
{
    /// <summary>
    /// Interaction logic for CreateFamily.xaml
    /// </summary>
    public partial class CreateFamily : Window
    {
        public CreateFamily(Document doc)
        {
            InitializeComponent();

            SelectModel model = new SelectModel(doc);
            DataContext = new MainModelView(model);
        }
    }
}
