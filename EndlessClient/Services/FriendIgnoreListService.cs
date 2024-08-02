using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutomaticTypeMapper;

namespace EndlessClient.Services
{
    [AutoMappedType]
    public class FriendIgnoreListService : IFriendIgnoreListService
    {
        public IReadOnlyList<string> LoadList(string path)
        {
            return Load(path);
        }

        public void SaveFriends(string path, IReadOnlyList<string> contents)
        {
            Save(isIgnore: false, path, contents);
        }

        public void SaveIgnored(string path, IReadOnlyList<string> contents)
        {
            Save(isIgnore: true, path, contents);
        }

        public void SaveNewFriend(string path, string name)
        {
            if (!File.Exists(path) || string.IsNullOrWhiteSpace(File.ReadAllText(path)))
                Save(isIgnore: false, path, new[] { name });
            else
                File.AppendAllLines(path, new[] { name });
        }

        public void SaveNewIgnore(string path, string name)
        {
            if (!File.Exists(path) || string.IsNullOrWhiteSpace(File.ReadAllText(path)))
                Save(isIgnore: true, path, new[] { name });
            else
                File.AppendAllLines(path, new[] { name });
        }

        private static List<string> Load(string fileName)
        {
            if (!File.Exists(fileName))
                return new List<string>();

            List<string> allLines;
            try
            {
                allLines = new List<string>(File.ReadAllLines(fileName));
            }
            catch (IOException)
            {
                return new List<string>();
            }

            allLines.RemoveAll(s => s.StartsWith("#") || string.IsNullOrWhiteSpace(s));

            return allLines.Select(Capitalize).Distinct().ToList();
        }

        private static void Save(bool isIgnore, string fileName, IEnumerable<string> lines)
        {
            using (var sw = new StreamWriter(fileName))
            {
                string friendOrIgnore = isIgnore ? "ignore" : "friend";
                sw.WriteLine($"# Endless Online 0.28 [ {friendOrIgnore} list ]\n");
                sw.WriteLine($"# List of {friendOrIgnore}{(isIgnore ? "d" : "")} characters, use a new line for each name\n\n");

                foreach (string s in lines)
                    sw.WriteLine(Capitalize(s));
            }
        }

        private static string Capitalize(string input) => char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }

    public interface IFriendIgnoreListService
    {
        IReadOnlyList<string> LoadList(string path);

        void SaveFriends(string path, IReadOnlyList<string> contents);

        void SaveIgnored(string path, IReadOnlyList<string> contents);

        void SaveNewFriend(string path, string name);

        void SaveNewIgnore(string path, string name);
    }
}