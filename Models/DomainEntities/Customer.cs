using System.Text.RegularExpressions;

namespace HRwflow.Models
{
    public struct CustomerProperties
    {
        public static readonly string NameCorrectSymbols = "abcdefghijklmnopqrstuvwxyz" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZабвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ -";

        private string _name;

        public string Name
        {
            get => _name is null ? string.Empty : _name;
            set => TrySetName(value);
        }

        public static string FormatName(string name)
        {
            if (name is not null)
            {
                name = name.Trim(' ', '-');
                foreach (var item in new string[] { "- ", " -" })
                {
                    while (name.Contains(item))
                    {
                        name = name.Replace(item, "-");
                    }
                }
                foreach (var symb in " -")
                {
                    while (name.Contains(symb.ToString() + symb))
                    {
                        name = name.Replace(symb.ToString() + symb, symb.ToString());
                    }
                }
            }
            return name;
        }

        public static bool NameIsCorrect(string name) => name is null
            || Regex.IsMatch(name, $"^[{NameCorrectSymbols}]{{0,50}}$");

        public bool TrySetName(string name)
        {
            name = FormatName(name);
            if (!NameIsCorrect(name))
            {
                return false;
            }
            _name = name;
            return true;
        }
    }

    public class Customer
    {
        private string _username;

        public CustomerProperties Properties { get; set; } = new();

        public string Username
        {
            get => _username;
            set => TrySetUsername(value);
        }

        public static string FormatUsername(string username)
        {
            if (username is not null)
            {
                username = username.Trim(' ').ToLower();
            }
            return username;
        }

        public static bool PasswordIsCorrect(string password) => password != null
                && 8 <= password.Length && password.Length <= 40;

        public static bool UsernameIsCorrect(string username)
            => username != null && Regex.IsMatch(username, $"^[a-z0-9]{{6,20}}$");

        public bool TrySetUsername(string username)
        {
            username = FormatUsername(username);
            if (!UsernameIsCorrect(username))
            {
                return false;
            }
            _username = username;
            return true;
        }
    }
}
