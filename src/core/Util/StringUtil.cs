using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdm.Core.Util
{
    public static class StringUtil
    {
        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            var inQuotes = false;
            var quoted = commandLine.Split((prev, c) =>
            {
                if (c == '\"' && prev != '\\')
                    inQuotes = !inQuotes;
                return !inQuotes && c == ' ';
            });
            return quoted.Select(arg => EscapeCmdString(arg.Trim(), false))
                .Where(arg => !String.IsNullOrEmpty(arg));
        }

        public static IEnumerable<string> Split(this string str, Func<char, char, bool> controller)
        {
            int nextPiece = 0;
            char prevChar = '\0';
            for (int c = 0; c < str.Length; c++)
            {
                if (controller(prevChar, str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
                prevChar = str[c];
            }
            yield return str.Substring(nextPiece);
        }

        public static string EscapeCmdString(string s, bool escape)
        {
            StringBuilder sb;
            if (escape)
            {
                var hasSpaces = s.IndexOf(' ') >= 0;
                sb = new StringBuilder(2 * s.Length);
                if (hasSpaces)
                    sb.Append(' ');
                sb.Append(s);
                sb.Replace("\"", "\\\""); // " -> \"
                if (hasSpaces)
                {
                    sb[0] = '"';
                    sb.Append('"');
                }
            }
            else
            {
                sb = new StringBuilder(s.Length);
                var trim = s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"';
                if (trim)
                    sb.Append(s, 1, s.Length - 2);
                else
                    sb.Append(s);
                sb.Replace("\\\"", "\""); // \" -> "
            }
            return sb.ToString();
        }
    }
}
