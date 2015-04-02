using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;


namespace mk.libUtils
{
    public static class DateUtils
    {
        public static DateTime GetPreviousWeekdayDate(DateTime? fromDate=null)
        {
            DateTime d = null == fromDate
                             ? DateTime.UtcNow
                             : fromDate.Value.Date;

            switch (d.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return d.AddDays(-3);
                case DayOfWeek.Sunday:
                    return d.AddDays(-2);
                default:
                    return d.AddDays(-1);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime">Source DateTime</param>
        /// <param name="convertToUTC">Default value is true.</param>
        /// <returns>A string containing the datetime in the RoundTrip format (iso-8601)</returns>
        public static string DateTimeToString(DateTime dateTime, bool convertToUTC=true)
        {
            DateTime dt = convertToUTC ? dateTime.ToUniversalTime() : dateTime;
            return dt.ToString("o");
        }


        /// <summary>
        /// Reformats the given string containing a sortable date-time to an iso-8601 conformant
        /// format date string that can be parsed as a DateTimeStyles.RoundtripKind format date.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="assumeUTC">If the timzone is not specified, this flag determines whether it should be assumed to be a UTC date. Default value is 'true'.</param>
        /// <returns>iso-8601 compliant date-string.</returns>
        /// <exception cref="System.FormatException">Throws a System.FormatException if the src string could not be parsed.</exception>
        /// 
        private static string ReformatSortableDateTimeString(string src, bool assumeUTC = true)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                throw new FormatException("The provided date-time string is either null or empty.");
            }


            Regex r = new Regex(DATE_REGEX_PATTERN, RegexOptions.IgnoreCase);
            Match m = r.Match(src);

            // doing m.Groups[1].Value is the same as m.Result("$1")
            string sfract = m.Groups[7].Value;
            string tz = m.Groups[8].Value;

            if (assumeUTC && string.IsNullOrWhiteSpace(tz))
            {
                tz = "Z";
            }

            StringBuilder s = new StringBuilder();

            s.Append(m.Groups[1].Value).Append("-").Append(m.Groups[2].Value).Append("-").Append(m.Groups[3].Value)
                .Append("T")
                .Append(m.Groups[4].Value).Append(":").Append(m.Groups[5].Value).Append(":").Append(m.Groups[6].Value);

            if (!string.IsNullOrWhiteSpace(sfract))
            {
                s.Append(".").Append(sfract);
            }

            if (!string.IsNullOrWhiteSpace(tz))
            {
                s.Append(tz);
            }


            string iso8601Date = s.ToString();

            if (iso8601Date.Length < 19)
            {
                throw new FormatException("'" + src + "' could not be parsed as a sortable date.");
            }

            return iso8601Date;
        }



        /// <summary>
        /// Reformats the given string containing a sortable date-time to an iso-8601 conformant
        /// format date string that can be parsed as a DateTimeStyles.RoundtripKind format date.
        /// 
        /// Please note that this uses the the DateTime.Parse() method, so the same Timezone and
        /// Daylight-Saving adjustments will be made to the created date.
        /// </summary>
        /// <param name="sortableDateTimeString"></param>
        /// <param name="assumeUTC">If the timzone is not specified, this flag determines whether it should be assumed to be a UTC date. Default value is 'true'.</param>
        /// <returns>A UTC DateTime object.</returns>
        /// <exception cref="System.FormatException">Throws a System.FormatException if the src string could not be parsed.</exception>
        ///
        public static DateTime ParseSortableDateTimeString(string sortableDateTimeString, bool assumeUTC = true)
        {
            string iso8601DateStr = ReformatSortableDateTimeString(sortableDateTimeString);
            return DateTime.Parse(iso8601DateStr, null, DateTimeStyles.RoundtripKind);
        }


        /// <summary>
        /// Reformats the given string containing a sortable date-time to an iso-8601 conformant
        /// format date string that can be parsed as a DateTimeStyles.RoundtripKind format date.
        /// 
        /// Please note that this uses the the DateTime.Parse() method, so the same Timezone and
        /// Daylight-Saving adjustments will be made to the created date.
        /// 
        /// This method does not throw any exceptions. It just returns the date as null if it fails to parse.
        /// </summary>
        /// <param name="sortableDateTimeString"></param>
        /// <param name="assumeUTC">If the timzone is not specified, this flag determines whether it should be assumed to be a UTC date. Default value is 'true'.</param>
        /// <returns>A nullable UTC DateTime object.</returns>
        /// 
        public static DateTime? TryParseSortableDateTimeString(string sortableDateTimeString, bool assumeUTC = true)
        {
            if (string.IsNullOrWhiteSpace(sortableDateTimeString))
            {
                return null;
            }


            try
            {
                string iso8601DateStr = ReformatSortableDateTimeString(sortableDateTimeString, assumeUTC);
                return DateTime.Parse(iso8601DateStr, null, DateTimeStyles.RoundtripKind);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static bool TryParseSortableDateTimeString(string sortableDateTimeString, out DateTime dateTime, bool assumeUTC = true)
        {
            DateTime? dt = TryParseSortableDateTimeString(sortableDateTimeString, assumeUTC);

            if (null == dt)
            {
                dateTime = DateTime.MinValue;
                return false;
            }

            dateTime = dt.Value;
            return true;
        }


        public const string DATE_REGEX_PATTERN = @"(\d{4})[-/]?(0[1-9]|1[012])[-/]?([0-3]\d)\s*T?\s*([012]\d):?([0-5]\d):?([0-5]\d)\.?(\d+)?\s*(Z|[-+][0-5]\d:?[0-5]\d)?";
        public const string DATE_FORMAT_ISO8601_COMPACT = "yyyyMMddTHHmmss.fffK";
        public const string DATE_FORMAT_ISO8601 = "yyyy-MM-ddTHH:mm:ss.fffK";
        public static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
