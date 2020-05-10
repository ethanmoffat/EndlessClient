using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EndlessClient.Old
{
    /// <summary>
    /// Helper static class that provides an interface for dealing with the friend/ignore INI files
    /// </summary>
    public static class InteractList
    {
        private const string FILE_FRIENDS = "config\\friends.ini";
        private const string FILE_IGNORE  = "config\\ignore.ini";


        public static List<string> LoadAllFriend()
        {
            return _loadList(FILE_FRIENDS);
        }

        public static List<string> LoadAllIgnore()
        {
            return _loadList(FILE_IGNORE);
        }

        public static void WriteNewFriend(string newFriend)
        {
            List<string> lines = _loadList(FILE_FRIENDS);
            if (lines.Find(x => x.ToLower() == newFriend.ToLower()) != null)
                return;

            lines.Add(newFriend);
            lines.Sort();
            WriteFriendList(lines);
        }

        public static void WriteNewIgnore(string newIgnore)
        {
            List<string> lines = _loadList(FILE_IGNORE);
            if (lines.Find(x => x.ToLower().Equals(newIgnore.ToLower())) != null)
                return;

            lines.Add(newIgnore);
            lines.Sort();
            WriteIgnoreList(lines);
        }

        public static void WriteFriendList(IEnumerable<string> lines)
        {
            _writeList(FILE_FRIENDS, lines);
        }

        public static void WriteIgnoreList(IEnumerable<string> lines)
        {
            _writeList(FILE_IGNORE, lines);
        }

        private static List<string> _loadList(string fileName)
        {
            List<string> allLines;
            try
            {
                allLines = new List<string>(File.ReadAllLines(fileName));
            }
            catch (Exception)
            {
                allLines = new List<string>();
            }

            //each first letter is capitalized
            allLines.RemoveAll(s => s.StartsWith("#") || string.IsNullOrWhiteSpace(s));
            allLines = allLines.Select(s => char.ToUpper(s[0]) + s.Substring(1).ToLower()).Distinct().ToList();

            return allLines;
        }

        private static void _writeList(string fileName, IEnumerable<string> lines)
        {
            bool isIgnoreList = fileName == FILE_IGNORE;
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                //this is not localized because the original client has it in english in the files
                string friendOrIgnore = isIgnoreList ? "ignore" : "friend";
                //header
                sw.WriteLine("# Endless Online 0.28 [ {0} list ]", friendOrIgnore);
                sw.WriteLine();
                sw.WriteLine("# List of {0}{1} characters, use a new line for each name\n", friendOrIgnore, isIgnoreList ? "d" : "");
                sw.WriteLine();

                foreach (string s in lines)
                {
                    sw.WriteLine(char.ToUpper(s[0]) + s.Substring(1).ToLower());
                }
            }
        }
    }
}
