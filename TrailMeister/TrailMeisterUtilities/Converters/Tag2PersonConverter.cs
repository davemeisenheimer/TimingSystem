using System;
using System.Globalization;
using System.Windows.Data;

using TrailMeisterDb;

namespace TrailMeisterUtilities.Converters
{
    // Converts DbTag and DbPerson to Person's name
    public class Tag2PersonConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is long personId &&
                values[1] is ObservableKeyedCollection<long, DbPerson> people)
            {
                NameConversionType conversiontype = NameConversionType.FirstLastAndNickName;

                if (people.Count == 0) return null;

                if (values.Length == 3)
                {
                    conversiontype = values[2] is NameConversionType ? (NameConversionType)values[2] : conversiontype;
                }

                DbPerson person = people[personId];

                Person2NameConverter person2NameConverter = new Person2NameConverter();
                return person2NameConverter.Convert(new object[] { conversiontype, person }, targetType, parameter, culture);
            }

            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
