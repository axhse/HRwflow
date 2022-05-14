using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HRwflow.Models
{
    public enum VacancyState
    {
        Active,
        Closed,
        Cancelled,
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

        public VacancyState State { get; set; }

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

        public static bool TitleIsCorrect(string title)
            => title is null || title.Length <= 100;

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
            if (!DescriptionIsCorrect(description))
            {
                return false;
            }
            _description = description;
            return true;
        }

        public bool TrySetTitle(string title)
        {
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
        public Dictionary<string, string> Notes = new();
        public DateTime CreationTime { get; } = DateTime.UtcNow;
        public int OwnerTeamId { get; set; }
        public VacancyProperties Properties { get; set; } = new();
        public int VacancyId { get; set; }
    }
}
