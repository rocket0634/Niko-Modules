using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace KModkit
{
    public enum Battery
    {
        Unknown = 0,
        Empty = 0,
        D = 1,
        AA = 2,
        AAx3 = 3,
        AAx4 = 4
    }

    public enum Port
    {
        DVI,
        Parallel,
        PS2,
        RJ45,
        Serial,
        StereoRCA,
        ComponentVideo,
        CompositeVideo,
        USB,
        HDMI,
        VGA,
        AC,
        PCMCIA
    }

    public enum Indicator
    {
        SND,
        CLR,
        CAR,
        IND,
        FRQ,
        SIG,
        NSA,
        MSA,
        TRN,
        BOB,
        FRK,
        NLL
    }

    public enum IndicatorColor
    {
        Black,
        White,
        Blue,
        Gray,
        Green,
        Magenta,
        Orange,
        Purple,
        Red,
        Yellow
    }

    public enum DayColor
    {
        Yellow,
        Brown,
        Blue,
        White,
        Magenta,
        Green,
        Orange,
        None
    }
    
    public enum Month
    {
        JAN,
        FEB,
        MAR,
        APR,
        MAY,
        JUNE,
        JULY,
        AUG,
        SEPT,
        OCT,
        NOV,
        DEC
    }

    public enum TimeFormat
    {
        American,
        International,
        None
    }

    /// <summary>
    /// Some helper extensions methods for the KMBombInfo class.
    /// </summary>
    public static class KMBombInfoExtensions
    {
        #region JSON Types

        public static string WidgetQueryTwofactor = "twofactor";
        public static string WidgetTwofactorKey = "twofactor_key";
        public static string WidgetQueryManufacture = "manufacture";
        public static string WidgetQueryDay = "day";
        public static string WidgetQueryTime = "time";

        private class IndicatorJSON
        {
            public string label = null;
            public string on = null;

            public bool IsOn()
            {
                bool isOn = false;
                bool.TryParse(on, out isOn);
                return isOn;
            }
        }

        private class ColorIndicatorJSON
        {
            public string label = null;
            public string color = null;
        }

        private class TwoFactorJSON
        {
            public int twofactor_key = 0;
        }

        private class BatteryJSON
        {
            public int numbatteries = 0;
        }

        private class PortsJSON
        {
            public string[] presentPorts = null;
        }

        private class SerialNumberJSON
        {
            public string serial = null;
        }

        private class ManufactureJSON
        {
            public string month;
            public int year;
        }

        private class DayJSON
        {
            public string day;
            public string daycolor;
            public int date;
            public int month;
            public bool colorenabled;
            public int monthColor;
        }

        private class TimeJSON
        {
            public string time;
            public bool am;
            public bool pm;
        }

        #endregion

        #region Helpers

        private static IEnumerable<T> GetJSONEntries<T>(KMBombInfo bombInfo, string queryKey, string queryInfo)
            where T : new()
        {
            return bombInfo.QueryWidgets(queryKey, queryInfo).Select(delegate (string queryEntry)
            {
                return JsonConvert.DeserializeObject<T>(queryEntry);
            });
        }

        private static IEnumerable<IndicatorJSON> GetIndicatorEntries(KMBombInfo bombInfo)
        {
            return GetJSONEntries<IndicatorJSON>(bombInfo, KMBombInfo.QUERYKEY_GET_INDICATOR, null).Where(x => x != null);
        }

        private static IEnumerable<ColorIndicatorJSON> GetColorIndicatorEntries(KMBombInfo bombInfo)
        {
            return GetJSONEntries<ColorIndicatorJSON>(bombInfo, KMBombInfo.QUERYKEY_GET_INDICATOR + "Color", null).Where(x => x != null);
        }

        private static IEnumerable<BatteryJSON> GetBatteryEntries(KMBombInfo bombInfo)
        {
            return GetJSONEntries<BatteryJSON>(bombInfo, KMBombInfo.QUERYKEY_GET_BATTERIES, null).Where(x => x != null);
        }

        private static IEnumerable<PortsJSON> GetPortEntries(KMBombInfo bombInfo)
        {
            return GetJSONEntries<PortsJSON>(bombInfo, KMBombInfo.QUERYKEY_GET_PORTS, null).Where(x => x != null);
        }

        private static IEnumerable<SerialNumberJSON> GetSerialNumberEntries(KMBombInfo bombInfo)
        {
            return GetJSONEntries<SerialNumberJSON>(bombInfo, KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null).Where(x => x != null);
        }

        private static IEnumerable<TwoFactorJSON> GetTwoFactorEntries(KMBombInfo bombInfo)
        {
            return GetJSONEntries<TwoFactorJSON>(bombInfo, WidgetQueryTwofactor, null).Where(x => x != null);
        }

        private static IEnumerable<ManufactureJSON> GetManufactureEntries(KMBombInfo bombInfo)
        {
            return GetJSONEntries<ManufactureJSON>(bombInfo, WidgetQueryManufacture, null).Where(x => x != null);
        }

        private static IEnumerable<DayJSON> GetDayEntries(KMBombInfo bombInfo)
        {
            return GetJSONEntries<DayJSON>(bombInfo, WidgetQueryDay, null).Where(x => x != null);
        }

        private static IEnumerable<TimeJSON> GetTimeEntries(KMBombInfo bombInfo)
        {
            return GetJSONEntries<TimeJSON>(bombInfo, WidgetQueryTime, null).Where(x => x != null);
        }

        #endregion

        #region Public Extensions

        public static bool IsIndicatorPresent(this KMBombInfo bombInfo, Indicator indicatorLabel)
        {
            return bombInfo.IsIndicatorPresent(indicatorLabel.ToString());
        }

        public static bool IsIndicatorPresent(this KMBombInfo bombInfo, string indicatorLabel)
        {
            return GetIndicatorEntries(bombInfo).Any((x) => indicatorLabel.Equals(x.label));
        }

        public static bool IsIndicatorColored(this KMBombInfo bombInfo, Indicator indicatorLabel, string indicatorColor)
        {
            return IsIndicatorColored(bombInfo, indicatorLabel.ToString(), indicatorColor);
        }

        public static bool IsIndicatorColored(this KMBombInfo bombInfo, string indicatorLabel, string indicatorColor)
        {
            return GetColoredIndicators(bombInfo, indicatorColor).Any((x) => x.Equals(indicatorLabel));
        }

        public static bool IsIndicatorColorPresent(this KMBombInfo bombInfo, string indicatorColor)
        {
            return GetColoredIndicators(bombInfo, indicatorColor).Any();
        }

        public static bool IsIndicatorOn(this KMBombInfo bombInfo, Indicator indicatorLabel)
        {
            return bombInfo.IsIndicatorOn(indicatorLabel.ToString());
        }

        public static bool IsIndicatorOn(this KMBombInfo bombInfo, string indicatorLabel)
        {
            return GetIndicatorEntries(bombInfo).Any((x) => x.IsOn() && indicatorLabel.Equals(x.label));
        }

        public static bool IsIndicatorOff(this KMBombInfo bombInfo, Indicator indicatorLabel)
        {
            return bombInfo.IsIndicatorOff(indicatorLabel.ToString());
        }

        public static bool IsIndicatorOff(this KMBombInfo bombInfo, string indicatorLabel)
        {
            return GetIndicatorEntries(bombInfo).Any((x) => !x.IsOn() && indicatorLabel.Equals(x.label));
        }

        public static IEnumerable<string> GetIndicators(this KMBombInfo bombInfo)
        {
            return GetIndicatorEntries(bombInfo).Select((x) => x.label);
        }

        public static IEnumerable<string> GetOnIndicators(this KMBombInfo bombInfo)
        {
            return GetIndicatorEntries(bombInfo).Where((x) => x.IsOn()).Select((x) => x.label);
        }

        public static IEnumerable<string> GetOffIndicators(this KMBombInfo bombInfo)
        {
            return GetIndicatorEntries(bombInfo).Where((x) => !x.IsOn()).Select((x) => x.label);
        }

        public static IEnumerable<string> GetColoredIndicators(this KMBombInfo bombInfo, Indicator label)
        {
            return GetColoredIndicators(bombInfo, null, label.ToString());
        }

        public static IEnumerable<string> GetColoredIndicators(this KMBombInfo bombInfo, IndicatorColor color)
        {
            return GetColoredIndicators(bombInfo, color.ToString());
        }

        public static IEnumerable<string> GetColoredIndicators(this KMBombInfo bombInfo, string color = null, string label = null)
        {
            var Colors = new List<string> { "Black", "White", "Blue", "Gray", "Green", "Magenta", "Orange", "Purple", "Red", "Yellow" };
            if (color != null)
            {
                Colors.RemoveAt(0);
                Colors.RemoveAt(0);
                if (color.Equals("Black"))
                {
                    return GetOffIndicators(bombInfo);
                }
                if (color.Equals("White"))
                {
                    //Can't just return OnIndicators as is, due to the fact that would return ALL of them as White, even when some of them are not white.
                    List<string> OnIndicators = new List<string>(GetOnIndicators(bombInfo));

                    foreach (string c in Colors)
                    {
                        foreach (string indicator in GetColoredIndicators(bombInfo, c))
                        {
                            OnIndicators.Remove(indicator);
                        }
                    }
                    return OnIndicators;
                }

                return GetColorIndicatorEntries(bombInfo)
                    .Where((x) => x.color.Equals(color, StringComparison.InvariantCultureIgnoreCase))
                    .Select((x) => x.label);
            }
            if (label != null)
            {
                var colorList = new List<string>();
                foreach (var c in Colors)
                {
                    colorList.AddRange(from i in bombInfo.GetColoredIndicators(c) where label.Equals(i, StringComparison.InvariantCultureIgnoreCase) select c);
                }
                return colorList;
            }
            return new List<string>();
        }

        public static int GetBatteryCount(this KMBombInfo bombInfo)
        {
            return GetBatteryEntries(bombInfo).Sum((x) => x.numbatteries);
        }

        public static int GetBatteryCount(this KMBombInfo bombInfo, Battery batteryType)
        {
            return GetBatteryCount(bombInfo, (int) batteryType);
        }

        public static int GetBatteryCount(this KMBombInfo bombInfo, int batteryType)
        {
            return GetBatteryEntries(bombInfo).Where((x) => x.numbatteries == batteryType)
                .Sum((x) => x.numbatteries);
        }

        public static int GetBatteryAACount(this KMBombInfo bombInfo)
        {
            return GetBatteryEntries(bombInfo).Where((x) => x.numbatteries > 1).Sum((x) => x.numbatteries);
        }

        public static int GetBatteryHolderCount(this KMBombInfo bombInfo)
        {
            return GetBatteryEntries(bombInfo).Count();
        }

        public static int GetBatteryHolderCount(this KMBombInfo bombInfo, Battery batteryType)
        {
            return GetBatteryHolderCount(bombInfo, (int) batteryType);
        }

        public static int GetBatteryHolderCount(this KMBombInfo bombInfo, int batteryType)
        {
            return GetBatteryEntries(bombInfo).Count(x => x.numbatteries == batteryType);
        }

        public static int GetPortCount(this KMBombInfo bombInfo)
        {
            return GetPortEntries(bombInfo).Sum((x) => x.presentPorts.Length);
        }

        public static int GetPortCount(this KMBombInfo bombInfo, Port portType)
        {
            return bombInfo.GetPortCount(portType.ToString());
        }

        public static int GetPortCount(this KMBombInfo bombInfo, string portType)
        {
            return GetPortEntries(bombInfo).Sum((x) => x.presentPorts.Count((y) => portType.Equals(y)));
        }

        public static int GetPortPlateCount(this KMBombInfo bombInfo)
        {
            return GetPortEntries(bombInfo).Count();
        }

        public static IEnumerable<string> GetPorts(this KMBombInfo bombInfo)
        {
            return GetPortEntries(bombInfo).SelectMany((x) => x.presentPorts);
        }

        public static IEnumerable<string[]> GetPortPlates(this KMBombInfo bombInfo)
        {
            return GetPortEntries(bombInfo).Where(x => x != null).Select((x) => x.presentPorts);
        }

        public static bool IsPortPresent(this KMBombInfo bombInfo, Port portType)
        {
            return bombInfo.IsPortPresent(portType.ToString());
        }

        public static bool IsPortPresent(this KMBombInfo bombInfo, string portType)
        {
            return GetPortEntries(bombInfo)
                .Any((x) => x.presentPorts != null && x.presentPorts.Any((y) => portType.Equals(y)));
        }

        public static int CountUniquePorts(this KMBombInfo bombInfo)
        {
            List<string> ports = new List<string>();

            foreach (var port in GetPorts(bombInfo))
            {
                if (!ports.Contains(port))
                    ports.Add(port);
            }

            return ports.Count;
        }

        public static bool IsDuplicatePortPresent(this KMBombInfo bombInfo)
        {
            List<string> ports = new List<string>();
            foreach (var port in GetPorts(bombInfo))
            {
                if (!ports.Contains(port))
                    ports.Add(port);
                else
                    return true;
            }
            return false;
        }

        public static bool IsDuplicatePortPresent(this KMBombInfo bombInfo, Port port)
        {
            return IsDuplicatePortPresent(bombInfo, port.ToString());
        }

        public static bool IsDuplicatePortPresent(this KMBombInfo bombInfo, string port)
        {
            return GetPortCount(bombInfo, port) > 1;
        }

        public static int CountDuplicatePorts(this KMBombInfo bombInfo)
        {
            List<string> ports = new List<string>();
            foreach (var port in GetPorts(bombInfo))
            {
                if (!ports.Contains(port) && IsDuplicatePortPresent(bombInfo, port))
                    ports.Add(port);
            }
            return ports.Count;
        }

        public static string GetSerialNumber(this KMBombInfo bombInfo)
        {
            var ret = GetSerialNumberEntries(bombInfo).FirstOrDefault();
            return ret == null ? null : ret.serial;
        }

        public static IEnumerable<char> GetSerialNumberLetters(this KMBombInfo bombInfo)
        {
            return GetSerialNumber(bombInfo).Where((x) => x < '0' || x > '9');
        }

        public static IEnumerable<int> GetSerialNumberNumbers(this KMBombInfo bombInfo)
        {
            return GetSerialNumber(bombInfo).Where((x) => x >= '0' && x <= '9').Select((y) => int.Parse("" + y));
        }

        public static bool IsTwoFactorPresent(this KMBombInfo bombInfo)
        {
            return GetTwoFactorCodes(bombInfo).Any();
        }

        public static int GetTwoFactorCounts(this KMBombInfo bombInfo)
        {
            return GetTwoFactorCodes(bombInfo).Count();
        }

        public static IEnumerable<int> GetTwoFactorCodes(this KMBombInfo bombInfo)
        {
            return GetTwoFactorEntries(bombInfo).Select((x) => x.twofactor_key);
        }
        #endregion

        #region DayTime Widget Extensions
        /// <summary>
        /// Checks to see if a Date of Manufacture widget is present
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static bool IsManufacturePresent(this KMBombInfo bombInfo)
        {
            return bombInfo.IsDateOfManufacturePresent();
        }

        /// <summary>
        /// Checks to see if a Date of Manufacture widget is present
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static bool IsDateOfManufacturePresent(this KMBombInfo bombInfo)
        {
            return GetManufactureEntries(bombInfo).Any();
        }

        /// <summary>
        /// If a Date of Manufacture widget is present, returns the name of the labeled month as an enum
        /// Otherwise, returns the current month as an enum
        /// This is meant to represent a past or publishing date. Please use Day of Week's month if you wish to emulate the current day
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static Month GetManufactureMonth(this KMBombInfo bombInfo)
        {
            if (!IsDateOfManufacturePresent(bombInfo)) return (Month)DateTime.Now.Month;
            return (Month)Enum.Parse(typeof(Month), GetManufactureEntries(bombInfo).First().month);
        }

        /// <summary>
        /// If a Date of Manufacture widget is present, returns the labeled month as an integer
        /// Otherwise, returns the current month as an integer
        /// This is meant to represent a past of publishing date. Please use Day of Week's month if you wish to emulate the current day
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static int GetManufactureMonthInt(this KMBombInfo bombInfo)
        {
            return (int)GetManufactureMonth(bombInfo);
        }

        /// <summary>
        /// If a Date of Manufacture widget is present, returns the labeled year
        /// Otherwise, returns the current year
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static int GetManufactureYear (this KMBombInfo bombInfo)
        {
            if (!IsDateOfManufacturePresent(bombInfo)) return DateTime.Now.Year;
            return GetManufactureEntries(bombInfo).First().year;
        }

        /// <summary>
        /// Checks to see if a Day of the Week widget is present
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static bool IsDayOfWeekPresent(this KMBombInfo bombInfo)
        {
            return GetDayEntries(bombInfo).Any();
        }

        /// <summary>
        /// If a Day of the Week widget is present, returns the name of the labeled day as a DayOfWeek enum
        /// Otherwise, returns the name of the current day as a DayOfWeek enum
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static DayOfWeek GetDateOfWeek(this KMBombInfo bombInfo)
        {
            if (!IsDayOfWeekPresent(bombInfo)) return DateTime.Now.DayOfWeek;
            return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), GetDayEntries(bombInfo).First().day);
        }

        /// <summary>
        /// If a Day of the Week widget is present, returns the labeled day of the week as an integer, starting at 0
        /// Otherwise, returns the current day of the week as an integer, starting at 0
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static int GetDateOfWeekInt(this KMBombInfo bombInfo)
        {
            return (int)GetDateOfWeek(bombInfo);
        }

        /// <summary>
        /// If a Day of the Week widget is present, returns either Green or as the shown color for the day of week, as an enum
        /// Otherwise, returns none
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static DayColor GetDayColor(this KMBombInfo bombInfo)
        {
            if (!IsDayOfWeekPresent(bombInfo)) return DayColor.None;
            if (!GetDayEntries(bombInfo).First().colorenabled) return DayColor.Green;
            return (DayColor)Enum.Parse(typeof(DayColor), GetDayEntries(bombInfo).First().daycolor);
        }

        /// <summary>
        /// If a Day of the Week widget is present, returns either Green or as the shown color for the day of week
        /// Otherwise, returns none
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static string GetDayColorString(this KMBombInfo bombInfo)
        {
            return GetDayColor(bombInfo).ToString();
        }

        /// <summary>
        /// If a Day of the Week widget is present, returns the number that represents the day of the month
        /// Otherwise, returns the day based on the current date
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static int GetDayDateNum(this KMBombInfo bombInfo)
        {
            if (!IsDayOfWeekPresent(bombInfo)) return DateTime.Now.Day;
            return GetDayEntries(bombInfo).First().date;
        }

        /// <summary>
        /// If a Day of the Week widget is present, returns the labeled month as an enum
        /// Otherwise, returns the current month as an enum
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static Month GetDayMonth(this KMBombInfo bombInfo)
        {
            return (Month)GetDayMonthInt(bombInfo);
        }

        /// <summary>
        /// If a Day of the Week widget is present, returns the number that represents the labeled month
        /// Otherwise, returns the current month
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static int GetDayMonthInt(this KMBombInfo bombInfo)
        {
            if (!IsDayOfWeekPresent(bombInfo)) return DateTime.Now.Month;
            return GetDayEntries(bombInfo).First().month;
        }

        /// <summary>
        /// If a Day of the Week widget is present, returns the format the widget is showing in
        /// Otherwise, returns none
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static TimeFormat GetDayFormat(this KMBombInfo bombInfo)
        {
            if (!IsDayOfWeekPresent(bombInfo)) return TimeFormat.None;
            return (TimeFormat)GetDayEntries(bombInfo).First().monthColor;
        }

        /// <summary>
        /// Checks to see if a Randomized Time widget is present
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static bool IsRandomTimePresent(this KMBombInfo bombInfo)
        {
            return GetTimeEntries(bombInfo).Any();
        }

        /// <summary>
        /// Returns Randomized Time widgets as TimeSpans HH:mm:ss
        /// AM/PM are not considered in this method
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static TimeSpan[] GetRandomTimes(this KMBombInfo bombInfo)
        {
            return GetTimeEntries(bombInfo).Select(x => DateTime.ParseExact(x.time, "HHmm", System.Globalization.CultureInfo.InvariantCulture).TimeOfDay).ToArray();
        }

        /// <summary>
        /// Returns Randomized Time widgets as TimeSpans hh:mm:ss
        /// AM/PM are translated to military time in this method
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static TimeSpan[] GetExactRandomTimes(this KMBombInfo bombInfo)
        {
            var a = GetTimeEntries(bombInfo);
            var b = new List<TimeSpan>();
            foreach (TimeJSON time in a)
            {
                if (time.am || time.pm)
                    b.Add(DateTime.ParseExact(time.time + (time.am ? "AM" : "PM"), "hhmmtt", System.Globalization.CultureInfo.InvariantCulture).TimeOfDay);
                else
                    b.Add(DateTime.ParseExact(time.time, "HHmm", System.Globalization.CultureInfo.InvariantCulture).TimeOfDay);
            }
            return b.ToArray();
        }

        /// <summary>
        /// Returns the number of Randomized Time widgets (Starting Time widget is not included)
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static int GetRandomTimeCount(this KMBombInfo bombInfo)
        {
            return GetTimeEntries(bombInfo).Count();
        }

        /// <summary>
        /// Checks to see if a number is present in any Randomized Time widget
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool RandomTimeIsNumberPresent(this KMBombInfo bombInfo, string num)
        {
            if (!IsRandomTimePresent(bombInfo)) return false;
            var a = GetRandomTimes(bombInfo);
            return a.Any(x => x.Hours.ToString().Contains(num) || x.Minutes.ToString().Contains(num));
        }

        /// <summary>
        /// Checks to see if a number is present in any Randomized Time widget
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool RandomTimeIsNumberPresent(this KMBombInfo bombInfo, int num)
        {
            return RandomTimeIsNumberPresent(bombInfo, num.ToString());
        }

        /// <summary>
        /// Checks to see if any Randomized Time widget with AM is present.
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static bool RandomTimeIsAMPresent(this KMBombInfo bombInfo)
        {
            if (!IsRandomTimePresent(bombInfo)) return false;
            return GetTimeEntries(bombInfo).Any(x => x.am);
        }

        /// <summary>
        /// Checks to see if any Randomized Time widget with PM is present.
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static bool RandomTimeIsPMPresent(this KMBombInfo bombInfo)
        {
            if (!IsRandomTimePresent(bombInfo)) return false;
            return GetTimeEntries(bombInfo).Any(x => x.pm);
        }

        /// <summary>
        /// Checks to see if any Randomized Time widget showing military time is present.
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static bool RandomTimeIsMilPresent(this KMBombInfo bombInfo)
        {
            if (!IsRandomTimePresent(bombInfo)) return false;
            return GetTimeEntries(bombInfo).Any(x => !x.am && !x.pm);
        }


        /// <summary>
        /// Returns the number of Randomized Time widgets where AM is present
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static int RandomTimeAMCount(this KMBombInfo bombInfo)
        {
            return GetTimeEntries(bombInfo).Select(x => x.am).Count();
        }

        /// <summary>
        /// Returns the number of Randomized Time widgets where PM is present
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static int RandomTimePMCount(this KMBombInfo bombInfo)
        {
            return GetTimeEntries(bombInfo).Select(x => x.pm).Count();
        }

        /// <summary>
        /// Returns the number of Randomized Time widgets that are shown in military time
        /// </summary>
        /// <param name="bombInfo"></param>
        /// <returns></returns>
        public static int RandomTimeMilCount(this KMBombInfo bombInfo)
        {
            return GetTimeEntries(bombInfo).Select(x => !x.am && !x.pm).Count();
        }
        #endregion
    }
}