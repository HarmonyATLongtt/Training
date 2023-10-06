using System;

namespace RevitTrainees.Forms
{
    public partial class Form_DistanceOfGrid : System.Windows.Forms.Form
    {
        public double distance;
        public bool check = false;

        public Form_DistanceOfGrid()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            double.TryParse(txtDistance.Text, out distance);
            check = true;
            Close();
            return;
        }
    }
}