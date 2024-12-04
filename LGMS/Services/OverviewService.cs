﻿using LGMS.Data.Model;
using System.Globalization;

namespace LGMS.Services
{
    public class OverviewService
    {
        public List<string> GetTimeLabels(string range)
        {
            List<string> labels = new List<string>();

            switch (range)
            {
                case "1 Month":
                    for (int i = 1; i <= 4; i++)
                    {
                        labels.Add($"Week {i}");
                    }
                    break;
                case "3 Months":
                    labels.AddRange(new[] { "Jan", "Feb", "Mar" });
                    break;
                case "6 Months":
                    labels.AddRange(new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" });
                    break;
                case "1 Year":
                    labels.AddRange(new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" });
                    break;
                case "2 Years":
                    labels.AddRange(new[] { "Year 1", "Year 2" });
                    break;
                default:
                    throw new ArgumentException("Invalid range");
            }

            return labels;
        }


        public List<object> GetContractCountsByTimePeriod(List<Contract> contracts, string range)
        {
            List<object> result = new List<object>();

            var groupedByType = contracts.GroupBy(c => c.Type).ToList();

            foreach (var typeGroup in groupedByType)
            {
                var counts = new List<int>();

                foreach (var label in GetTimeLabels(range))
                {
                    int count = GetContractCountForTimePeriod(typeGroup, label, range);
                    counts.Add(count);
                }

                result.Add(new
                {
                    title = typeGroup.Key.Title,
                    counts = counts
                });
            }

            return result;
        }

        public int GetContractCountForTimePeriod(IGrouping<ContractType, Contract> typeGroup, string label, string range)
        {
            int count = 0;
            DateTime startPeriod;
            DateTime endPeriod;

            switch (range)
            {
                case "1 Month":
                    startPeriod = DateTime.Now.AddDays(-7 * (GetWeekNumber(label) - 1));
                    endPeriod = startPeriod.AddDays(7);
                    break;
                case "3 Months":
                    startPeriod = DateTime.Now.AddMonths(-3).AddMonths(GetMonthNumber(label) - 1);
                    endPeriod = startPeriod.AddMonths(1);
                    break;
                case "6 Months":
                    startPeriod = DateTime.Now.AddMonths(-6).AddMonths(GetMonthNumber(label) - 1);
                    endPeriod = startPeriod.AddMonths(1);
                    break;
                case "1 Year":
                    int monthIndex = DateTime.ParseExact(label, "MMM", CultureInfo.InvariantCulture).Month;
                    startPeriod = DateTime.Now.AddMonths(-(12 - monthIndex));
                    endPeriod = startPeriod.AddMonths(1);
                    break;
                case "2 Years":
                    startPeriod = DateTime.Now.AddYears(-2).AddYears(GetYearNumber(label) - 1);
                    endPeriod = startPeriod.AddYears(1);
                    break;
                default:
                    return 0;
            }

            count = typeGroup.Count(c => c.CompletionDate >= startPeriod && c.CompletionDate < endPeriod);
            return count;
        }


        public int GetWeekNumber(string weekLabel)
        {
            return int.Parse(weekLabel.Split(' ')[1]);
        }

        public int GetMonthNumber(string monthLabel)
        {
            return DateTime.ParseExact(monthLabel, "MMM", CultureInfo.InvariantCulture).Month;
        }


        public int GetQuarterNumber(string quarterLabel)
        {
            return int.Parse(quarterLabel.Substring(1));
        }

        public int GetYearNumber(string yearLabel)
        {
            return int.Parse(yearLabel.Split(' ')[1]);
        }
    }
}
