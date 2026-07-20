using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Core
{
    public struct VersionNumber : IEquatable<VersionNumber>, IComparable<VersionNumber>
    {
        public const int MAX_MINUTES = 24 * 60;

        [JsonProperty(PropertyName = "year")]
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonProperty(PropertyName = "month")]
        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonProperty(PropertyName = "day")]
        [JsonPropertyName("day")]
        public int Day { get; set; }

        [JsonProperty(PropertyName = "minute")]
        [JsonPropertyName("minute")]
        public int Minute { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public DateTime UTC => new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc).AddMinutes(Minute);

        public bool IsValid
        {
            get
            {
                // there's zero chance of builds earlier than this
                if (Year < 2016)
                    return false;

                // not in our calendar!
                if (Month <= 0 || Month > 12)
                    return false;

                // need to do a bit of extra validation later, but catch any invalid days early
                if (Day <= 0 || Day > 31)
                    return false;

                if (Minute < 0 || Minute > MAX_MINUTES)
                    return false;

                // Do a bit fancier validations, checking whether it's "too new"
                var maxDate = DateTime.UtcNow.AddHours(1); // small tolerance

                if (UTC > maxDate)
                    return false;

                return true;
            }
        }

        public VersionNumber(int year, int month, int day, int minute)
        {
            this.Year = year;
            this.Month = month;
            this.Day = day;
            this.Minute = minute;
        }

        public VersionNumber(DateTime time)
        {
            time = time.ToUniversalTime();

            this.Year = time.Year;
            this.Month = time.Month;
            this.Day = time.Day;
            this.Minute = time.Minute + time.Hour * 60;
        }

        public VersionNumber(Version version)
        {
            this.Year = version.Major;
            this.Month = version.Minor;
            this.Day = version.Build;
            this.Minute = version.Revision;
        }

        public override string ToString() => $"{Year}.{Month}.{Day}.{Minute}";

        public static VersionNumber Parse(string str)
        {
            var parts = str.Split('.');

            if (parts.Length != 4)
                throw new ArgumentException("Input string is in incorrect format");

            var y = int.Parse(parts[0]);
            var m = int.Parse(parts[1]);
            var d = int.Parse(parts[2]);
            var mm = int.Parse(parts[3]);

            var version = new VersionNumber(y, m, d, mm);

            if (!version.IsValid)
                throw new FormatException("Invalid version");

            return version;
        }

        public static bool TryParse(string str, out VersionNumber version)
        {
            var parts = str.Split('.');

            if (parts.Length != 4)
            {
                version = default;
                return false;
            }

            if (int.TryParse(parts[0], out var y) &&
                int.TryParse(parts[1], out var m) &&
                int.TryParse(parts[2], out var d) &&
                int.TryParse(parts[3], out var mm))
            {
                version = new VersionNumber(y, m, d, mm);

                if (version.IsValid)
                    return true;
            }

            version = default;
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is VersionNumber version)
                return Equals(version);

            return false;
        }

        public bool Equals(VersionNumber other) => Year == other.Year && Month == other.Month && Day == other.Day && Minute == other.Minute;

        public int CompareTo(VersionNumber other)
        {
            if (this < other)
                return -1;
            else if (this == other)
                return 0;
            else
                return 1;
        }

        public override int GetHashCode()
        {
            int hashCode = 1651387891;
            hashCode = hashCode * -1521134295 + Year.GetHashCode();
            hashCode = hashCode * -1521134295 + Month.GetHashCode();
            hashCode = hashCode * -1521134295 + Day.GetHashCode();
            hashCode = hashCode * -1521134295 + Minute.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(VersionNumber a, VersionNumber b) => a.Equals(b);
        public static bool operator !=(VersionNumber a, VersionNumber b) => !(a == b);

        public static bool operator >(VersionNumber a, VersionNumber b)
        {
            if (a.Year > b.Year)
                return true;

            if (a.Year < b.Year)
                return false;

            if (a.Month > b.Month)
                return true;

            if (a.Month < b.Month)
                return false;

            if (a.Day > b.Day)
                return true;

            if (a.Day < b.Day)
                return false;

            if (a.Minute > b.Minute)
                return true;

            return false;
        }

        public static bool operator <(VersionNumber a, VersionNumber b)
        {
            if (a.Year < b.Year)
                return true;

            if (a.Year > b.Year)
                return false;

            if (a.Month < b.Month)
                return true;

            if (a.Month > b.Month)
                return false;

            if (a.Day < b.Day)
                return true;

            if (a.Day > b.Day)
                return false;

            if (a.Minute < b.Minute)
                return true;

            return false;
        }
    }
}
