using System;
using System.Text.RegularExpressions;

namespace HRwflow.Models
{
    public class Customer
    {
        private CustomerProperties _properties = new();
        private string _username;

        public CustomerProperties Properties
        {
            get => _properties;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _properties = value;
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (!UsernameIsCorrect(value))
                {
                    throw new ArgumentException("Invalid argument.", nameof(value));
                }
                _username = value;
            }
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

        public static bool UsernameIsCorrect(string username) => username != null
                            && Regex.IsMatch(username, $"^[a-z0-9]{{6,20}}$");
    }

    public class CustomerProperties
    {
        public static readonly string NameCorrectSymbols = "abcdefghijklmnopqrstuvwxyz" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZабвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ -";

        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                if (!NameIsCorrect(value))
                {
                    throw new ArgumentException("Invalid argument.", nameof(value));
                }
                _name = value;
            }
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

        public static bool NameIsCorrect(string name) => name == FormatName(name)
            && name != null && Regex.IsMatch(name, $"^[{NameCorrectSymbols}]{{0,50}}$");
    }
}
