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
        private string _description;
        private string _title;

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

        public static bool TitleIsCorrect(string title)
            => title is null || title.Length <= 100;

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
        public int Id { get; set; }
        public VacancyProperties Properties { get; set; } = new();
    }
}
