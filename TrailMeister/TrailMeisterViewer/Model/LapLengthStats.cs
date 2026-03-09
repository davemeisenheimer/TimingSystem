using System.Globalization;
using TrailMeisterUtilities.Converters;

namespace TrailMeisterViewer.Model
{
    public class LapLengthStats
    {
        private static readonly Ms2TimeConverter _fmt = new();

        public int LengthM { get; init; }
        public int Count { get; init; }
        public ulong BestMs { get; init; }
        public ulong AvgMs { get; init; }
        public ulong TotalMs { get; init; }

        public string Header => $"{LengthM}m";

        public string Dist
        {
            get
            {
                double metres = (double)LengthM * Count;
                return metres > 1000
                    ? string.Format("{0:0.##}km", metres / 1000)
                    : $"{metres}m";
            }
        }

        public string Time => (string)_fmt.Convert(
            new object[] { TotalMs, TimeConversionPrecision.ToTheTenth },
            typeof(string), null!, CultureInfo.CurrentCulture);

        public string Best => (string)_fmt.Convert(
            new object[] { BestMs, TimeConversionPrecision.ToTheTenth },
            typeof(string), null!, CultureInfo.CurrentCulture);

        public string Avg => (string)_fmt.Convert(
            new object[] { AvgMs, TimeConversionPrecision.ToTheTenth },
            typeof(string), null!, CultureInfo.CurrentCulture);
    }
}
