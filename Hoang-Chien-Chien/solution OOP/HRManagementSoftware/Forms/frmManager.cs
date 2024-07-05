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
    public partial class frmManager : Form
    {
        #region Variable
        int focusRowSub;
        int focusRowDep;
        #endregion
        public frmManager()
        {
            InitializeComponent();
            dgvSub.DataSource = null;
            dgvDep.DataSource = null;
            dgvEmp.DataSource = null;
            dgvSub.RowHeadersVisible = false;
            dgvSub.SelectionMode = DataGridViewSelectionMode.FullRowSelect;          
            dgvDep.RowHeadersVisible = false;
            dgvDep.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEmp.RowHeadersVisible = false;
            dgvEmp.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
          
            LoadSubsidiaryCom();
        }
        #region Load Data
        void LoadSubsidiaryCom()
        {         
            dgvSub.DataSource = null;
            dgvSub.AutoGenerateColumns = false;
            List<CompanyDTO> dataSource = Global.subsidiaryComs.Select(d => new CompanyDTO
            {
                Id = d.Id,
                Name = d.Name,
                PresidentName = d.President?.Name
            }).ToList();
            dgvSub.DataSource = dataSource;
            if(dataSource == null) dgvDep.DataSource = null;
        }
        void LoadDepartment(Guid idSubsidiaryCom)
        {
            List<Department> itemDep = Global.departments.Where(p=>p.IdSubsidiaryCom == idSubsidiaryCom).ToList();           
            dgvDep.DataSource = null;
            dgvDep.AutoGenerateColumns = false;
            List<DepartmentDTO> dataSource = itemDep.Select(d => new DepartmentDTO
            {
                ID = d.Id,
                IdSubsidiaryCom = d.IdSubsidiaryCom,
                Name = d.Name,
                ManagerName = d.Manager?.Name,
                DeputyName = d.Deputy?.Name
            }).ToList();
            dgvDep.DataSource = dataSource;
            if (dataSource == null)
            {
                dgvEmp.DataSource = null;
            }

        }      
        void LoadEmplyee(Guid idDep,Guid idSubCom)
        {
            List<Employee> itemEmp = new List<Employee>();
            if (viewAll)
            {
                itemEmp = Global.employees.Where(p => p.Position != Global.ChucVu.TongGiamDoc && p.IdSubsidiaryCom == idSubCom).ToList();
            } else
                itemEmp = Global.employees.Where(p =>p.Position != Global.ChucVu.TongGiamDoc && p.Position != Global.ChucVu.GiamDoc && p.IdDep == idDep && p.IdSubsidiaryCom == idSubCom).ToList();
            dgvEmp.DataSource = null;
            dgvEmp.AutoGenerateColumns = false;
            List<EmployeeDTO> dataSource = itemEmp.Select(d => new EmployeeDTO
            {
                Id = d.ID,              
                Name = d.Name,
                Score = d.Score,
                Position = Global.GetEnumDescription(d.Position),
            }).ToList();
            dgvEmp.DataSource = dataSource;
                      
        }
        private void DgvSub_SelectionChanged(object sender, EventArgs e)
        {         
            var currentRow = dgvSub.CurrentRow;
            if (currentRow == null) return;
            Guid idSubsidiaryCom = Guid.Parse(currentRow.Cells["colIdSub"].Value?.ToString());              
            LoadDepartment(idSubsidiaryCom);                       
        }
        private void DgvDep_SelectionChanged(object sender, EventArgs e)
        {          
            var currentRow = dgvDep.CurrentRow;
            if (currentRow == null) return;
            Guid idDep = Guid.Parse(currentRow.Cells["colIdDep"].Value?.ToString());
            Guid idSubsidiaryCom = Guid.Parse(currentRow.Cells["colIdSubCom"].Value?.ToString());
            LoadEmplyee(idDep, idSubsidiaryCom);         
        }
        #endregion
        #region CRUD Subsidiary Companny
        private void BtnAddSub_Click(object sender, EventArgs e)
        {
            frmSubsidiaryComInfor frm = new frmSubsidiaryComInfor();
            frm.stateChange = true;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadSubsidiaryCom();
            }
        }

        private void BtnViewSub_Click(object sender, EventArgs e)
        {
            var currentRow = dgvSub.CurrentRow;
            if (currentRow == null) return;
            Guid idSubsidiaryCom = Guid.Parse(currentRow.Cells["colIdSub"].Value?.ToString());            
            frmSubsidiaryComInfor frm = new frmSubsidiaryComInfor();
            frm.idSubsidiaryCom = idSubsidiaryCom;
            frm.presidentName = currentRow.Cells["colNamePresident"].Value?.ToString();
            frm.ShowDialog();
        }

        private void BtnEditSubCom_Click(object sender, EventArgs e)
        {
            var currentRow = dgvSub.CurrentRow;
            if (currentRow == null) return;
            Guid idSubsidiaryCom = Guid.Parse(currentRow.Cells["colIdSub"].Value?.ToString());          
            frmSubsidiaryComInfor frm = new frmSubsidiaryComInfor();
            frm.idSubsidiaryCom = idSubsidiaryCom;
            frm.stateChange = true;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadSubsidiaryCom();
            }
        }

        private void BtnDeleteSubCom_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("BẠN CÓ CHẮC MUỐN XÓA CÔNG TY ĐƯỢC CHỌN NÀY KHÔNG?", "CẢNH BÁO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                foreach (DataGridViewRow selectedRow in dgvSub.SelectedRows)
                {
                    var currentRow = selectedRow;
                    if (currentRow == null) return;
                    Guid idSubsidiaryCom = Guid.Parse(currentRow.Cells["colIdSub"].Value?.ToString());
                    
                    Global.subsidiaryComs.Remove(Global.subsidiaryComs.Find(p => p.Id == idSubsidiaryCom));
                    Global.departments.Remove(Global.departments.Find(p => p.IdSubsidiaryCom == idSubsidiaryCom));
                    Global.employees.Remove(Global.employees.Find(p => p.IdSubsidiaryCom == idSubsidiaryCom));
                    
                }
            }
            LoadSubsidiaryCom();
            if (Global.subsidiaryComs.Count() <= 0)
            {
                dgvDep.AutoGenerateColumns = false;
                dgvDep.DataSource = null;
                dgvEmp.DataSource = null;
            } else
                LoadDepartment(Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString()));
        }

        #endregion
        #region CRUD Department
        private void BtnViewDep_Click(object sender, EventArgs e)
        {
            var currentRow = dgvDep.CurrentRow;
            if (currentRow == null) return;
            Guid idDep = Guid.Parse(currentRow.Cells["colIdDep"].Value?.ToString());
            
            frmDepartmentInfor frm = new frmDepartmentInfor();
            frm.idDepartment = idDep;
            frm.ManagerName = currentRow.Cells["colMangerName"].Value?.ToString();
            frm.DeputyName = currentRow.Cells["colDeputy"].Value?.ToString();
            frm.ShowDialog();
        }

        private void BtnAddDep_Click(object sender, EventArgs e)
        {
            var currentRowSub = dgvSub.CurrentRow;
            if (currentRowSub == null) return;
            Guid idSubsidiaryCom = Guid.Parse(currentRowSub.Cells["colIdSub"].Value?.ToString());
            
            frmDepartmentInfor frm = new frmDepartmentInfor();
            frm.idSubsidiaryCom = idSubsidiaryCom;
            frm.stateChange = true;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadDepartment(idSubsidiaryCom);
            }
        }

        private void BtnEditDep_Click(object sender, EventArgs e)
        {
            var currentRow = dgvDep.CurrentRow;
            if (currentRow == null) return;
            Guid idDep = Guid.Parse(currentRow.Cells["colIdDep"].Value?.ToString());
            frmDepartmentInfor frm = new frmDepartmentInfor();
            frm.idDepartment = idDep;
            frm.ManagerName = currentRow.Cells["colMangerName"].Value?.ToString();
            frm.stateChange = true;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                var currentRowSub = dgvSub.CurrentRow;
                Guid idSubsidiaryCom = Guid.Parse(currentRowSub.Cells["colIdSub"].Value?.ToString());
                LoadDepartment(idSubsidiaryCom);
            }
        }

        private void BtnDeleteDep_Click(object sender, EventArgs e)
        {
            
            if (MessageBox.Show("BẠN CÓ CHẮC MUỐN XÓA PHÒNG BAN ĐÃ ĐƯỢC CHỌN KHÔNG?", "CẢNH BÁO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                foreach (DataGridViewRow selectedRow in dgvDep.SelectedRows)
                {
                    var currentRow = selectedRow;
                    if (currentRow == null) return;
                    Guid idDepartment = Guid.Parse(currentRow.Cells["colIdDep"].Value?.ToString());


                    Global.departments.Remove(Global.departments.Find(p => p.Id == idDepartment));
                    Global.employees.Remove(Global.employees.Find(p => p.IdDep == idDepartment));
                    

                }
            }
            LoadDepartment(Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString()));
            if (Global.departments.Where(p=>p.IdSubsidiaryCom == Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString())).Count() <= 0)
            {
                dgvEmp.DataSource = null;
                dgvEmp.AutoGenerateColumns = false;
            } else
                LoadEmplyee(Guid.Parse(dgvDep.CurrentRow.Cells["colIdDep"].Value?.ToString()), Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString()));
        }

        #endregion
        #region CRUD Employee
        private void BtnViewEmp_Click(object sender, EventArgs e)
        {
            var currentRow = dgvSub.CurrentRow;
            if (currentRow == null) return;
            Guid idSubsidiaryCom = Guid.Parse(currentRow.Cells["colIdSub"].Value?.ToString());            
            focusRowSub = currentRow.Index;

            currentRow = dgvDep.CurrentRow;
            if (currentRow == null) return;
            Guid idDep = Guid.Parse(currentRow.Cells["colIdDep"].Value?.ToString());
            focusRowDep = currentRow.Index;

            currentRow = dgvEmp.CurrentRow;
            if (currentRow == null) return;
            Guid idEmp = Guid.Parse(currentRow.Cells["colIdEmp"].Value?.ToString());

            frmEmployeeInfor frm = new frmEmployeeInfor();
            frm.idDepartment = idDep;
            frm.idSubsidiaryCom = idSubsidiaryCom;
            frm.stateChange = true;
            frm.idEmployee = idEmp;
            frm.ShowDialog();
        }

        private void BtnAddEmp_Click(object sender, EventArgs e)
        {
            var currentRow = dgvSub.CurrentRow;
            if (currentRow == null) return;
            Guid idSubsidiaryCom = Guid.Parse(currentRow.Cells["colIdSub"].Value?.ToString());           
            focusRowSub = currentRow.Index;

            currentRow = dgvDep.CurrentRow;
            if (currentRow == null) return;
            Guid idDep = Guid.Parse(currentRow.Cells["colIdDep"].Value?.ToString());
            focusRowDep = currentRow.Index;

            frmEmployeeInfor frm = new frmEmployeeInfor();
            frm.idDepartment = idDep;
            frm.idSubsidiaryCom = idSubsidiaryCom;
            frm.stateChange = true;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadDepartment(idSubsidiaryCom);                
                dgvSub.CurrentCell = dgvSub.Rows[focusRowSub].Cells["colNameSub"];
                dgvSub.Rows[focusRowSub].Selected = true;
                dgvDep.CurrentCell = dgvDep.Rows[focusRowDep].Cells["colName"];
                dgvDep.Rows[focusRowDep].Selected = true;
                LoadSubsidiaryCom();
                LoadEmplyee(idDep, idSubsidiaryCom);
            }
        }

        private void BtnEditEmp_Click(object sender, EventArgs e)
        {
            var currentRow = dgvEmp.CurrentRow;
            if (currentRow == null) return;
            Guid idEmp = Guid.Parse(currentRow.Cells["colIdEmp"].Value?.ToString());

            currentRow = dgvSub.CurrentRow;
            if (currentRow == null) return;
            Guid idSubsidiaryCom = Guid.Parse(currentRow.Cells["colIdSub"].Value?.ToString());          
            focusRowSub = currentRow.Index;

            currentRow = dgvDep.CurrentRow;         
            if (currentRow == null) return;
            Guid idDep = Guid.Parse(currentRow.Cells["colIdDep"].Value?.ToString());
            focusRowDep = currentRow.Index;
            frmEmployeeInfor frm = new frmEmployeeInfor();
            frm.idEmployee = idEmp;
            frm.idDepartment = idDep;
            frm.idSubsidiaryCom = idSubsidiaryCom;
            frm.stateChange = true;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadSubsidiaryCom();            
                dgvSub.CurrentCell = dgvSub.Rows[focusRowSub].Cells["colNameSub"];
                dgvSub.Rows[focusRowSub].Selected = true;
                dgvDep.CurrentCell = dgvDep.Rows[focusRowDep].Cells["colName"];
                dgvDep.Rows[focusRowDep].Selected = true;
                LoadDepartment(idSubsidiaryCom);
                LoadEmplyee(idDep, idSubsidiaryCom);

            }
        }

        private void BtnDeleteEmp_Click(object sender, EventArgs e)
        {
            
            if (MessageBox.Show("BẠN CÓ CHẮC MUỐN XÓA NHỮNG NHÂN VIÊN ĐÃ ĐƯỢC CHỌN KHÔNG?", "CẢNH BÁO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                foreach (DataGridViewRow selectedRow in dgvEmp.SelectedRows)
                {
                    var currentRow = selectedRow;
                    if (currentRow == null) return;
                    Guid idEmp = Guid.Parse(currentRow.Cells["colIdEmp"].Value?.ToString());
                    Global.employees.Remove(Global.employees.Find(p => p.ID == idEmp));
                    ResestPosition(idEmp);
                }
            }
            LoadEmplyee(Guid.Parse(dgvDep.CurrentRow.Cells["colIdDep"].Value?.ToString()), Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString()));
            LoadDepartment(Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString()));
            LoadSubsidiaryCom();    
            if (Global.employees.Where(p => p.IdSubsidiaryCom == Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString()) && p.IdDep == Guid.Parse(dgvDep.CurrentRow.Cells["colIdDep"].Value?.ToString())).Count() <= 0)
            {
                dgvEmp.DataSource = null;
                dgvEmp.AutoGenerateColumns = false;
            } else
                LoadEmplyee(Guid.Parse(dgvDep.CurrentRow.Cells["colIdDep"].Value?.ToString()), Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString()));
        }
        #endregion
        #region Other
        public void ResestPosition(Guid idEmp)
        {
            if (Global.company.President != null && idEmp == Global.company.President.ID) Global.company.President = null;
            foreach(SubsidiaryCom subsidiaryCom in Global.subsidiaryComs)
            {
                if (subsidiaryCom.President != null && idEmp == subsidiaryCom.President.ID) subsidiaryCom.President = null;
            }
            foreach (Department department in Global.departments)
            {
                if (department.Manager != null && department.Manager.ID == idEmp) department.Manager = null;
                if (department.Deputy != null && department.Deputy.ID == idEmp) department.Deputy = null;
            }
        }

        bool viewAll;
        private void BtnViewMode_Click(object sender, EventArgs e)
        {
            if (viewAll)
            {
                btnViewMode.Text = "Xem toàn bộ nhân viên";
                btnViewMode.Size = new Size(166, 34);
                lbMode.Text = "Danh sách nhân viên của Phòng ban";
                viewAll = false;
                if (dgvDep.CurrentRow == null || dgvSub.CurrentRow == null) return;
                LoadEmplyee(Guid.Parse(dgvDep.CurrentRow.Cells["colIdDep"].Value?.ToString()), Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString()));               
            } else
            {
                btnViewMode.Text = "Xem nhân viên từng bộ phận";
                btnViewMode.Size = new Size(200, 34);
                lbMode.Text = "Danh sách toàn bộ nhân viên Công ty";
                viewAll = true;
                if (dgvDep.CurrentRow == null || dgvSub.CurrentRow == null) return;
                LoadEmplyee(Guid.Parse(dgvDep.CurrentRow.Cells["colIdDep"].Value?.ToString()), Guid.Parse(dgvSub.CurrentRow.Cells["colIdSub"].Value?.ToString()));               
            }
        }
        #endregion
    }
}
