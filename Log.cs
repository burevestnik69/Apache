using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Apache
{
    class Log
    {
        public Log(Dictionary<string, string> data)
        {
            Data = data;
        }

        public Dictionary<string, string> Data { get; }

        public static Log Parse(string logLine, string format)
        {
            var splits = SplitText(logLine);
            var formats = format.Split(' ').ToList();

            return new Log(GetData(splits, formats));
        }

        static List<string> SplitText(string input)
        {
            return Regex.Matches(input, @"(?:^|\s)(?:""[^""]*""|\[[^\]]*\]|[^\s]+)(?=\s|$)")
                        .Cast<Match>()
                        .Select(m => m.Value.Trim())
                        .ToList();
        }

        static Dictionary<string, string> GetData(List<string> splits, List<string> formats)
        {
            if (splits.Count != formats.Count)
                throw new Exception("Логи не соответствуют формату!");

            var data = new Dictionary<string, string>();

            for (int i = 0; i < formats.Count; i++)
            {
                switch (formats[i])
                {
                    case "%t" when splits[i].StartsWith("["):
                        string dateTimeStr = splits[i].Trim('[', ']');
                        string format = "dd/MMM/yyyy:HH:mm:ss zzz";
                        if (DateTime.TryParseExact(dateTimeStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                        {
                            data.Add(formats[i], dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else
                        {
                            throw new Exception("Ошибка с получением даты в логах!");
                        }
                        break;
                    case "%>s":
                        if (int.TryParse(splits[i], out int status))
                        {
                            data.Add(formats[i], splits[i]);
                        }
                        else
                        {
                            throw new Exception("Ошибка с получением статуса в логах!");
                        }
                        break;
                    case "\\\"%r\\\"" when splits[i].StartsWith("\""):
                        data.Add(formats[i].Replace("\\\"", ""), splits[i].Trim('\"'));
                        break;

                    default:
                        data.Add(formats[i], splits[i]);
                        break;
                }
            }
            return data;
        }
    }
}
