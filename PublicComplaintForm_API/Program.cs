using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using PublicComplaintForm_API.Models;
using PublicComplaintForm_API.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.Json;

string baseDir = AppContext.BaseDirectory;

string logDir = Path.Combine(baseDir, "logs");

if (!Directory.Exists(logDir))
{
    Directory.CreateDirectory(logDir);
}

string logPath = Path.Combine(baseDir, "logs", "app.log");

var builder = WebApplication.CreateBuilder(args);

var serverIdentity = Environment.GetEnvironmentVariable("ServerIdentity")
                     ?? string.Empty;

var envConfig = new ConfigSettings();
try
{
    builder.Configuration.GetSection(serverIdentity).Bind(envConfig);
}
catch(Exception ex)
{
    envConfig = new ConfigSettings();
    envConfig.SaveFileFolder = "DEFAULT VALUE";
    envConfig.LocalSQL = "DEFAULT VALUE";
    envConfig.SurveySQLConnectionString = "DEFAULT VALUE";
}

if (string.IsNullOrEmpty(envConfig.SaveFileFolder))
{
    envConfig.SaveFileFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
    Console.WriteLine($"WARNING: SaveFileFolder not configured. Using default: {envConfig.SaveFileFolder}");
}
if (string.IsNullOrEmpty(envConfig.LocalSQL))
{
    envConfig.LocalSQL = "Data Source=localhost;Initial Catalog=YourDB;Integrated Security=True;";
    Console.WriteLine("WARNING: LocalSQL not configured. Using default connection string.");
}
if (string.IsNullOrEmpty(envConfig.SurveySQLConnectionString))
{
    envConfig.SurveySQLConnectionString = "Data Source=localhost;Initial Catalog=SurveyDB;Integrated Security=True;";
    Console.WriteLine("WARNING: SurveySQLConnectionString not configured. Using default connection string.");
}

var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));

var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".pdf", ".doc", ".docx", ".png", ".jpeg", ".jpg", ".gif", ".ogg", ".mp4", ".mp3", ".msg"
};

var dbService = new DatabaseService(envConfig.LocalSQL, envConfig.SurveySQLConnectionString, LogManager.GetLogger(typeof(Program)));

builder.Services.AddSingleton(LogManager.GetLogger(typeof(Program)));
builder.Services.AddMemoryCache();
builder.Services.AddSingleton(dbService);
builder.Services.AddSingleton<CaptchaService>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";

            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            var logger = context.RequestServices.GetService<ILog>();
            logger?.Info("Unhandled exception: " + exception?.Message);

            await context.Response.WriteAsJsonAsync(new
            {
                error = "An error has occurred processing your request." + exception?.Message,
                requestId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier
            });
        });
    });
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Referrer-Policy", "same-origin");

    // Older browsers use X-Frame-Options header
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // Newer browsers use Content-Security-Policy header
    context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors 'none'");
    await next();
});

app.MapGet("/", async ([FromServices] ILog log) =>
{
    log.Info("Someone accessed root endpoint. (Endpoint: /)");
    return Results.Json("API is running...");
});

app.MapGet("/courts", async ([FromServices] ILog log,
                            [FromServices] DatabaseService db) =>
{
    //await logger.LogAsync("Someone accessed /courts endpoint. (Endpoint: /courts)");
    log.Info("Someone accessed /courts endpoint.");
    List<Court> courtsList = await db.FetchCourtList();

    return Results.Ok(new { courtsList });
});

app.MapGet("/captcha", async ([FromServices] CaptchaService cs,
                                [FromServices] ILog log,
                                IMemoryCache cache) =>
{
    //await logger.LogAsync("Someone accessed /captcha endpoint.");
    log.Info("Someone accessed /captcha endpoint.");

    var captcha = cs.GenerateCaptcha();

    var sessionId = Guid.NewGuid().ToString();

    cache.Set(sessionId, captcha.Code, TimeSpan.FromHours(1));

    using (var ms = new MemoryStream())
    {
        await captcha.Image!.SaveAsync(ms, PngFormat.Instance);

        var imageBytes = ms.ToArray();

        //await logger.LogAsync("Generated captcha image. (Session ID: " + sessionId + ")");
        log.Info("Generated captcha image. (Session ID: " + sessionId + ")");

        return Results.Ok(new { sessionId, captchaImage = Convert.ToBase64String(imageBytes) });
    }
});

app.MapPost("/survey", async([FromServices] IAntiforgery antiforgery,
                             [FromBody] SurveyData surveyData,
                             [FromServices] DatabaseService db) =>
{
    var result = await db.CanSubmitSurvey(surveyData);

    if(!result)
    {
        return Results.Ok("This survey has already been submitted.");
    }

    await db.SubmitSurvey(surveyData);

    return Results.Ok(surveyData);
}).DisableAntiforgery();

app.MapPost("/submit-form", async ([FromServices] IAntiforgery antiforgery,
                                    [FromForm] IFormCollection formData,
                                    [FromForm] IFormFileCollection files,
                                    [FromServices] ILog log,
                                    [FromServices] DatabaseService db,
                                    [FromServices] CaptchaService cs,
                                    IMemoryCache cache,
                                    HttpContext context) =>
{
    // Note: Sanitization should be done on individual fields as needed

    //await logger.LogAsync($"Received {files.Count} files");
    log.Info($"Received {files.Count} files");

    foreach(var file in files)
    {
        var extension = Path.GetExtension(file.FileName);

        if(string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            return Results.Ok($"File '{file.FileName}' has an illegal file extension.");
        }
    }

    var inquiryId = Guid.NewGuid();

    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
    uploadsPath = Path.Combine(envConfig.SaveFileFolder, inquiryId.ToString());

    if (!Directory.Exists(uploadsPath))
        Directory.CreateDirectory(uploadsPath);

    var savedFiles = new List<string>();

    if (files is not null)
    {
        log.Info("Files detected. File count is " + files.Count);

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                log.Info("File being saved");

                var filePath = Path.Combine(uploadsPath, file.FileName);
                using var stream = new FileStream(filePath, FileMode.Create);
              
                await file.CopyToAsync(stream);
                savedFiles.Add(file.FileName);

                log.Info("File was saved.");
            }
        }
    }
    else
    {
        log.Info("No files detected");
    }

    if (!formData.ContainsKey("captchaSessionId") || string.IsNullOrEmpty(formData["captchaSessionId"]))
        return Results.BadRequest("captchaSessionId is required.");

    if (!formData.ContainsKey("captchaCode") || string.IsNullOrEmpty(formData["captchaCode"]))
        return Results.BadRequest("captchaCode is required.");

    var captchaSessionId = formData["captchaSessionId"].ToString();
    var captchaCode = formData["captchaCode"].ToString();

    log.Info($"Captcha validation - SessionId: {captchaSessionId}, UserInput: {captchaCode}");

    var isCaptchaValid = cs.ValidateCaptcha(captchaSessionId, captchaCode, cache);

    if (!isCaptchaValid)
    {
        log.Info("Captcha validation failed.");
        return Results.Ok("Invalid captcha.");
    }

    cache.Remove(captchaSessionId);
    log.Info("Captcha validated successfully.");

    // פה יש צורך לבצע שמירה של הנתונים בצורה כזאת או אחרת.

    var response = new
    {
        Message = "Form submitted successfully!",
        FormData = formData,
        UploadedFiles = savedFiles
    };

    return Results.Json(response);
}).DisableAntiforgery();

app.MapGet("/log", async (HttpContext context, [FromServices] IConfiguration config) =>
{
    // Default value
    int linesToRead = 50;

    // Parse optional query parameter
    if (context.Request.Query.TryGetValue("lines", out var linesVal) && int.TryParse(linesVal, out var parsedLines))
    {
        linesToRead = parsedLines;
    }

    Console.WriteLine(logPath);

    if (!File.Exists(logPath))
    {
        return Results.Ok("Log file not found.");
    }

    var lines = LogService.ReadLastLines(logPath, linesToRead);

    return Results.Json(lines, new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true
    });
});

app.MapPost("/send-email", async (EmailRequest request) =>
{
    var smtpServer = "SERVER IP";
    var smtpPort = 587;
    var username = "USER";
    var password = "PASS";
    var fromAddress = "HabaNoreply@court.gov.il";

    // identical to your EmailService behavior
    ServicePointManager.ServerCertificateValidationCallback =
        (sender, certificate, chain, sslPolicyErrors) => true;

    // ---- NEW: Decode the Base64 issue string ----
    string decodedIssue;
    try
    {
        var bytes = Convert.FromBase64String(request.Issue);
        decodedIssue = Encoding.UTF8.GetString(bytes);
    }
    catch
    {
        decodedIssue = "**Failed to decode Base64 issue**";
    }
    // --------------------------------------------

    var subject = "בקשה חסומה - פניות הציבור";

    var body = $@"
        <div style='direction:rtl;text-align:right;'>
            <p>שלום,</p>
            <p>התקבלה בקשה שנחסמה במערכת.</p>
            <p><strong>פירוט הבעיה:</strong></p>
            <pre style='white-space:pre-wrap;font-family:consolas;background:#f2f2f2;padding:10px;border-radius:8px;'>
{decodedIssue}
            </pre>
            <p><strong>כתובת IP:</strong> {request.IP}</p>
        </div>
    ";

    try
    {
        using var smtp = new SmtpClient(smtpServer, smtpPort)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true, // STARTTLS same as your config
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        using var message = new MailMessage()
        {
            From = new MailAddress(fromAddress),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        foreach (var recipient in envConfig.EmailList)
            message.To.Add(recipient);

        await smtp.SendMailAsync(message);

        return Results.Ok(new { success = true, message = "Email sent." });
    }
    catch (Exception ex)
    {
        return Results.Problem("Failed to send email: " + ex.Message);
    }
});

app.MapPost("/contact-details", async ([FromBody] ContactFormRequest request,
                                                 [FromServices] ILog log) =>
{
    log.Info($"Received contact details request. Court Case Number: {request.CourtCaseNumber}, Court House: {request.CourtHouse}");
    var response = new
    {
        success = true,
        message = "Validation successful",
        data = new
        {
            courtCaseNumber = request.CourtCaseNumber,
            contactDescription = request.ContactDescription,
            courtHouse = request.CourtHouse
        }
    };

    log.Info($"Contact details validation successful for Court Case Number: {request.CourtCaseNumber}");

    return Results.Ok(response);
}).DisableAntiforgery();

app.MapGet("/monthly-referral-report", async ([FromServices] ILog log,
                                               [FromServices] DatabaseService db,
                                               [FromQuery] int? month,
                                               [FromQuery] int? year) =>
{
    try
    {
        var targetMonth = month ?? DateTime.Now.Month;
        var targetYear = year ?? DateTime.Now.Year;
        
        if (targetMonth < 1 || targetMonth > 12)
        {
            log.Warn($"invalid month parameter received: {targetMonth}");
            return Results.BadRequest(new { error = "Month must be between 1 and 12" });
        }
        var currentDate = DateTime.Now;
        var requestedDate = new DateTime(targetYear, targetMonth, 1);
        var currentMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1);

        if (targetYear < 2000)
        {
            log.Warn($"invalid year parameter received: {targetYear}");
            return Results.BadRequest(new { error = "Year must be between 2000 and current year" });
        }

        if (requestedDate > currentMonthStart)
        {
            log.Warn($"future month requested: {targetMonth}/{targetYear}. Current month: {currentDate.Month}/{currentDate.Year}");
            return Results.BadRequest(new { error = $"Cannot request report for future months. Current month is {currentDate.Month}/{currentDate.Year}" });
        }

        log.Info($"Fetching monthly referral report for {targetMonth}/{targetYear}");

        var report = await db.GetMonthlyReferralReport(targetMonth, targetYear);

        log.Info($"Successfully retrieved monthly referral report. Total departments: {report.Count}");

        return Results.Ok(new
        {
            success = true,
            month = targetMonth,
            year = targetYear,
            reportDate = $"{targetYear}-{targetMonth:D2}",
            data = report
        });
    }
    catch (Exception ex)
    {
        log.Error($"Error retrieving monthly referral report: {ex.Message}", ex);
        return Results.Problem(
            detail: ex.Message,
            title: "Error retrieving monthly referral report"
        );
    }
});


app.Run();