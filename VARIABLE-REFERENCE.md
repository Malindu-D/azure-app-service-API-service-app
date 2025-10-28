# Variable Reference - Complete Guide

This document lists all input and output variables for both features in this API application.

---

## Feature 1: User Data Submission

### Input Variables (from Static Web App to API)

Send POST request to: `https://your-api-app.azurewebsites.net/api/userdata`

```json
{
  "name": "John Doe",
  "age": 25
}
```

| Variable | Type   | Required | Validation       |
| -------- | ------ | -------- | ---------------- |
| `name`   | string | ✅ Yes   | 1-100 characters |
| `age`    | number | ✅ Yes   | 1-150            |

### Output Variables (to Service Bus Queue)

Queue Name: `userdata-queue`

```json
{
  "name": "John Doe",
  "age": 25,
  "receivedAt": "2025-10-28T10:30:00Z",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

| Variable        | Type   | Description                      |
| --------------- | ------ | -------------------------------- |
| `name`          | string | User's name (from input)         |
| `age`           | number | User's age (from input)          |
| `receivedAt`    | string | ISO 8601 timestamp when received |
| `correlationId` | string | Unique tracking ID (GUID)        |

---

## Feature 2: Email Notification

### Input Variables (from Static Web App to API)

Send POST request to: `https://your-api-app.azurewebsites.net/api/notification`

```json
{
  "recipientEmail": "user@example.com",
  "templateId": "welcome-email",
  "dataId": "order-12345"
}
```

| Variable         | Type   | Required | Validation         |
| ---------------- | ------ | -------- | ------------------ |
| `recipientEmail` | string | ✅ Yes   | Valid email format |
| `templateId`     | string | ❌ No    | Max 50 characters  |
| `dataId`         | string | ❌ No    | Max 100 characters |

### Output Variables (to Email Export App)

HTTP POST to: `{EmailExportApp:Url}/api/send-email`

```json
{
  "recipientEmail": "user@example.com",
  "templateId": "welcome-email",
  "dataId": "order-12345"
}
```

| Variable         | Type   | Description                               |
| ---------------- | ------ | ----------------------------------------- |
| `recipientEmail` | string | Email address of recipient                |
| `templateId`     | string | Template ID (null if not provided)        |
| `dataId`         | string | Database record ID (null if not provided) |

---

## Quick Reference Card

### For Static Web App Developers

```javascript
// Feature 1: Submit User Data
fetch("https://your-api.azurewebsites.net/api/userdata", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    name: "John Doe", // Required: string, 1-100 chars
    age: 25, // Required: number, 1-150
  }),
});

// Feature 2: Send Email Notification
fetch("https://your-api.azurewebsites.net/api/notification", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    recipientEmail: "user@example.com", // Required: valid email
    templateId: "welcome-email", // Optional: string, max 50 chars
    dataId: "order-12345", // Optional: string, max 100 chars
  }),
});
```

### For Function App Developers (User Data Consumer)

```csharp
// Read from Service Bus queue: userdata-queue
public class UserDataMessage
{
    public string Name { get; set; }           // User's name
    public int Age { get; set; }              // User's age
    public DateTime ReceivedAt { get; set; }  // When API received it
    public string CorrelationId { get; set; } // Tracking ID
}
```

### For Email Export App Developers

```csharp
// Implement endpoint: POST /api/send-email
public class EmailRequest
{
    public string RecipientEmail { get; set; }  // Email address
    public string TemplateId { get; set; }      // Template ID (nullable)
    public string DataId { get; set; }          // Database ID (nullable)
}
```

---

## Configuration Variables

### In appsettings.json / App Service Configuration

```json
{
  "ServiceBus": {
    "Namespace": "your-namespace.servicebus.windows.net",
    "UserDataQueue": "userdata-queue"
  },
  "EmailExportApp": {
    "Url": "https://your-email-export-app.azurewebsites.net"
  },
  "Cors": {
    "AllowedOrigins": ["https://your-static-web-app.azurestaticapps.net"]
  }
}
```

### Azure App Service Configuration Keys

Use these exact keys in Azure Portal → Configuration:

- `ServiceBus__Namespace`
- `ServiceBus__UserDataQueue`
- `EmailExportApp__Url`
- `Cors__AllowedOrigins__0`
- `Cors__AllowedOrigins__1`
- `ApplicationInsights__ConnectionString`

---

## Endpoints Summary

| Endpoint                   | Method | Purpose                                  |
| -------------------------- | ------ | ---------------------------------------- |
| `/api/userdata`            | POST   | Submit user data to Service Bus          |
| `/api/userdata/health`     | GET    | Health check for user data service       |
| `/api/notification`        | POST   | Forward email notification to export app |
| `/api/notification/health` | GET    | Health check for notification service    |
| `/health`                  | GET    | Overall API health check                 |
| `/swagger`                 | GET    | API documentation (dev only)             |
