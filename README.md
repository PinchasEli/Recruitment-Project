# Public Complaint Form System

Full-stack web application for handling public complaints and inquiries with Angular frontend and ASP.NET Core Web API backend.

## Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Key Features](#key-features)
- [Project Structure](#project-structure)
- [Security](#security)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Deployment](#deployment)

---

## Overview

A secure platform for submitting complaints, inquiries, and feedback with:
- Multi-step form interface with validation
- CAPTCHA verification
- Secure file upload
- Survey functionality
- Monthly referral reporting
- Email notifications
- Comprehensive logging

---

## Technology Stack

### Frontend
- Angular 19.1.0
- Angular Material 19.2.3
- TypeScript 5.7.2
- RxJS 7.8.0

### Backend
- ASP.NET Core 8.0 (.NET 8)
- Dapper 2.1.66
- Microsoft SQL Server
- log4net 3.0.4
- SixLabors.ImageSharp 3.1.7
- FluentValidation 11.11.0

---

## Key Features

### Multi-Step Form
- Contact type selection
- Personal details with validation
- Case information collection
- Secure document upload
- Summary review

### Security Features
- **CAPTCHA**: Custom generation with 1-hour session expiration
- **CSRF Protection**: Anti-forgery tokens (X-CSRF-TOKEN)
- **File Upload**: Whitelist validation (`.pdf`, `.doc`, `.docx`, `.png`, `.jpeg`, `.jpg`, `.gif`, `.ogg`, `.mp4`, `.mp3`, `.msg`)
- **Security Headers**: CSP, X-Frame-Options, Referrer-Policy
- **Input Sanitization**: HtmlSanitizer + parameterized queries
- **Environment-based Configuration**: Separate settings per environment

### Logging & Monitoring
- log4net with rolling file appender (5MB files, 5 backups)
- Audit trail for all API requests
- Log retrieval endpoint: `/log?lines=50`

### File Management
- Unique GUID-based folders per submission
- Configurable upload directory
- File tracking in responses

---

## Project Structure

```
Recruitment-Project/
├── PublicComplaintForm/              # Angular Frontend
│   ├── src/app/
│   │   ├── contact-type/
│   │   ├── contactor-details/
│   │   ├── contact-details/
│   │   ├── document-upload/
│   │   ├── summary/
│   │   ├── survey-page/
│   │   └── models/
│   └── public/config.json            # API URLs
│
└── PublicComplaintForm_API/          # ASP.NET Core Backend
    ├── Models/                       # Data models
    ├── Services/                     # Business logic
    │   ├── CaptchaService.cs
    │   ├── DatabaseService.cs
    │   └── LogService.cs
    ├── Program.cs                    # Entry point & endpoints
    ├── appsettings.json              # Configuration
    ├── log4net.config                # Logging settings
    └── Dockerfile
```

---

## Security

### Authentication & Validation
- CSRF tokens for all state-changing operations
- FluentValidation for model validation
- Client-side and server-side validation

### File Upload Security
- Extension whitelist (case-insensitive)
- File size limits
- Isolated storage per submission
- Files stored outside web root

### SQL Injection Prevention
- Parameterized queries via Dapper
- No string concatenation in SQL

### XSS Prevention
- HTML sanitization (HtmlSanitizer)
- Content Security Policy headers
- Angular automatic output escaping

### CAPTCHA System
- 6-character alphanumeric code
- Session-based with GUID
- One-time use
- 1-hour expiration

### Error Handling
- Development: Detailed error pages
- Production: Generic messages, full logging
- All exceptions logged with context

### CORS Configuration
```csharp
// Development
policy.WithOrigins("http://localhost:4200")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();

// Production: Update with actual domain
```

---

## Installation

### Prerequisites
- Node.js v18+
- .NET 8 SDK
- SQL Server 2019+
- Git

### Setup

**Clone Repository**
```bash
git clone <repository-url>
cd Recruitment-Project
```

**Frontend**
```bash
cd PublicComplaintForm
npm install
```

**Backend**
```bash
cd PublicComplaintForm_API
dotnet restore
dotnet build
```

---

## Configuration

### Backend (appsettings.json)

```json
{
  "DMZTEST": {
    "LocalSQL": "Server=localhost;Database=ComplaintDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "SaveFileFolder": "C:\\Uploads\\ComplaintForm",
    "SurveySQLConnectionString": "Server=localhost;Database=SurveyDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "EmailList": ["admin@example.com"]
  }
}
```

**Set Environment Variable**
```bash
# Windows
set ServerIdentity=DMZTEST

# Linux/macOS
export ServerIdentity=DMZTEST
```

### Frontend (public/config.json)

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

---

## Running the Application

### Development

**Backend**
```bash
cd PublicComplaintForm_API
dotnet run
# API runs on http://localhost:5209
```

**Frontend**
```bash
cd PublicComplaintForm
ng serve
# App runs on http://localhost:4200
```

### Production Build

**Frontend**
```bash
ng build --configuration production
# Output: dist/public-complaint-form/
```

**Backend**
```bash
dotnet publish -c Release -o ./publish
```

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/` | Health check |
| `GET` | `/courts` | Get courts list |
| `GET` | `/captcha` | Generate CAPTCHA image |
| `POST` | `/submit-form` | Submit complaint form (multipart) |
| `POST` | `/survey` | Submit survey responses |
| `POST` | `/contact-details` | Validate contact details |
| `GET` | `/monthly-referral-report?month=X&year=Y` | Get monthly statistics |
| `POST` | `/send-email` | Send notification email |
| `GET` | `/log?lines=50` | Retrieve log entries |

### Example: Generate CAPTCHA

**Request**
```http
GET /captcha
```

**Response**
```json
{
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "captchaImage": "data:image/png;base64,iVBORw0KGgo..."
}
```

### Example: Submit Form

**Request**
```http
POST /submit-form
Content-Type: multipart/form-data

captchaSessionId=3fa85f64-5717-4562-b3fc-2c963f66afa6
captchaCode=ABC123
files=@document.pdf
```

**Response**
```json
{
  "message": "Form submitted successfully!",
  "uploadedFiles": ["document.pdf"]
}
```

---

## Deployment

### Docker

```bash
cd PublicComplaintForm_API
docker build -t complaint-api .
docker run -d -p 8080:8080 \
  -e ServerIdentity=PROD \
  -v /uploads:/app/Uploads \
  -v /logs:/app/Logs \
  complaint-api
```

### IIS

**Backend**
1. Publish: `dotnet publish -c Release -o ./publish`
2. Create IIS site pointing to `publish` folder
3. Set Application Pool to "No Managed Code"
4. Configure `ServerIdentity` environment variable
5. Grant write permissions to Uploads/Logs folders

**Frontend**
1. Build: `ng build --configuration production`
2. Deploy `dist/public-complaint-form/browser/` to web root
3. Configure URL rewriting for Angular routing

### Linux (Systemd + Nginx)

**Systemd Service** (`/etc/systemd/system/complaint-api.service`)
```ini
[Unit]
Description=Complaint API
After=network.target

[Service]
WorkingDirectory=/var/www/complaint-api
ExecStart=/usr/bin/dotnet PublicComplaintForm_API.dll
Restart=always
Environment=ServerIdentity=PROD

[Install]
WantedBy=multi-user.target
```

**Nginx Configuration**
```nginx
server {
    listen 80;
    server_name api.yourdomain.com;
    
    location / {
        proxy_pass http://localhost:5209;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
```

---

## Database Setup

### Required Tables

```sql
-- Courts
CREATE TABLE Courts (
    CourtId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourtName NVARCHAR(200) NOT NULL
);

-- Contacts
CREATE TABLE Contacts (
    ContactId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(200),
    Phone NVARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Complaints
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

-- Departments & Referrals (for reporting)
CREATE TABLE Departments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    DepartmentName NVARCHAR(200) NOT NULL
);

CREATE TABLE Referrals (
    ReferralId INT PRIMARY KEY IDENTITY(1,1),
    DepartmentId INT FOREIGN KEY REFERENCES Departments(Id),
    ReferralDate DATE NOT NULL,
    Details NVARCHAR(MAX)
);
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| **CORS Errors** | Verify origin in `Program.cs` CORS policy |
| **Database Connection** | Check connection string, SQL Server status, firewall |
| **File Upload Fails** | Verify folder exists, has write permissions, extension allowed |
| **CAPTCHA Invalid** | Check session ID, expiration (1 hour), case-insensitive code |
| **Logs Not Writing** | Ensure `Logs/` folder exists with write permissions |

---

**Last Updated**: January 2026  
**Version**: 1.0.0
