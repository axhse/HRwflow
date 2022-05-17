using System;
using System.Collections.Generic;
using System.Linq;

namespace HRwflow.Models
{
    public enum TimeSpans : long
    {
        Hour = 3600,
        Day = Hour * 24,
        Week = Day * 7,
        Month = Day * 30,
        Year = Day * 365,
        Max = Year * 10000
    }

    public enum VacancySortingRules
    {
        RecentCreated,
        OldestCreated,
        RecentNoted,
        OldestNoted
    }

    public class VacancyListVM
    {
        public IEnumerable<Vacancy> AllVacancies { get; set; }
        public TimeSpans CreationTimeOffset { get; set; } = TimeSpans.Max;
        public TimeSpans LastNoteTimeOffset { get; set; } = TimeSpans.Max;

        public IEnumerable<Vacancy> SelectedVacancies
        {
            get
            {
                bool selector(Vacancy vacancy)
                    => (DateTime.UtcNow - vacancy.CreationTime).TotalSeconds
                    < (long)CreationTimeOffset
                    && (DateTime.UtcNow - vacancy.LastNoteTime).TotalSeconds
                    < (long)LastNoteTimeOffset
                    && VacancyStates.Contains(vacancy.Properties.State);
                return from vacancy in AllVacancies
                       where selector(vacancy)
                       select vacancy;
            }
        }

        public VacancySortingRules SortingRule { get; set; } = VacancySortingRules.RecentNoted;

        public int TeamId { get; set; }
        public string TeamName { get; set; }

        public HashSet<VacancyStates> VacancyStates { get; set; }
                                    = Enum.GetValues<VacancyStates>().ToHashSet();
    }
}
