using System;
using System.Globalization;
using System.Windows.Data;

using TrailMeisterDb;

namespace TrailMeisterUtilities.Converters
{
    public enum NameConversionType
    {
        First,
        Last,
        Nickname,
        FirstAndNickname,
        LastAndNickname,
        FirstLastAndNickName,
        FirstAndLast,
        PersonIdDashFirstAndLast,
        PersonIdDashNickName,
    }
    // Converts DbTag and DbPerson to Person's name
    public class Person2NameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is NameConversionType nameType &&
                values[1] is DbPerson person)
            {
                string conversion = "";

                switch (nameType)
                {
                    case NameConversionType.First:
                        return person.FirstName;
                    case NameConversionType.Last:
                        return person.LastName;
                    case NameConversionType.FirstLastAndNickName:
                        conversion =
                            person.NickName != null && person.NickName.Length > 0
                                ? person.NickName
                                : person.FirstName;

                        conversion = conversion + " (" + person.FirstName + " " + person.LastName + ")";
                        return conversion;
                    case NameConversionType.LastAndNickname:
                        conversion =
                            person.NickName != null && person.NickName.Length > 0
                                ? person.NickName + " (" + person.LastName + ")"
                                : person.LastName;

                        return conversion;
                    case NameConversionType.FirstAndNickname:
                        conversion =
                            person.NickName != null && person.NickName.Length > 0
                                ? person.NickName + " (" + person.FirstName + ")"
                                : person.FirstName;

                        return conversion;
                    case NameConversionType.Nickname:
                        return person.NickName;
                    case NameConversionType.FirstAndLast:
                        return person.FirstName + " " + person.LastName;
                    case NameConversionType.PersonIdDashFirstAndLast:
                        return person.PersonId + " - " + person.FirstName + " " + person.LastName;
                    case NameConversionType.PersonIdDashNickName:
                        return person.PersonId + " - " + person.NickName;
                }
            }

            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
