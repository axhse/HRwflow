using System.Collections.Generic;

namespace HRwflow.Models
{
    public struct TeamProperties
    {
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
                name = name.Trim();
            }
            foreach (var symb in " ")
            {
                while (name.Contains(symb.ToString() + symb))
                {
                    name = name.Replace(symb.ToString() + symb, symb.ToString());
                }
            }
            return name;
        }

        public static bool NameIsCorrect(string name)
            => name is not null && 2 <= name.Length && name.Length <= 40;

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

    public class Team
    {
        public Dictionary<string, TeamPermissions> Permissions { get; set; } = new();
        public TeamProperties Properties { get; set; } = new();
        public int TeamId { get; set; }

        public bool HasMember(string username)
            => Permissions.ContainsKey(username);
    }
}
