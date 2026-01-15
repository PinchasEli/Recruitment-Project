namespace PublicComplaintForm_API.Models
{
    public class MonthlyReferralReport
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int CurrentMonthTotal { get; set; }
        public int PreviousMonthTotal { get; set; }
        public int SameMonthLastYearTotal { get; set; }
        public decimal? PercentChangeFromPrevMonth { get; set; }
        public decimal? PercentChangeFromLastYear { get; set; }
    }
}
