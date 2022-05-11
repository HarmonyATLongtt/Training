namespace RevitAPI_Sample.Views
{
    partial class FormWatch
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
            this.rtxtValue = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rtxtValue
            // 
            this.rtxtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtxtValue.Location = new System.Drawing.Point(0, 0);
            this.rtxtValue.Name = "rtxtValue";
            this.rtxtValue.ReadOnly = true;
            this.rtxtValue.Size = new System.Drawing.Size(800, 450);
            this.rtxtValue.TabIndex = 0;
            this.rtxtValue.Text = "";
            // 
            // FormWatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rtxtValue);
            this.KeyPreview = true;
            this.Name = "FormWatch";
            this.Text = "Watch";
            this.Load += new System.EventHandler(this.FrmWatch_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FormWatch_KeyPress);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtxtValue;
    }
}