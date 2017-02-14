using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Fang
{
    public class RegexHelper
    {
        static char[] REGEX_KEYWORD = new char[] { '(', ')', '.', '*', '+', '+', '?', '^', '$', '[', ']', '{', '}', '/', '\\' };

        public static List<string> GetKeywords(string keyword)
        {
            keyword = Escape(keyword);
            List<string> keywords = null;
            if (!string.IsNullOrEmpty(keyword))
            {
                Regex regex = new Regex("\\s+");
                keyword = regex.Replace(keyword, " ");
                keywords = keyword.Split(' ').ToList();
            }
            return keywords;
        }

        public static string EscapeForSql(string patten)
        {
            return patten
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }

        public static string Escape(string patten)
        {
            return Escape(patten, REGEX_KEYWORD);
        }

        public static string Escape(string patten, char[] chars)
        {
            if(string.IsNullOrEmpty(patten))
            {
                return patten;
            }

            StringBuilder pattenBuilder = new StringBuilder();
            foreach(char pattenChar in patten)
            {
                if(chars.Contains(pattenChar))
                {
                    pattenBuilder.Append("\\" + pattenChar);
                }
                else
                {
                    pattenBuilder.Append(pattenChar);
                }
            }

            return pattenBuilder.ToString();
        }

        public static bool MatchInt(string input, string patten, out int matchValue)
        {
            matchValue = 0;
            Regex idWhereRegex = new Regex(patten, RegexOptions.IgnoreCase);
            if (idWhereRegex.IsMatch(input))
            {
                Match match = idWhereRegex.Match(input);
                return int.TryParse(match.Groups[1].Value, out matchValue);
            }
            return false;
        }

        public static bool MatchSqlWhereId(string input, out int matchValue)
        {
            if (RegexHelper.MatchInt(input, "[\\s]*ID[\\s]*=[\\s]*[']?([^']*)[']?[\\s]*", out matchValue))
            {
                return true;
            }
            else if (RegexHelper.MatchInt(input, "[\\s]*[']?([^']*)[']?[\\s]*", out matchValue))
            {
                return true;
            }

            return false;
        }

        public static bool IsMatchString(string input, string patten)
        {
            Regex regex = new Regex(patten, RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }

        public static bool MatchString(string input, string patten, out string matchValue)
        {
            matchValue = "";
            Regex idWhereRegex = new Regex(patten, RegexOptions.IgnoreCase);
            if (idWhereRegex.IsMatch(input))
            {
                Match match = idWhereRegex.Match(input);
                matchValue = match.Groups[1].Value;
                return true;
            }
            return false;
        }

        public static bool MatchSqlValue(string input, string filedName, out string matchValue)
        {
            string patten = string.Format("[\\s]*{0}[\\s]*=[\\s]*'(.*)'[\\s]*", filedName);
            matchValue = "";
            Regex idWhereRegex = new Regex(patten, RegexOptions.IgnoreCase);
            if (idWhereRegex.IsMatch(input))
            {
                Match match = idWhereRegex.Match(input);
                matchValue = match.Groups[1].Value;
                return true;
            }
            return false;
        }

        public static bool IsMatchSqlValue(string input, string filedName)
        {
            string patten = string.Format("[\\s]*{0}[\\s]*=[\\s]*'(.*)'[\\s]*", filedName);
            
            Regex regex = new Regex(patten, RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }
    }
}
