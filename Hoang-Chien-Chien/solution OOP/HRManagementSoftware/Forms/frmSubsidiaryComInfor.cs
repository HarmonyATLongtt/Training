using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRManagementSoftware
{
    
    public partial class frmSubsidiaryComInfor : Form
    {
        #region Variable
        public Guid idSubsidiaryCom;
        public string presidentName;
        public bool stateChange;
        bool addSate = true;
        #endregion
        public frmSubsidiaryComInfor()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }
        #region Load data
        void LoadInfor(Guid idSubsidiaryCom)
        {
            if (stateChange)
            {
                txtName.Enabled = txtAddress.Enabled = txtDoB.Enabled = txtBusinessLicense.Enabled = txtDescription.Enabled = true;
                btnSave.Visible = true;
                dgvGoodEmp.Visible = false;
                this.Size = new System.Drawing.Size(564, 450);
            }
            SubsidiaryCom item = Global.subsidiaryComs.Find(p=>p.Id == idSubsidiaryCom);
            if (item == null) return;
            
            txtName.Text = item.Name;
            txtAddress.Text = item.Address;
            txtPresident.Text = presidentName;
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
            txtnumberOfEmp.Text = Global.employees.Where(p=> p.Position != Global.ChucVu.TongGiamDoc && p.IdSubsidiaryCom == item.Id).ToList().Count().ToString();
        }
        void LoadGoodEmplyee(Guid idSubCom)
        {
            List<Employee> itemEmp = Global.employees.Where(p => p.Position != Global.ChucVu.TongGiamDoc && p.Score >= 8.5 && p.IdSubsidiaryCom == idSubCom).ToList();
            dgvGoodEmp.DataSource = null;
            dgvGoodEmp.AutoGenerateColumns = false;
            List<EmployeeDTO> dataSource = itemEmp.Select(d => new EmployeeDTO
            {
                Id = d.ID,
                Name = d.Name,
                Score = d.Score,
                Position = Global.GetEnumDescription(d.Position),
            }).ToList();
            dgvGoodEmp.DataSource = dataSource;

        }
        
        private void FrmSubsidiaryComInfor_Shown(object sender, EventArgs e)
        {
            LoadInfor(idSubsidiaryCom);
            LoadGoodEmplyee(idSubsidiaryCom);
        }
        #endregion
        #region Save
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
            SubsidiaryCom subsidiaryCom = new SubsidiaryCom();            
            if(Global.subsidiaryComs.Where(p=>p.Id == idSubsidiaryCom).FirstOrDefault() != null)
            {
                subsidiaryCom = Global.subsidiaryComs.Where(p => p.Id == idSubsidiaryCom).FirstOrDefault();
                addSate = false;
            }
            subsidiaryCom.Name = txtName.Text;
            subsidiaryCom.Address = txtAddress.Text;
            subsidiaryCom.Dob = dob;
            subsidiaryCom.BusinessLicense = txtBusinessLicense.Text;
            subsidiaryCom.Description = txtDescription.Text;
            if (addSate)
            {
                Global.subsidiaryComs.Add(subsidiaryCom);           
            }
            return true;
            
        }
        #endregion
    }
}
