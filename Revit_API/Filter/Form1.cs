using System;
using System.Windows.Forms;

namespace Filter
{
    public partial class Form1 : Form
    {
        public string compareArea = null;
        public float area;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            compareArea = cboRules.SelectedItem.ToString();

            if (float.TryParse(txtArea.Text, out area) == false)
            {
                MessageBox.Show("Area is invalid");
            }

            Close();

            return;
        }
    }
}
