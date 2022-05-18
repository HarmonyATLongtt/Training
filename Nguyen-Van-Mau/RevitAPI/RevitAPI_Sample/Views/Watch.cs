using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitAPI_Sample.Views
{
    public partial class FormWatch : Form
    {
        internal string Value { get; set; }

        public FormWatch()
        {
            InitializeComponent();
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("Copy", CopyClick));
            rtxtValue.ContextMenu = menu;
            //ContextMenuStrip ctx = new ContextMenuStrip();
            //ctx.
            //MainMenu

            //ContextMenuStrip contextMenu = new ContextMenuStrip();
            //contextMenu.Items.Add()
            //MenuStrip mnu;
            //mnu.Items.Add()
        }

        private void CopyClick(object sender, EventArgs e)
        {
            rtxtValue.Copy();
        }

        private void FrmWatch_Load(object sender, EventArgs e)
        {
            rtxtValue.Text = Value;
        }

        private void FormWatch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                this.Dispose();
            }
        }
    }
}