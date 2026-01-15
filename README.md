# Public Complaint Form System

A comprehensive full-stack web application for handling public complaints and inquiries, featuring an Angular frontend and ASP.NET Core Web API backend with advanced security measures, file upload capabilities, and survey functionality.

## Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Key Features](#key-features)
- [Project Structure](#project-structure)
- [Security Considerations](#security-considerations)
- [Error Handling](#error-handling)
- [Cross-Origin Resource Sharing (CORS)](#cross-origin-resource-sharing-cors)
- [Installation Instructions](#installation-instructions)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Database Setup](#database-setup)
- [Deployment](#deployment)
- [Development Guidelines](#development-guidelines)

---

## Project Overview

The Public Complaint Form System is designed to facilitate citizen engagement with court services by providing a secure, user-friendly platform for submitting complaints, inquiries, and feedback. The system includes:

- Multi-step form interface with validation
- CAPTCHA verification to prevent automated submissions
- File upload capability with security validation
- Survey functionality for feedback collection
- Monthly referral reporting system
- Email notification system for blocked submissions
- Comprehensive logging and audit trails

---

## Architecture

### System Architecture

```
┌─────────────────┐         ┌──────────────────┐         ┌─────────────────┐
│                 │         │                  │         │                 │
│  Angular SPA    │◄───────►│  ASP.NET Core    │◄───────►│  SQL Server     │
│  (Frontend)     │  HTTP   │  Web API         │  ADO.NET│  Database       │
│                 │         │  (Backend)       │  Dapper │                 │
└─────────────────┘         └──────────────────┘         └─────────────────┘
      │                              │
      │                              │
      ▼                              ▼
┌─────────────────┐         ┌──────────────────┐
│  Static Files   │         │  File System     │
│  (Public Assets)│         │  (Uploads, Logs) │
└─────────────────┘         └──────────────────┘
```

### Application Flow

1. **User Interaction**: Users interact with the Angular frontend
2. **API Communication**: Frontend sends HTTP requests to the backend API
3. **Validation**: Backend validates requests, including CAPTCHA verification
4. **File Processing**: Uploaded files are validated and stored securely
5. **Data Persistence**: Form data is stored in SQL Server databases
6. **Logging**: All actions are logged using log4net
7. **Response**: API returns structured responses to the frontend

---

## Technology Stack

### Frontend

- **Framework**: Angular 19.1.0
- **UI Components**: Angular Material 19.2.3
- **Styling**: SCSS
- **State Management**: RxJS 7.8.0
- **Build Tool**: Angular CLI 19.1.5
- **Language**: TypeScript 5.7.2

### Backend

- **Framework**: ASP.NET Core 8.0 (.NET 8)
- **Web Server**: Kestrel
- **ORM**: Dapper 2.1.66
- **Database**: Microsoft SQL Server (via Microsoft.Data.SqlClient 6.0.1)
- **Image Processing**: SixLabors.ImageSharp 3.1.7
- **Validation**: FluentValidation 11.11.0
- **Logging**: log4net 3.0.4
- **Sanitization**: HtmlSanitizer 9.0.884

### DevOps & Infrastructure

- **Containerization**: Docker (Linux containers)
- **Version Control**: Git
- **Development Environment**: Visual Studio 2022 / Visual Studio Code

---

## Key Features

### 1. Multi-Step Form System

- **Contact Type Selection**: Choose between complaint, inquiry, or other types
- **Personal Details**: Capture contact information with validation
- **Contact Details**: Collect case numbers and descriptions
- **Document Upload**: Secure file upload with extension validation
- **Summary Review**: Review all entered information before submission

### 2. Security Features

#### CAPTCHA System
- Custom CAPTCHA generation using ImageSharp
- Session-based validation with expiration (1 hour)
- In-memory caching for CAPTCHA codes
- Prevents automated bot submissions

#### Anti-Forgery Protection
- CSRF token implementation (X-CSRF-TOKEN header)
- Protects against cross-site request forgery attacks

#### File Upload Security
- Whitelist-based file extension validation
- Allowed extensions: `.pdf`, `.doc`, `.docx`, `.png`, `.jpeg`, `.jpg`, `.gif`, `.ogg`, `.mp4`, `.mp3`, `.msg`
- File size validation
- Unique folder per submission (GUID-based)

#### Security Headers
- `Referrer-Policy: same-origin`
- `X-Frame-Options: DENY`
- `Content-Security-Policy: frame-ancestors 'none'`
- Prevents clickjacking and information disclosure

#### Input Sanitization
- HTML sanitization using HtmlSanitizer library
- SQL injection prevention via parameterized queries
- FluentValidation for model validation

### 3. Logging & Monitoring

- **Comprehensive Logging**: log4net with rolling file appender
- **Log Rotation**: Maximum 5 backup files, 5MB per file
- **Audit Trail**: All API requests and actions are logged
- **Log Endpoint**: `/log?lines=50` for retrieving recent log entries
- **Structured Logging**: Thread ID, log level, timestamp, and message

### 4. File Management

- **Upload Directory**: Configurable via `SaveFileFolder` setting
- **Organized Storage**: Each submission gets a unique GUID-based folder
- **File Tracking**: List of uploaded files returned in API response

### 5. Survey System

- Standalone survey functionality
- Duplicate submission prevention
- Separate database for survey data

### 6. Reporting

- **Monthly Referral Reports**: Statistical analysis by department
- **Comparative Analysis**: Month-over-month and year-over-year comparisons
- **Percentage Change Tracking**: Automatic calculation of trends

---

## Project Structure

```
Recruitment-Project/
│
├── PublicComplaintForm/                 # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── contact-type/            # Contact type selection component
│   │   │   ├── contactor-details/       # User details form component
│   │   │   ├── contact-details/         # Complaint details component
│   │   │   ├── document-upload/         # File upload component
│   │   │   ├── summary/                 # Form summary component
│   │   │   ├── finished/                # Success page component
│   │   │   ├── survey-page/             # Survey component
│   │   │   ├── models/                  # TypeScript interfaces/models
│   │   │   ├── breadcrumbs-manager.service.ts
│   │   │   ├── config.service.ts
│   │   │   ├── court-handler.service.ts
│   │   │   └── form-handler.service.ts
│   │   ├── main.ts
│   │   └── styles.scss
│   ├── public/                          # Static assets
│   │   ├── config.json                  # Environment-specific API URLs
│   │   └── [SVG icons and images]
│   ├── angular.json
│   ├── package.json
│   └── tsconfig.json
│
└── PublicComplaintForm_API/             # ASP.NET Core Backend
    ├── Models/
    │   ├── ConfigSettings.cs            # Configuration model
    │   ├── ContactFormRequest.cs        # Contact form DTO
    │   ├── MonthlyReferralReport.cs     # Reporting model
    │   ├── Court.cs                     # Court entity
    │   ├── CustomCaptcha.cs             # CAPTCHA model
    │   ├── EmailRequest.cs              # Email notification model
    │   ├── PublicComplaintData.cs       # Main form data model
    │   ├── SurveyData.cs                # Survey submission model
    │   └── SurveyQuestion.cs            # Survey question model
    │
    ├── Services/
    │   ├── CaptchaService.cs            # CAPTCHA generation & validation
    │   ├── DatabaseService.cs           # Database operations (Dapper)
    │   ├── LogService.cs                # Log reading utility
    │   └── SanitizingService.cs         # Input sanitization
    │
    ├── Program.cs                       # Application entry point & endpoints
    ├── appsettings.json                 # Configuration settings
    ├── log4net.config                   # Logging configuration
    ├── Dockerfile                       # Container configuration
    ├── PublicComplaintForm_API.csproj   # Project file
    ├── Uploads/                         # File upload directory
    └── Logs/                            # Application logs
```

---

## Security Considerations

### 1. Input Validation

**Backend Validation**:
- All input is validated using FluentValidation
- Data annotations on models enforce constraints
- String length limits prevent buffer overflow attacks
- Required fields are enforced

**Frontend Validation**:
- Angular Reactive Forms with validators
- Real-time feedback to users
- Client-side validation as first line of defense

### 2. File Upload Security

**Validation Layers**:
```csharp
var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".pdf", ".doc", ".docx", ".png", ".jpeg", ".jpg", 
    ".gif", ".ogg", ".mp4", ".mp3", ".msg"
};
```

**Security Measures**:
- File extension whitelist (not blacklist)
- Case-insensitive extension checking
- File size limits
- Unique storage directories to prevent overwriting
- Files stored outside of web root

### 3. SQL Injection Prevention

- **Parameterized Queries**: All database operations use parameterized queries via Dapper
- **No String Concatenation**: SQL queries never concatenate user input
- **ORM Layer**: Dapper handles parameter escaping automatically

Example:
```csharp
var parameters = new { Month = month, Year = year };
var results = await connection.QueryAsync<MonthlyReferralReport>(sql, parameters);
```

### 4. Cross-Site Scripting (XSS) Prevention

- **HTML Sanitization**: HtmlSanitizer library cleans user input
- **Content Security Policy**: Restricts resource loading
- **Output Encoding**: Angular automatically escapes output

### 5. CAPTCHA System

**Generation**:
- Random 6-character alphanumeric code
- Noise dots added for bot resistance
- Session-based storage with GUID

**Validation**:
- Case-insensitive comparison
- One-time use (cache cleared after validation)
- 1-hour expiration
- Session ID required for validation

### 6. Environment-Based Configuration

**Sensitive Data Protection**:
- Connection strings in `appsettings.json`
- Environment-based configuration via `ServerIdentity` variable
- Separate configurations for DMZTEST, CRM9D, and PROD
- No hardcoded credentials in source code

### 7. Email Security

**SMTP Configuration**:
- TLS/SSL encryption enabled
- Credential-based authentication
- Server certificate validation callback
- HTML email body with proper encoding

---

## Error Handling

### Global Exception Handler

**Development Environment**:
```csharp
app.UseDeveloperExceptionPage();
```
- Detailed error pages with stack traces
- For debugging purposes only

**Production Environment**:
```csharp
app.UseExceptionHandler(exceptionHandlerApp =>
{
    // Returns sanitized error response
    // Logs full exception details
    // Returns generic message to client
});
```

### Error Handling Strategy

1. **Catch at Service Layer**: Services catch and log specific exceptions
2. **Propagate or Transform**: Convert to appropriate HTTP responses
3. **Log Everything**: All errors logged with context
4. **Return User-Friendly Messages**: No sensitive information exposed

### Logging Levels

- **INFO**: General application flow (API requests, successful operations)
- **WARN**: Validation failures, recoverable errors
- **ERROR**: Exceptions, system errors
- **DEBUG**: Detailed diagnostic information (not in production)

Example:
```csharp
try
{
    // Operation
    log.Info($"Operation successful for {id}");
}
catch (Exception ex)
{
    log.Error($"Error in operation: {ex.Message}", ex);
    throw;
}
```

---

## Cross-Origin Resource Sharing (CORS)

### CORS Configuration

The API implements a permissive CORS policy for the frontend:

```csharp
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
```

### Key Points

- **Allowed Origin**: `http://localhost:4200` (Angular dev server)
- **Credentials**: Enabled to support cookies and authentication headers
- **Headers**: All headers allowed
- **Methods**: All HTTP methods allowed (GET, POST, PUT, DELETE, etc.)

### Production Considerations

For production deployment, modify CORS policy:

```csharp
policy.WithOrigins(
    "https://yourdomain.com",
    "https://www.yourdomain.com"
)
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials();
```

**Important**: Never use `.AllowAnyOrigin()` with `.AllowCredentials()` as this creates a security vulnerability.

---

## Installation Instructions

### Prerequisites

1. **Node.js** (v18 or higher) - [Download](https://nodejs.org/)
2. **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **SQL Server** (2019 or higher) or **SQL Server Express**
4. **Visual Studio 2022** (optional but recommended) or **Visual Studio Code**
5. **Git** - [Download](https://git-scm.com/)

### Clone Repository

```bash
git clone <repository-url>
cd Recruitment-Project
```

### Frontend Setup

```bash
cd PublicComplaintForm

# Install dependencies
npm install

# Verify installation
ng version
```

### Backend Setup

```bash
cd ../PublicComplaintForm_API

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build
```

---

## Configuration

### Backend Configuration

#### 1. appsettings.json

Configure environment-specific settings:

```json
{
  "DMZTEST": {
    "LocalSQL": "Server=localhost;Database=ComplaintDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "SaveFileFolder": "C:\\Uploads\\ComplaintForm",
    "SurveySQLConnectionString": "Server=localhost;Database=SurveyDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "EmailList": [
      "admin@example.com",
      "support@example.com"
    ]
  }
}
```

#### 2. Environment Variable

Set the `ServerIdentity` environment variable to select configuration:

**Windows (PowerShell)**:
```powershell
$env:ServerIdentity = "DMZTEST"
```

**Windows (Command Prompt)**:
```cmd
set ServerIdentity=DMZTEST
```

**Linux/macOS**:
```bash
export ServerIdentity=DMZTEST
```

#### 3. log4net.config

Logging is pre-configured. Adjust settings if needed:

```xml
<log4net>
  <appender name="RollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="Logs/app.log" />
    <maximumFileSize value="5MB" />
    <maxSizeRollBackups value="5" />
  </appender>
</log4net>
```

### Frontend Configuration

#### public/config.json

Configure API endpoints for different environments:

```json
{
  "localhost": {
    "apiUrl": "http://localhost:5209"
  },
  "production": {
    "apiUrl": "https://api.yourdomain.com"
  }
}
```

The application automatically detects the environment based on URL.

---

## Running the Application

### Development Mode

#### 1. Start Backend (Option A: Visual Studio)

1. Open `PublicComplaintForm_API.sln` in Visual Studio 2022
2. Set `ServerIdentity` environment variable in `launchSettings.json` or system environment
3. Press **F5** or click **Run**
4. API will start on `http://localhost:5209` (or configured port)

#### 2. Start Backend (Option B: Command Line)

```bash
cd PublicComplaintForm_API
dotnet run
```

API will be available at `http://localhost:5209`

#### 3. Start Frontend

```bash
cd PublicComplaintForm
ng serve
```

Application will be available at `http://localhost:4200`

#### 4. Access Application

Open browser and navigate to: `http://localhost:4200`

### Production Build

#### Frontend

```bash
cd PublicComplaintForm
ng build --configuration production
```

Built files will be in `dist/public-complaint-form/`

#### Backend

```bash
cd PublicComplaintForm_API
dotnet publish -c Release -o ./publish
```

Published files will be in `publish/` directory

---

## API Endpoints

### Public Endpoints

#### 1. Root Endpoint

```http
GET /
```

**Description**: Health check endpoint  
**Response**: `"API is running..."`

---

#### 2. Get Courts List

```http
GET /courts
```

**Description**: Retrieve list of available courts  
**Response**:
```json
{
  "courtsList": [
    {
      "courtId": "guid",
      "courtName": "Court Name"
    }
  ]
}
```

---

#### 3. Generate CAPTCHA

```http
GET /captcha
```

**Description**: Generate a new CAPTCHA image  
**Response**:
```json
{
  "sessionId": "guid",
  "captchaImage": "base64-encoded-image"
}
```

**Usage**:
1. Call this endpoint to get a CAPTCHA
2. Display the image to the user
3. Store the `sessionId` for validation
4. CAPTCHA expires in 1 hour

---

#### 4. Submit Form

```http
POST /submit-form
Content-Type: multipart/form-data
```

**Description**: Submit complaint form with files  
**Parameters**:
- `captchaSessionId` (required): CAPTCHA session ID
- `captchaCode` (required): User-entered CAPTCHA code
- Additional form fields
- `files`: Uploaded files (optional)

**Response**:
```json
{
  "message": "Form submitted successfully!",
  "formData": { ... },
  "uploadedFiles": ["file1.pdf", "file2.jpg"]
}
```

**Error Responses**:
- `400`: Missing CAPTCHA fields
- `200`: Invalid CAPTCHA (with error message)
- `200`: Invalid file extension

---

#### 5. Submit Survey

```http
POST /survey
Content-Type: application/json
```

**Description**: Submit survey responses  
**Request Body**:
```json
{
  "surveyId": "guid",
  "responses": [...]
}
```

**Response**:
```json
{
  "message": "Survey submitted successfully"
}
```

---

#### 6. Contact Details Validation

```http
POST /contact-details
Content-Type: application/json
```

**Description**: Validate and process contact details  
**Request Body**:
```json
{
  "courtCaseNumber": "12345",
  "contactDescription": "Description of issue",
  "courtHouse": "Court Name"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Validation successful",
  "data": {
    "courtCaseNumber": "12345",
    "contactDescription": "Description of issue",
    "courtHouse": "Court Name"
  }
}
```

---

#### 7. Monthly Referral Report

```http
GET /monthly-referral-report?month=12&year=2025
```

**Description**: Get monthly referral statistics  
**Query Parameters**:
- `month` (optional): Month (1-12), defaults to current month
- `year` (optional): Year, defaults to current year

**Response**:
```json
{
  "success": true,
  "month": 12,
  "year": 2025,
  "reportDate": "2025-12",
  "data": [
    {
      "departmentName": "Department A",
      "currentMonthTotal": 150,
      "previousMonthTotal": 140,
      "sameMonthLastYearTotal": 130,
      "percentChangeFromPrevMonth": 7.14,
      "percentChangeFromLastYear": 15.38
    }
  ]
}
```

---

#### 8. Send Email Notification

```http
POST /send-email
Content-Type: application/json
```

**Description**: Send email notification for blocked submissions  
**Request Body**:
```json
{
  "issue": "base64-encoded-issue-description",
  "ip": "192.168.1.1"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Email sent."
}
```

---

#### 9. Get Logs

```http
GET /log?lines=50
```

**Description**: Retrieve recent log entries  
**Query Parameters**:
- `lines` (optional): Number of lines to retrieve (default: 50)

**Response**: Array of log entries

---

## Database Setup

### Database Schema

The application uses two SQL Server databases:

1. **Main Database** (`LocalSQL`): Stores complaints, contacts, courts, and referrals
2. **Survey Database** (`SurveySQLConnectionString`): Stores survey responses

### Required Tables

#### Main Database Tables

**1. Courts Table**
```sql
CREATE TABLE Courts (
    CourtId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourtName NVARCHAR(200) NOT NULL
);
```

**2. Contacts Table**
```sql
CREATE TABLE Contacts (
    ContactId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(200),
    Phone NVARCHAR(20),
    IdNumber NVARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE()
);
```

**3. Complaints Table**
```sql
CREATE TABLE Complaints (
    ComplaintId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ContactId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Contacts(ContactId),
    InquiryId UNIQUEIDENTIFIER UNIQUE NOT NULL,
    CourtId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Courts(CourtId),
    CourtCaseNumber NVARCHAR(100),
    Description NVARCHAR(MAX),
    ReceivedFiles BIT DEFAULT 0,
    SubmissionDate DATETIME DEFAULT GETDATE()
);
```

**4. Departments Table** (for reporting)
```sql
CREATE TABLE Departments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    DepartmentName NVARCHAR(200) NOT NULL
);
```

**5. Referrals Table** (for reporting)
```sql
CREATE TABLE Referrals (
    ReferralId INT PRIMARY KEY IDENTITY(1,1),
    DepartmentId INT FOREIGN KEY REFERENCES Departments(Id),
    ReferralDate DATE NOT NULL,
    Details NVARCHAR(MAX)
);
```

#### Survey Database Tables

**Survey Responses Table**
```sql
CREATE TABLE SurveyResponses (
    ResponseId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SurveyId UNIQUEIDENTIFIER NOT NULL,
    QuestionId INT NOT NULL,
    Answer NVARCHAR(MAX),
    SubmittedDate DATETIME DEFAULT GETDATE()
);
```

### Database Connection Setup

**SQL Server Authentication**:
```
Server=YOUR_SERVER;Database=ComplaintDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

**Windows Authentication**:
```
Server=YOUR_SERVER;Database=ComplaintDB;Trusted_Connection=True;TrustServerCertificate=True;
```

### Initial Data

Populate the Courts table with initial data:

```sql
INSERT INTO Courts (CourtName) VALUES 
    ('בית משפט עליון'),
    ('בית משפט מחוזי תל אביב'),
    ('בית משפט מחוזי ירושלים'),
    ('בית משפט השלום תל אביב');
```

---

## Deployment

### Docker Deployment

#### 1. Build Docker Image

```bash
cd PublicComplaintForm_API
docker build -t public-complaint-api:latest .
```

#### 2. Run Container

```bash
docker run -d \
  -p 8080:8080 \
  -e ServerIdentity=PROD \
  -e ConnectionStrings__LocalSQL="YOUR_CONNECTION_STRING" \
  -v /path/to/uploads:/app/Uploads \
  -v /path/to/logs:/app/Logs \
  --name complaint-api \
  public-complaint-api:latest
```

### IIS Deployment

#### Backend (API)

1. **Publish the Application**:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Create IIS Website**:
   - Open IIS Manager
   - Add new website pointing to `publish` folder
   - Configure Application Pool (.NET CLR Version: No Managed Code)
   - Set environment variables in Application Pool settings

3. **Configure Environment Variables**:
   - Set `ServerIdentity` in Application Pool environment variables
   - Ensure upload and log directories have write permissions

#### Frontend (Angular)

1. **Build for Production**:
   ```bash
   ng build --configuration production
   ```

2. **Deploy to Web Server**:
   - Copy contents of `dist/public-complaint-form/browser/` to web root
   - Configure URL rewriting for Angular routing
   - Update `config.json` with production API URL

**web.config for Angular** (URL Rewriting):
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Angular Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

### Linux Deployment (Nginx)

#### 1. Install .NET Runtime

```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

#### 2. Configure Systemd Service

Create `/etc/systemd/system/complaint-api.service`:

```ini
[Unit]
Description=Public Complaint API
After=network.target

[Service]
WorkingDirectory=/var/www/complaint-api
ExecStart=/usr/bin/dotnet /var/www/complaint-api/PublicComplaintForm_API.dll
Restart=always
RestartSec=10
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ServerIdentity=PROD

[Install]
WantedBy=multi-user.target
```

#### 3. Start Service

```bash
sudo systemctl enable complaint-api
sudo systemctl start complaint-api
```

#### 4. Configure Nginx Reverse Proxy

```nginx
server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://localhost:5209;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Development Guidelines

### Code Style

#### Backend (C#)
- Follow Microsoft C# coding conventions
- Use PascalCase for public members
- Use camelCase for private fields with `_` prefix
- Always use explicit types (avoid `var` unless type is obvious)
- Add XML documentation comments for public APIs

#### Frontend (TypeScript/Angular)
- Follow Angular style guide
- Use kebab-case for file names
- Use camelCase for variables and functions
- Use PascalCase for classes and interfaces
- Prefix interfaces with `I` when appropriate

### Git Workflow

1. **Feature Branches**: Create branch from `master` for new features
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Commit Messages**: Use clear, descriptive commit messages
   ```
   feat: Add CAPTCHA validation to contact form
   fix: Resolve file upload permission issue
   docs: Update API documentation
   ```

3. **Pull Requests**: Create PR when feature is complete
   - Include description of changes
   - Reference related issues
   - Ensure all tests pass

### Testing

#### Backend Testing
```bash
cd PublicComplaintForm_API
dotnet test
```

#### Frontend Testing
```bash
cd PublicComplaintForm
ng test
```

### Debugging

#### Backend Debugging
- Use Visual Studio debugger
- Check logs in `Logs/app.log`
- Use `/log?lines=100` endpoint for recent logs

#### Frontend Debugging
- Use browser DevTools
- Check network tab for API requests
- Use Angular DevTools extension

---

## Troubleshooting

### Common Issues

#### 1. CORS Errors

**Problem**: `Access-Control-Allow-Origin` header errors  
**Solution**: Verify CORS configuration in `Program.cs` includes correct origin

#### 2. Database Connection Failures

**Problem**: Cannot connect to SQL Server  
**Solution**: 
- Verify connection string in `appsettings.json`
- Ensure SQL Server is running
- Check firewall settings
- Add `TrustServerCertificate=True` if using self-signed certificates

#### 3. File Upload Fails

**Problem**: Files not uploading or permission denied  
**Solution**:
- Check `SaveFileFolder` path exists and is writable
- Verify file extension is in allowed list
- Check file size limits

#### 4. CAPTCHA Validation Fails

**Problem**: CAPTCHA always fails validation  
**Solution**:
- Check session ID is being passed correctly
- Verify CAPTCHA hasn't expired (1-hour limit)
- Ensure case-insensitive comparison

#### 5. Logs Not Writing

**Problem**: `Logs/app.log` not created  
**Solution**:
- Verify `Logs` directory exists and is writable
- Check `log4net.config` is in application root
- Ensure log4net is initialized in `Program.cs`

---

## License

[Specify your license here]

## Contributors

[List project contributors]

## Support

For issues and questions:
- **Email**: [support email]
- **Issue Tracker**: [GitHub issues URL]

---

**Last Updated**: January 2026  
**Version**: 1.0.0
