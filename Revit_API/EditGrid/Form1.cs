using System;
using System.Windows.Forms;

namespace EditGrid
{
    public partial class Form1 : Form
    {
        public double distance;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            double.TryParse(txtDistance.Text, out distance);
            Close();
            return;
        }
    }
}
