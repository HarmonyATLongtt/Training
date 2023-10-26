namespace RevitAPI_B1
{
    partial class Form1
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
            this.cboLevels = new System.Windows.Forms.ComboBox();
            this.cboViews = new System.Windows.Forms.ComboBox();
            this.btnLevels = new System.Windows.Forms.Button();
            this.btnViews = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cboLevels
            // 
            this.cboLevels.FormattingEnabled = true;
            this.cboLevels.Location = new System.Drawing.Point(59, 61);
            this.cboLevels.Name = "cboLevels";
            this.cboLevels.Size = new System.Drawing.Size(121, 24);
            this.cboLevels.TabIndex = 0;
            // 
            // cboViews
            // 
            this.cboViews.FormattingEnabled = true;
            this.cboViews.Location = new System.Drawing.Point(373, 62);
            this.cboViews.Name = "cboViews";
            this.cboViews.Size = new System.Drawing.Size(154, 24);
            this.cboViews.TabIndex = 0;
            // 
            // btnLevels
            // 
            this.btnLevels.Location = new System.Drawing.Point(201, 62);
            this.btnLevels.Name = "btnLevels";
            this.btnLevels.Size = new System.Drawing.Size(75, 23);
            this.btnLevels.TabIndex = 1;
            this.btnLevels.Text = "Create";
            this.btnLevels.UseVisualStyleBackColor = true;
            this.btnLevels.Click += new System.EventHandler(this.btnLevels_Click);
            // 
            // btnViews
            // 
            this.btnViews.Location = new System.Drawing.Point(553, 62);
            this.btnViews.Name = "btnViews";
            this.btnViews.Size = new System.Drawing.Size(75, 23);
            this.btnViews.TabIndex = 1;
            this.btnViews.Text = "Create";
            this.btnViews.UseVisualStyleBackColor = true;
            this.btnViews.Click += new System.EventHandler(this.btnViews_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(56, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Levels:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(370, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Views";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnViews);
            this.Controls.Add(this.btnLevels);
            this.Controls.Add(this.cboViews);
            this.Controls.Add(this.cboLevels);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboLevels;
        private System.Windows.Forms.ComboBox cboViews;
        private System.Windows.Forms.Button btnLevels;
        private System.Windows.Forms.Button btnViews;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}