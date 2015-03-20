using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Equilobe.DailyReport.Utils
{
    public static class Validations
    {
        public const string TIME = @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$"; // 00:00 - 23:59
        public const string MAIL = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"; // single mail
        public const string MAILS = @"^((\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)\s*[;]{0,1}\s*)+$"; // multiple mail, delimited by , or ; or space
        public const string DIGITS = @"^[0-9]*$"; // 0-9*...
        public const string DAYS = @"^([1-9]|(1|2)[0-9]|3[0,1])?(\s([1-9]|(1|2)[0-9]|3[0,1]))*$"; // 1-31
        public const string URL = @"^(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$";

        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static bool Time(this string time)
        {
            if (string.IsNullOrEmpty(time))
                return true;

            Regex regexTime = new Regex(TIME);
            return regexTime.IsMatch(time);
        }

        public static bool Mail(this string mail)
        {
            if (string.IsNullOrEmpty(mail))
                return true;

            Regex regexMail = new Regex(MAIL);
            return regexMail.IsMatch(mail);
        }

        public static bool Mails(this string mails)
        {
            if (string.IsNullOrEmpty(mails))
                return true;

            Regex regexMails = new Regex(MAILS);
            return regexMails.IsMatch(mails);
        }

        public static bool Digits(this string digits)
        {
            if (string.IsNullOrEmpty(digits))
                return true;

            Regex regexDigits = new Regex(DIGITS);
            return regexDigits.IsMatch(digits);
        }

        public static bool Days(this string days)
        {
            if (string.IsNullOrEmpty(days))
                return true;

            Regex regexDays = new Regex(DAYS);
            return regexDays.IsMatch(days);
        }

        public static bool Url(this string url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            Regex regexUrl = new Regex(URL);
            return regexUrl.IsMatch(url);
        }
    }
}
