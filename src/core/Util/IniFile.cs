using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sdm.Core
{
    public enum IniError
    {
        StrToInt,
        StrToBool,
        KeyNotFound,
        SectionNotFound
    };

    public sealed class IniException : Exception
    {
        public IniError Error { get; private set; }

        public IniException(IniError error) { Error = error; }
    }

    public sealed class IniFile
    {
        private readonly string[] lines;
        private readonly SortedList<string, int> sections;
        
        public IniFile(string filename, Encoding encoding)
        {
            lines = File.ReadAllLines(filename, encoding);
            sections = new SortedList<string, int>(32);
            ScanSections();
        }

        private IniFile()
        {
            lines = new string[0];
            sections = new SortedList<string, int>(1);
        }

        static IniFile() { Empty = new IniFile(); }

        public static IniFile Empty { get; private set; }

        public static KeyValuePair<string, string> ExtractKeyValuePair(string line, bool trimComment)
        {
            string key = null;
            string value = "";
            var buf = line.Trim();
            if (trimComment)
            {
                var semicolon = buf.IndexOf(';');
                if (semicolon == 0)
                    return new KeyValuePair<string, string>(key, value);
                if (semicolon > 0)
                    buf = buf.Substring(0, semicolon).Trim();
            }
            var delim = buf.IndexOf('=');
            if (delim == -1)
            {
                key = buf;
                return new KeyValuePair<string, string>(key, value);
            }
            key = buf.Substring(0, delim).Trim();
            if (delim < buf.Length - 1)
                value = buf.Substring(delim + 1).Trim();
            return new KeyValuePair<string, string>(key, value);
        }

        public string GetString(string section, string key)
        {
            if (!sections.ContainsKey(section))
                throw new IniException(IniError.SectionNotFound);
            var value = "";
            var keyFound = false;
            for (var i = sections[section]; i < lines.Length; i++)
            {
                var pair = ExtractKeyValuePair(lines[i], true);
                if (pair.Key == key)
                {
                    value = pair.Value;
                    keyFound = true;
                    break;
                }
            }
            if (!keyFound)
                throw new IniException(IniError.KeyNotFound);
            return value;
        }

        public int GetInt32(string section, string key)
        {
            var buf = GetString(section, key);
            try
            {
                return Convert.ToInt32(buf);
            }
            catch
            {
                throw new IniException(IniError.StrToInt);
            }
        }

        private static bool StringToBool(string s)
        {
            s = s.ToLower();
            if (s == "1" || s == "true")
                return true;
            if (s == "0" || s == "false")
                return false;
            throw new IniException(IniError.StrToBool);
        }

        public bool GetBool(string section, string key)
        {
            var buf = GetString(section, key);
            buf = buf.ToLower();
            try
            {
                return StringToBool(buf);
            }
            catch
            {
                throw new IniException(IniError.StrToBool);
            }
        }

        public bool TryGetString(string section, string key, ref string result)
        {
            try
            {
                result = GetString(section, key);
                return true;
            }
            catch (IniException)
            {
                return false;
            }
        }

        public bool TryGetInt32(string section, string key, ref int result)
        {
            try
            {
                result = GetInt32(section, key);
                return true;
            }
            catch (IniException)
            {
                return false;
            }
        }

        public bool TryGetBool(string section, string key, ref bool result)
        {
            try
            {
                result = GetBool(section, key);
                return true;
            }
            catch (IniException)
            {
                return false;
            }
        }

        public bool ContainsSection(string section) { return sections.ContainsKey(section); }

        public int GetSectionCount(Predicate<string> match = null)
        {
            var sectionCount = sections.Count;
            if (match == null)
                return sectionCount;
            var result = 0;
            for (int i = 0; i < sectionCount; i++)
            {
                if (match(sections.Keys[i]))
                    result++;
            }
            return result;
        }

        private void ScanSections()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var len = lines[i].Length;
                if (len < 3)
                    continue;
                if (lines[i][0] == ';')
                    continue;
                var sect1 = lines[i].IndexOf('[');
                var sect2 = lines[i].IndexOf(']');
                if (sect2 - sect1 < 2)
                    continue;
                if (sect1 < sect2 && sect1 >= 0 && sect2 >= 0)
                {
                    var buf = lines[i].Substring(sect1 + 1, sect2 - sect1 - 1);
                    if (!sections.ContainsKey(buf))
                        sections.Add(buf, i);
                }
            }
        }
    }
}
