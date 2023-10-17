namespace Create_ModelLine.Forms
{
    partial class ModelLineForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelLineForm));
            this.rdModelLine = new System.Windows.Forms.RadioButton();
            this.rdDetailLine = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbView_Level = new System.Windows.Forms.Label();
            this.cboLevel_View = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cboLineStyle = new System.Windows.Forms.ComboBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rdModelLine
            // 
            resources.ApplyResources(this.rdModelLine, "rdModelLine");
            this.rdModelLine.Checked = true;
            this.rdModelLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdModelLine.Name = "rdModelLine";
            this.rdModelLine.TabStop = true;
            this.rdModelLine.UseVisualStyleBackColor = true;
            this.rdModelLine.CheckedChanged += new System.EventHandler(this.rdModelLine_CheckedChanged);
            // 
            // rdDetailLine
            // 
            resources.ApplyResources(this.rdDetailLine, "rdDetailLine");
            this.rdDetailLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdDetailLine.Name = "rdDetailLine";
            this.rdDetailLine.UseVisualStyleBackColor = true;
            this.rdDetailLine.CheckedChanged += new System.EventHandler(this.rdDetailLine_CheckedChanged);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.rdDetailLine);
            this.groupBox1.Controls.Add(this.rdModelLine);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // lbView_Level
            // 
            resources.ApplyResources(this.lbView_Level, "lbView_Level");
            this.lbView_Level.Name = "lbView_Level";
            // 
            // cboLevel_View
            // 
            resources.ApplyResources(this.cboLevel_View, "cboLevel_View");
            this.cboLevel_View.BackColor = System.Drawing.SystemColors.Window;
            this.cboLevel_View.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLevel_View.FormattingEnabled = true;
            this.cboLevel_View.Name = "cboLevel_View";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cboLineStyle
            // 
            resources.ApplyResources(this.cboLineStyle, "cboLineStyle");
            this.cboLineStyle.BackColor = System.Drawing.SystemColors.Window;
            this.cboLineStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLineStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cboLineStyle.FormattingEnabled = true;
            this.cboLineStyle.Name = "cboLineStyle";
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnOk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOk.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnCancel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ModelLineForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.cboLineStyle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboLevel_View);
            this.Controls.Add(this.lbView_Level);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModelLineForm";
            this.Load += new System.EventHandler(this.ModelLineForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rdModelLine;
        private System.Windows.Forms.RadioButton rdDetailLine;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lbView_Level;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboLineStyle;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cboLevel_View;
    }
}