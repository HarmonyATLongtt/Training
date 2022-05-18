namespace WPF_Sample.Models
{
    /// <summary>
    /// A struct support auto generate Id
    /// </summary>
    internal struct IdGenerator
    {
        private static int _currentId;

        /// <summary>
        /// Gennerate next Id
        /// </summary>
        public static int NextId => _currentId++;

        /// <summary>
        /// Reset id back to 0
        /// </summary>
        public static void Reset() => _currentId = 0;
    }
}