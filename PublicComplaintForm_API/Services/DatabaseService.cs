using PublicComplaintForm_API.Models;
using Microsoft.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.SqlServer.Server;
using log4net;

namespace PublicComplaintForm_API.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString = string.Empty;
        private readonly string _surveyConnectionString = string.Empty;

        private readonly ILog _logger;

        public DatabaseService(ILog logger)
        {
            _logger = logger;
        }

        public DatabaseService(string connectionString, string surveyConnectionString, ILog logger)
        {
            _connectionString = connectionString ?? string.Empty;
            _surveyConnectionString = surveyConnectionString ?? string.Empty;
            _logger = logger;
        }

        public async Task<Guid> GetCourtId(string courtName)
        {
            return Guid.Empty;
        }

        public async Task<Guid> GetCityId(string cityName)
        {
            return Guid.Empty;
        }

        public async Task<Guid> DoesContactExist(string IdNumber)
        {
            return Guid.Empty;
        }

        public async Task<List<Court>> FetchCourtList()
        {
            // TODO: Remove this once the DB is ready
            return new List<Court>
            {
                new Court { CourtId = Guid.NewGuid(), CourtName = "בית משפט א" },
                new Court { CourtId = Guid.NewGuid(), CourtName = "בית משפט ב" },
                new Court { CourtId = Guid.NewGuid(), CourtName = "בית משפט ג" },
                new Court { CourtId = Guid.NewGuid(), CourtName = "בית משפט ד" },
                new Court { CourtId = Guid.NewGuid(), CourtName = "בית משפט ה" },
                new Court { CourtId = Guid.NewGuid(), CourtName = "בית משפט ו" },
                new Court { CourtId = Guid.NewGuid(), CourtName = "בית משפט ז" },
                new Court { CourtId = Guid.NewGuid(), CourtName = "בית משפט ח" },
                new Court { CourtId = Guid.NewGuid(), CourtName = "בית משפט ט" }
            };
        }

        public async Task<Guid> InsertContact(PublicComplaintData formData)
        {
            return Guid.Empty;
        }

        public async Task<bool> InsertComplaint(
            PublicComplaintData formData,
            Guid contactId,
            Guid inquiryId,
            bool receivedFiles)
        {
            return false;
        }

        public async Task<bool> CanSubmitSurvey(SurveyData surveyData)
        {
            return false;
        }

        public async Task SubmitSurvey(SurveyData surveyData)
        {
            return;
        }

        public async Task SubmitForm(
            PublicComplaintData formData,
            List<string> files,
            Guid inquiryId,
            bool receivedFiles)
        {
            return;
        }

        public async Task<List<MonthlyReferralReport>> GetMonthlyReferralReport(int month, int year)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var sql = @"
                        WITH monthly_data AS (
                            SELECT 
                                department_id,
                                YEAR(referral_date) AS year,
                                MONTH(referral_date) AS month,
                                COUNT(*) AS total
                            FROM referrals
                            WHERE referral_date >= DATEFROMPARTS(@Year - 1, @Month, 1)
                            GROUP BY department_id, YEAR(referral_date), MONTH(referral_date)
                        )
                        SELECT 
                            d.department_name AS DepartmentName,
                            ISNULL(curr.total, 0) AS CurrentMonthTotal,
                            ISNULL(prev.total, 0) AS PreviousMonthTotal,
                            ISNULL(last_year.total, 0) AS SameMonthLastYearTotal,
                            CASE 
                                WHEN prev.total IS NULL OR prev.total = 0 THEN NULL
                                ELSE ROUND(((curr.total - prev.total) * 100.0 / prev.total), 2)
                            END AS PercentChangeFromPrevMonth,
                            CASE 
                                WHEN last_year.total IS NULL OR last_year.total = 0 THEN NULL
                                ELSE ROUND(((curr.total - last_year.total) * 100.0 / last_year.total), 2)
                            END AS PercentChangeFromLastYear
                        FROM departments d
                        LEFT JOIN monthly_data curr 
                            ON d.id = curr.department_id 
                            AND curr.year = @Year 
                            AND curr.month = @Month
                        LEFT JOIN monthly_data prev 
                            ON d.id = prev.department_id 
                            AND prev.year = CASE WHEN @Month = 1 THEN @Year - 1 ELSE @Year END
                            AND prev.month = CASE WHEN @Month = 1 THEN 12 ELSE @Month - 1 END
                        LEFT JOIN monthly_data last_year 
                            ON d.id = last_year.department_id 
                            AND last_year.year = @Year - 1 
                            AND last_year.month = @Month
                        ORDER BY d.department_name";

                    var parameters = new { Month = month, Year = year };
                    var results = await connection.QueryAsync<MonthlyReferralReport>(sql, parameters);

                    _logger.Info($"Successfully retrieved monthly referral report for {month}/{year}. Total departments: {results.Count()}");

                    return results.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error retrieving monthly referral report: {ex.Message}", ex);
                throw;
            }
        }
    }
}
