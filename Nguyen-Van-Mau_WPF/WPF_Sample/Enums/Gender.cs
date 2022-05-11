using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Sample.Enums
{
    /// <summary>
    /// Represent the gender
    /// </summary>
    public enum Gender
    {
        [Description("Chưa rõ")]
        None,

        [Description("Nam")]
        Male,

        [Description("Nữ")]
        Female,
    }
}