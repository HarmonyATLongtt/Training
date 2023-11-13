using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace toDuc26102023.Utils
{
    public class IO
    {
        private void ShowException(Exception e)
        {
            MessageBox.Show(e.Message + "\n" + e.StackTrace);
        }
    }
}
