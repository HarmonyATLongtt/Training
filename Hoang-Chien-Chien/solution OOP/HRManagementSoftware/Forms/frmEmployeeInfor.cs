using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static HRManagementSoftware.Global;

namespace HRManagementSoftware
{
    public partial class frmEmployeeInfor : Form
    {
        #region Variable
        public Guid idEmployee;
        public Guid idDepartment;
        public Guid idSubsidiaryCom;
        public string ManagerName;
        public string DeputyName;
        public bool stateChange;
        bool addSate = true;
        List<string> companyName = new List<string>();
        List<string> positionName = new List<string>();
        #endregion
        public frmEmployeeInfor()
        {
            InitializeComponent();           
            companyName.Add("Tổng công ty");
            foreach (ChucVu position in Enum.GetValues(typeof(ChucVu)))
            {
                positionName.Add(Global.GetEnumDescription(position));
            }
            foreach (SubsidiaryCom company in subsidiaryComs)
            {
                companyName.Add(company.Name);
            }        
            LoadCbCompany();           
        }
        #region Load data Combobox
        void LoadCbCompany()
        {           
            cbCompany.DataSource = companyName;
            cbCompany.SelectedIndex = -1;
        }
        void LoadCbDepartment()
        {
            if(companyName.Count <= 1) return;
            cbDepartment.DataSource = departments.FindAll(p => p.IdSubsidiaryCom == subsidiaryComs[cbCompany.SelectedIndex - 1].Id).ToList();
            cbDepartment.DisplayMember = "Name";          
            cbDepartment.SelectedIndex = -1;
        }
        void LoadCbPosition()
        {
            
            cbPosition.DataSource = positionName;           
            cbPosition.SelectedIndex = -1;
        }
        private void CbCompany_SelectedValueChanged(object sender, EventArgs e)
        {

            if (cbCompany.SelectedIndex < 0)
            {
                cbDepartment.DataSource = null;
                cbPosition.DataSource = null;
                return;
            }
            if (cbCompany.SelectedIndex == 0)
            {
                cbDepartment.DataSource = null;
                cbPosition.DataSource = positionName.Take(1).ToList();
                return;
            }
            LoadCbDepartment();
            LoadCbPosition();
        }
        #endregion
        #region Load data
        void LoadInfor(Guid idEmployee)
        {
            cbCompany.SelectedIndex = subsidiaryComs.IndexOf(subsidiaryComs.Find(p=>p.Id == idSubsidiaryCom)) + 1;
            cbDepartment.SelectedIndex = departments.Where(p=>p.IdSubsidiaryCom == subsidiaryComs[cbCompany.SelectedIndex - 1].Id).ToList().IndexOf(departments.Find(p => p.Id == idDepartment));
            if (stateChange)
            {
                txtName.Enabled = txtDoB.Enabled = txtCitizenId.Enabled = txtContractDate.Enabled = cbPosition.Enabled = txtSalary.Enabled = txtScore.Enabled = true;
                btnSave.Visible = true;              
            }
            Employee item = Global.employees.Find(p => p.ID == idEmployee);
            if (item == null) return;
            
            txtName.Text = item.Name;
            txtCitizenId.Text = item.CitizenId;
            txtSalary.Text = item.Salary.ToString();
            txtScore.Text = item.Score.ToString();
            DateTime date;
            if (DateTime.TryParse(item.Dob.ToString(), out date))
            {
                txtDoB.Text = date.ToString("dd/MM/yyyy");
            }
            else
            {
                txtDoB.Text = string.Empty;
            }
            if (DateTime.TryParse(item.ContractDate.ToString(), out date))
            {
                txtContractDate.Text = date.ToString("dd/MM/yyyy");
            }
            else
            {
                txtContractDate.Text = string.Empty;
            }

            if(!String.IsNullOrEmpty(txtContractDate.Text) && item.ContractDate.HasValue)
            {
                
                TimeSpan timeSpan = DateTime.Now - item.ContractDate.Value;
                // Tính số năm, tháng, ngày từ khoảng thời gian
                int years = (int)(timeSpan.Days / 365.25);
                int remainingDays = timeSpan.Days % (int)(365.25);
                int months = remainingDays / 30;
                int days = remainingDays % 30;
                txtWorkingTime.Text = $"{years} năm {months} tháng {days} ngày";
            }
            cbPosition.SelectedIndex = (int)item.Position - 1;
            
        }
        
        private void FrmEmployeeInfor_Shown(object sender, EventArgs e)
        {
            LoadInfor(idEmployee);
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
            if (String.IsNullOrEmpty(txtCitizenId.Text.Trim()))
            {
                MessageBox.Show("VUI LÒNG NHẬP ĐẦY ĐỦ THÔNG TIN CẦN THIẾT", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            DateTime dob;
            string format = "dd/MM/yyyy";
            if (!DateTime.TryParseExact(txtDoB.Text, format, null, System.Globalization.DateTimeStyles.None, out dob))
            {
                MessageBox.Show("Vui lòng nhập thông tin ngày, tháng, năm sinh theo chuẩn định dạng dd/MM/yyyy", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            DateTime ContractDate;
            if (!DateTime.TryParseExact(txtContractDate.Text, format, null, System.Globalization.DateTimeStyles.None, out ContractDate))
            {
                MessageBox.Show("Vui lòng nhập thông tin ngày ký hợp đồng theo chuẩn định dạng dd/MM/yyyy", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (cbCompany.SelectedIndex == -1)
            {
                MessageBox.Show("VUI LÒNG CHỌN CÔNG TY", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (cbDepartment.SelectedIndex == -1 && cbCompany.SelectedIndex > 0)
            {
                MessageBox.Show("VUI LÒNG CHỌN PHÒNG BAN", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (cbPosition.SelectedIndex == -1)
            {
                MessageBox.Show("VUI LÒNG CHỌN CHỨC VỤ", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            double Salary;
            if (!double.TryParse(txtSalary.Text, out Salary) || Salary <=0 || String.IsNullOrEmpty(txtSalary.Text.Trim()))
            {
                
                MessageBox.Show("VUI LÒNG NHẬP VÀO SỐ TIỀN LƯƠNG HỢP LỆ", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            double Score;
            if (!double.TryParse(txtScore.Text, out Score) || Score < 0 || Score > 10 || String.IsNullOrEmpty(txtScore.Text.Trim()))
            {
                MessageBox.Show("VUI LÒNG NHẬP VÀO SỐ ĐIỂM HỢP LỆ", "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Employee employee = new Employee();
            
            if (Global.employees.Where(p => p.ID == idEmployee).FirstOrDefault() != null)
            {
                employee.IdSubsidiaryCom = idSubsidiaryCom;
                employee.IdDep = idDepartment;
                employee = Global.employees.Where(p => p.ID == idEmployee).FirstOrDefault();
                addSate = false;
            }
            employee.Name = txtName.Text;
            employee.Dob = dob;
            employee.CitizenId = txtCitizenId.Text;
            employee.ContractDate = ContractDate;
            if (cbCompany.SelectedIndex>0)
            {
                idSubsidiaryCom = subsidiaryComs[cbCompany.SelectedIndex - 1].Id;
                idDepartment = departments[cbDepartment.SelectedIndex].Id;
            }       
             
            employee.IdSubsidiaryCom = idSubsidiaryCom;
            employee.IdDep = idDepartment;

            if ((ChucVu)(cbPosition.SelectedIndex + 1) == ChucVu.PhoPhong)
            {
                List<Employee> employees = Global.employees.FindAll(p => p.IdSubsidiaryCom == idSubsidiaryCom && p.IdDep == idDepartment && p.Position == ChucVu.PhoPhong);
                foreach (Employee emp in employees) {
                    emp.Position = ChucVu.NhanVien;
                }
                Department department = Global.departments.Find(p=>p.Id == idDepartment);
                department.Deputy = employee;

            }
            if ((ChucVu)(cbPosition.SelectedIndex + 1) == ChucVu.TruongPhong)
            {
                List<Employee> employees = Global.employees.FindAll(p => p.IdSubsidiaryCom == idSubsidiaryCom && p.IdDep == idDepartment && p.Position == ChucVu.TruongPhong);
                foreach (Employee emp in employees)
                {
                    emp.Position = ChucVu.NhanVien;
                }
                Department department = Global.departments.Find(p => p.Id == idDepartment);
                department.Manager = employee;

            }
            if ((ChucVu)(cbPosition.SelectedIndex + 1) == ChucVu.GiamDoc)
            {
                List<Employee> employees = Global.employees.FindAll(p => p.IdSubsidiaryCom == idSubsidiaryCom && p.Position == ChucVu.GiamDoc);
                foreach (Employee emp in employees)
                {
                    emp.Position = ChucVu.NhanVien;
                }
                
                SubsidiaryCom subsidiaryCom = Global.subsidiaryComs.Find(p => p.Id == idSubsidiaryCom);
                subsidiaryCom.President = employee;
            }
            if ((ChucVu)(cbPosition.SelectedIndex + 1) == ChucVu.TongGiamDoc)
            {
                List<Employee> employees = Global.employees.FindAll(p => p.Position == ChucVu.TongGiamDoc);
                foreach (Employee emp in employees)
                {
                    emp.Position = ChucVu.NhanVien;
                }
                Global.company.President = employee;

            }
            employee.Position = (ChucVu)(cbPosition.SelectedIndex + 1);
            employee.Salary = Salary;
            employee.Score = Score;

            if (addSate)
            {
                Global.employees.Add(employee);
            }
            return true;

        }
        #endregion
        

        
    }
}
