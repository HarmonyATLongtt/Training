using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRManagementSoftware
{
    public partial class frmDepartmentInfor : Form
    {
        #region Variable
        public Guid idDepartment;
        public Guid idSubsidiaryCom;
        public string ManagerName;
        public string DeputyName;
        public bool stateChange;
        bool addSate = true;
        #endregion       
        public frmDepartmentInfor()
        {
            InitializeComponent();
        }      
        #region Load Data
        void LoadInfor(Guid idSubsidiaryCom)
        {
            if (stateChange)
            {
                txtName.Enabled = true;
                btnSave.Visible = true;               
            }
            Department item = Global.departments.Find(p => p.Id == idDepartment);
            if (item == null) return;

            txtName.Text = item.Name;
            txtManagerName.Text = ManagerName;
            txtDeputyName.Text = DeputyName;          
            txtnumberOfEmp.Text = Global.employees.Where(p => p.IdDep == item.Id).ToList().Count().ToString();
        }

        private void FrmDepartmentInfor_Shown(object sender, EventArgs e)
        {
            LoadInfor(idDepartment);
        }
        #endregion
        #region Lưu thông tin
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (Save())
                this.DialogResult = DialogResult.OK;
        }
        
        bool Save()
        {
            if (String.IsNullOrEmpty(txtName.Text.Trim()))
            {
                MessageBox.Show("VUI LÒNG NHẬP ĐẦY ĐỦ THÔNG TIN CẦN THIẾT", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
            Department department = new Department();
            department.IdSubsidiaryCom = idSubsidiaryCom;
            if (Global.departments.Where(p => p.Id == idDepartment).FirstOrDefault() != null)
            {
                department = Global.departments.Where(p => p.Id == idDepartment).FirstOrDefault();
                addSate = false;
            }
            department.Name = txtName.Text;          

            if (addSate)
            {
                Global.departments.Add(department);
            }
            return true;

        }
        #endregion
    }
}
