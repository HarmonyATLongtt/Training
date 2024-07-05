namespace HRManagementSoftware
{
    partial class frmDepartmentInfor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtnumberOfEmp = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDeputyName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtManagerName = new System.Windows.Forms.TextBox();
            this.lbNameManager = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lbName = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtnumberOfEmp
            // 
            this.txtnumberOfEmp.BackColor = System.Drawing.Color.White;
            this.txtnumberOfEmp.Enabled = false;
            this.txtnumberOfEmp.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtnumberOfEmp.Location = new System.Drawing.Point(213, 196);
            this.txtnumberOfEmp.Name = "txtnumberOfEmp";
            this.txtnumberOfEmp.Size = new System.Drawing.Size(318, 26);
            this.txtnumberOfEmp.TabIndex = 28;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(23, 199);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(146, 18);
            this.label5.TabIndex = 27;
            this.label5.Text = "Tổng số nhân viên:";
            // 
            // txtDeputyName
            // 
            this.txtDeputyName.BackColor = System.Drawing.Color.White;
            this.txtDeputyName.Enabled = false;
            this.txtDeputyName.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDeputyName.Location = new System.Drawing.Point(213, 146);
            this.txtDeputyName.Name = "txtDeputyName";
            this.txtDeputyName.Size = new System.Drawing.Size(318, 26);
            this.txtDeputyName.TabIndex = 22;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(23, 149);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 18);
            this.label2.TabIndex = 21;
            this.label2.Text = "Phó Phòng:";
            // 
            // txtManagerName
            // 
            this.txtManagerName.BackColor = System.Drawing.Color.White;
            this.txtManagerName.Enabled = false;
            this.txtManagerName.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtManagerName.Location = new System.Drawing.Point(213, 99);
            this.txtManagerName.Name = "txtManagerName";
            this.txtManagerName.Size = new System.Drawing.Size(318, 26);
            this.txtManagerName.TabIndex = 20;
            // 
            // lbNameManager
            // 
            this.lbNameManager.AutoSize = true;
            this.lbNameManager.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNameManager.Location = new System.Drawing.Point(23, 102);
            this.lbNameManager.Name = "lbNameManager";
            this.lbNameManager.Size = new System.Drawing.Size(116, 18);
            this.lbNameManager.TabIndex = 19;
            this.lbNameManager.Text = "Trưởng Phòng:";
            // 
            // txtName
            // 
            this.txtName.BackColor = System.Drawing.Color.White;
            this.txtName.Enabled = false;
            this.txtName.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtName.Location = new System.Drawing.Point(214, 53);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(318, 26);
            this.txtName.TabIndex = 18;
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbName.Location = new System.Drawing.Point(23, 56);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(121, 18);
            this.lbName.TabIndex = 17;
            this.lbName.Text = "Tên Phòng ban:";
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Image = global::HRManagementSoftware.Properties.Resources.Save_32x32;
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSave.Location = new System.Drawing.Point(24, 6);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 34);
            this.btnSave.TabIndex = 31;
            this.btnSave.Text = "Save";
            this.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Visible = false;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // frmDepartmentInfor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 251);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtnumberOfEmp);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtDeputyName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtManagerName);
            this.Controls.Add(this.lbNameManager);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lbName);
            this.Name = "frmDepartmentInfor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thông tin Phòng ban";
            this.Shown += new System.EventHandler(this.FrmDepartmentInfor_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtnumberOfEmp;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDeputyName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtManagerName;
        private System.Windows.Forms.Label lbNameManager;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lbName;
    }
}