using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace HRManagementSoftware
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            LoadData();
            this.MaximizeBox = false;
        }
        #region Load data
        void LoadData()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.xml");

            XmlSerializer serializer = new XmlSerializer(typeof(Wrapper));

            if (!File.Exists(filePath)) return;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                Wrapper deserializedData = (Wrapper)serializer.Deserialize(fileStream);
                Console.WriteLine("Data has been deserialized from " + filePath);

                // Gán lại dữ liệu đã giải tuần tự hóa vào các thuộc tính của lớp Global
                Global.company = deserializedData.Company;
                Global.subsidiaryComs = deserializedData.SubsidiaryComs;
                Global.departments = deserializedData.Departments;
                Global.employees = deserializedData.Employees;
                       
            }

        }
        void LoadInfor()
        {
            
            Company item = Global.company;
            txtName.Text = item.Name;
            txtAddress.Text = item.Address;
            if(item.President != null)
                txtPresident.Text = item.President.Name;
            DateTime dob;
            if (DateTime.TryParse(item.Dob.ToString(), out dob))
            {
                txtDoB.Text = dob.ToString("dd/MM/yyyy");
            }
            else
            {
                txtDoB.Text = string.Empty;
            }
            txtBusinessLicense.Text = item.BusinessLicense;
            txtDescription.Text = item.Description;
            txtnumberOfEmp.Text = Global.employees.Count().ToString();
            txtNumberOfSub.Text = Global.subsidiaryComs.Count().ToString();
        }
        private void FrmMain_Shown(object sender, EventArgs e)
        {
            LoadInfor();
        }
        #endregion
        #region Function button
        private void BtnManager_Click(object sender, EventArgs e)
        {
            frmManager frm = new frmManager();
            this.Hide();
            frm.ShowDialog();
            this.Show();
            LoadData();
        }

        private void BtnEditInfor_Click(object sender, EventArgs e)
        {
            btnManager.Visible = btnEditInfor.Visible = false;
            btnCancel.Visible = btnSave.Visible = true;
            txtName.Enabled = txtAddress.Enabled = txtDoB.Enabled = txtBusinessLicense.Enabled = txtDescription.Enabled = true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            btnManager.Visible = btnEditInfor.Visible = true;
            btnCancel.Visible = btnSave.Visible = false;


            if (Save())
            {
                MessageBox.Show("THAY ĐỔI THÔNG TIN TỔNG CÔNG TY THÀNH CÔNG", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            txtName.Enabled = txtAddress.Enabled = txtDoB.Enabled = txtBusinessLicense.Enabled = txtDescription.Enabled = false;
        }
        bool Save()
        {
            if (String.IsNullOrEmpty(txtName.Text.Trim()))
            {
                MessageBox.Show("VUI LÒNG NHẬP ĐẦY ĐỦ THÔNG TIN CẦN THIẾT", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (String.IsNullOrEmpty(txtAddress.Text.Trim()))
            {
                MessageBox.Show("VUI LÒNG NHẬP ĐẦY ĐỦ THÔNG TIN CẦN THIẾT", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            DateTime dob;
            string format = "dd/MM/yyyy";
            if (!DateTime.TryParseExact(txtDoB.Text, format, null, System.Globalization.DateTimeStyles.None, out dob))
            {
                MessageBox.Show("Vui lòng nhập thông tin ngày thành lập theo chuẩn định dạng dd/MM/yyyy", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (String.IsNullOrEmpty(txtBusinessLicense.Text.Trim()))
            {
                MessageBox.Show("VUI LÒNG NHẬP ĐẦY ĐỦ THÔNG TIN CẦN THIẾT", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (String.IsNullOrEmpty(txtBusinessLicense.Text.Trim()))
            {
                MessageBox.Show("VUI LÒNG NHẬP ĐẦY ĐỦ THÔNG TIN CẦN THIẾT", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            Global.company.Name = txtName.Text;
            Global.company.Address = txtAddress.Text;
            Global.company.Dob = dob;
            Global.company.BusinessLicense = txtBusinessLicense.Text;
            Global.company.Description = txtDescription.Text;
            return true;

        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            btnManager.Visible = btnEditInfor.Visible = true;
            btnCancel.Visible = btnSave.Visible = false;
            txtName.Enabled = txtAddress.Enabled = txtDoB.Enabled = txtBusinessLicense.Enabled = txtDescription.Enabled = false;
            LoadInfor();
        }
        #endregion        
        #region Save data to XML
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.xml");

            XmlSerializer serializer = new XmlSerializer(typeof(Wrapper));


            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(fileStream, new Wrapper(Global.company,Global.subsidiaryComs,Global.departments,Global.employees));
            }
        }
        #endregion
    }
}
