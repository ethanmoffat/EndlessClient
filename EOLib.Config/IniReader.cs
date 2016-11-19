// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EOLib.Config
{
    public class IniReader
    {
        private readonly SortedList<string, SortedList<string, string>> _sections;
        private readonly string _filename;

        public IniReader(string filename)
        {
            _filename = filename;
            _sections = new SortedList<string, SortedList<string, string>>();
        }

        #region Public Interface

        public bool Load()
        {
            try
            {
                using (var str = new StreamReader(_filename))
                {
                    var currentHeader = "";
                    while (!str.EndOfStream)
                    {
                        var nextLine = str.ReadLine();

                        if (EmptyOrCommentLine(nextLine))
                            continue;
                        nextLine = RemoveCommentsFromLine(nextLine);

                        if (IsSectionHeader(nextLine))
                        {
                            currentHeader = AddNewHeader(nextLine);
                            continue;
                        }
                        
                        //get the pair of key/value
                        string[] pair = nextLine.Split('=');
                        if (pair.Length != 2)
                            continue;

                        _sections[currentHeader][pair[0]] = pair[1];
                    }
                }
            }
            catch
            {
                if (!Directory.Exists(Directory.GetParent(_filename).FullName))
                {
                    Directory.CreateDirectory(Directory.GetParent(_filename).FullName);
                }
                if (!File.Exists(_filename))
                {
                    File.Create(_filename).Close();
                }
                return false;
            }

            return true;
        }

        public bool GetValue(string section, string key, out string value)
        {
            value = null;
            if (!_sections.ContainsKey(section))
                return false;
            if (!_sections[section].ContainsKey(key))
                return false;

            value = _sections[section][key];
            return true;
        }

        public bool GetValue(string section, string key, out int value)
        {
            value = int.MaxValue;
            if (!_sections.ContainsKey(section))
                return false;
            if (!_sections[section].ContainsKey(key))
                return false;

            try
            {
                value = Convert.ToInt32(_sections[section][key]);
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        public bool GetValue(string section, string key, out bool value)
        {
            value = false;
            if (!_sections.ContainsKey(section))
                return false;
            if (!_sections[section].ContainsKey(key))
                return false;

            //convert possible strings into true/false values that can be parsed
            var toConvert = _sections[section][key];
            switch (toConvert.ToLower())
            {
                case "yes":
                case "1":
                case "on":
                    toConvert = "true";
                    break;
                case "no":
                case "0":
                case "off":
                    toConvert = "false";
                    break;
            }

            try
            {
                value = Convert.ToBoolean(toConvert);
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Private Helpers

        private bool EmptyOrCommentLine(string line)
        {
            return string.IsNullOrEmpty(line) || line[0] == '\'' || line[0] == '#';
        }

        private string RemoveCommentsFromLine(string line)
        {
            var comment = line.Contains('#') ? '#' : line.Contains('\'') ? '\'' : char.MinValue;
            if (comment > char.MinValue)
                line = line.Remove(line.IndexOf(comment));
            return line.Trim();
        }

        private bool IsSectionHeader(string input)
        {
            return input[0] == '[' && input[input.Length - 1] == ']';
        }

        private string AddNewHeader(string nextLine)
        {
            var header = nextLine.Remove(0, 1);
            header = header.Remove(header.Length - 1);
            _sections.Add(header, new SortedList<string, string>());
            return header;
        }

        #endregion
    }
}
