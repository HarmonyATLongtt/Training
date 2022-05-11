using System.Collections.ObjectModel;

namespace WPF_Sample.Models
{
    /// <summary>
    /// Represent a main model
    /// </summary>
    public class MainModel
    {
        /// <summary>
        /// List of class
        /// </summary>
        public ObservableCollection<ClassModel> ClassModels { get; set; }

        /// <summary>
        /// Create new instance of MainModel
        /// </summary>
        public MainModel()
        {
            ClassModels = new ObservableCollection<ClassModel>();
        }
    }
}