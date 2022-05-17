using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HRwflow.Models
{
    public enum VacancyStates
    {
        Active,
        Completed,
    }

    public struct VacancyProperties
    {
        public static readonly string TagCorrectSymbols = "abcdefghijklmnopqrstuvwxyz" +
            "абвгдеёжзийклмнопрстуфхцчшщъыьэюя0123456789";

        private string _description;
        private HashSet<string> _tags;
        private string _title;

        public HashSet<string> AllTags
        {
            get => _tags is null ? new() : new(_tags);
            set
            {
                if (value is not null)
                {
                    _tags = new(value.Where(tag => TagIsCorrect(tag)));
                }
            }
        }

        public string Description
        {
            get => _description is null ? string.Empty : _description;
            set => TrySetDescription(value);
        }

        public VacancyStates State { get; set; }

        public string Title
        {
            get => _title is null ? string.Empty : _title;
            set => TrySetTitle(value);
        }

        public static bool DescriptionIsCorrect(string description)
            => description is null || description.Length <= 1000;

        public static string FormatTag(string tag)
        {
            if (tag is null)
            {
                return null;
            }
            return string.Join(string.Empty,
                tag.ToLower().Where(s => TagCorrectSymbols.Contains(s)));
        }

        public static bool TagIsCorrect(string tag) => tag is not null
            && Regex.IsMatch(tag, $"^[{TagCorrectSymbols}]{{2,20}}$");

        public static bool TitleIsCorrect(string title) => title is null
            || (1 <= title.Length && title.Length <= 100);

        public void RemoveTag(string tag)
        {
            if (tag is not null)
            {
                _tags.Remove(tag);
            }
        }

        public bool TryAddTag(string tag)
        {
            tag = FormatTag(tag);
            if (TagIsCorrect(tag))
            {
                _tags.Add(tag);
                return true;
            }
            return false;
        }

        public bool TrySetDescription(string description)
        {
            if (description is not null)
            {
                description = description.Trim();
            }
            if (!DescriptionIsCorrect(description))
            {
                return false;
            }
            _description = description;
            return true;
        }

        public bool TrySetTitle(string title)
        {
            if (title is not null)
            {
                title = title.Trim();
            }
            if (!TitleIsCorrect(title))
            {
                return false;
            }
            _title = title;
            return true;
        }
    }

    public class Vacancy
    {
        public Dictionary<string, VacancyNote> Notes = new();
        public DateTime CreationTime { get; } = DateTime.UtcNow;
        public DateTime LastNoteTime { get; set; } = DateTime.UtcNow;
        public int OwnerTeamId { get; set; }
        public VacancyProperties Properties { get; set; } = new();
        public int VacancyId { get; set; }
    }

    public class VacancyNote
    {
        private string _text;
        public DateTime LastChangeTime { get; private set; } = DateTime.UtcNow;

        public string Text
        {
            get => _text is null ? string.Empty : _text;
            set => TrySetText(value);
        }

        public static bool TextIsCorrect(string text)
            => text is null || text.Length <= 1000;

        public bool TrySetText(string text)
        {
            if (text is not null)
            {
                text = text.Trim();
            }
            if (!TextIsCorrect(text))
            {
                return false;
            }
            _text = text;
            LastChangeTime = DateTime.UtcNow;
            return true;
        }
    }
}
