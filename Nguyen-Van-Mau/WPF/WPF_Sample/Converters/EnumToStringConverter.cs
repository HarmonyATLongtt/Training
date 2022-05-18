using System;
using System.ComponentModel;
using System.Globalization;

using System.Windows.Data;

namespace WPF_Sample.Converters
{
    /// <summary>
    /// get description of enum
    /// </summary>
    internal class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum e)
            {
                return GetEnumDescription(e);
            }
            else
            {
                return "Not found";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            return "xxxx"/*Binding.DoNothing*/;
        }

        public static string GetEnumDescription(Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }
    }
}