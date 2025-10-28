# Email Notification Integration Guide

## Overview

This guide explains how the notification feature works with **direct HTTP integration** to your email export app. The notification feature forwards simple requests directly to your email export app, which handles database queries and sending emails via Azure Communication Services.

## Architecture

```
Static Web App → This API App → Email Export App → Database + Azure Communication Services
```

- **Static Web App**: Sends notification requests with recipient email and IDs
- **This API App**: Validates and forwards requests via HTTP POST
- **Email Export App**: Fetches data from database and sends emails using Azure Communication Services

---

## Input Variables (From Static Web App)

Your static web app should send these variables in a POST request to `/api/notification`:

| Variable Name    | Type   | Required | Description                                           |
| ---------------- | ------ | -------- | ----------------------------------------------------- |
| `recipientEmail` | string | ✅ Yes   | Email address of the recipient                        |
| `templateId`     | string | ❌ No    | Template identifier for email export app to use       |
| `dataId`         | string | ❌ No    | Database record ID for email export app to fetch data |

### Example Request from Static Web App

```javascript
// JavaScript example
const response = await fetch(
  "https://your-api-app.azurewebsites.net/api/notification",
  {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      recipientEmail: "user@example.com",
      templateId: "welcome-email",
      dataId: "order-12345",
    }),
  }
);

const result = await response.json();
console.log(result);
```

---

## Output to Email Export App

This API app forwards the request to your email export app at the configured URL with these variables:

### HTTP Request Details

- **Method**: POST
- **Endpoint**: `/api/send-email`
- **Content-Type**: `application/json`
- **Timeout**: 30 seconds

### Payload Structure

```json
{
  "recipientEmail": "user@example.com",
  "templateId": "welcome-email",
  "dataId": "order-12345"
}
```

### Variables Sent to Email Export App

| Variable Name    | Type   | Description                                |
| ---------------- | ------ | ------------------------------------------ |
| `recipientEmail` | string | Email address of the recipient             |
| `templateId`     | string | Template identifier (null if not provided) |
| `dataId`         | string | Database record ID (null if not provided)  |

---

## Email Export App Requirements

Your email export app should implement an endpoint that:

1. **Endpoint**: `POST /api/send-email`
2. **Accepts JSON**: With `recipientEmail`, `templateId`, and `dataId`
3. **Returns Success**: HTTP 200-299 status code on success
4. **Returns Error**: HTTP 400+ status code with error details on failure

### Example Email Export App Implementation (C#)

```csharp
[HttpPost("api/send-email")]
public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
{
    try
    {
        // 1. Fetch data from database using dataId
        var emailData = await _dbService.GetEmailDataAsync(request.DataId);

        // 2. Get template based on templateId
        var template = await _templateService.GetTemplateAsync(request.TemplateId);

        // 3. Send email using Azure Communication Services
        await _emailService.SendEmailAsync(
            to: request.RecipientEmail,
            subject: template.Subject,
            body: RenderTemplate(template, emailData)
        );

        return Ok(new { Success = true, MessageId = "email-123" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send email");
        return StatusCode(500, new { Success = false, Error = ex.Message });
    }
}

public class EmailRequest
{
    public string RecipientEmail { get; set; }
    public string TemplateId { get; set; }
    public string DataId { get; set; }
}
```

---

## Configuration

### Step 1: Configure Email Export App URL

Update `appsettings.json` with your email export app URL:

```json
{
  "EmailExportApp": {
    "Url": "https://your-email-export-app.azurewebsites.net"
  }
}
```

### Step 2: Update App Service Configuration

In Azure Portal:

1. Go to your App Service
2. Navigate to **Configuration** → **Application settings**
3. Add/Update:
   - **Name**: `EmailExportApp__Url`
   - **Value**: `https://your-email-export-app.azurewebsites.net`
4. Click **Save**

---

## Testing

### Test from Static Web App

```html
<!DOCTYPE html>
<html>
  <head>
    <title>Send Email Notification</title>
  </head>
  <body>
    <h1>Email Notification Test</h1>
    <form id="emailForm">
      <label>Recipient Email:</label>
      <input type="email" id="recipientEmail" required /><br /><br />

      <label>Template ID (optional):</label>
      <input type="text" id="templateId" /><br /><br />

      <label>Data ID (optional):</label>
      <input type="text" id="dataId" /><br /><br />

      <button type="submit">Send Notification</button>
    </form>
    <div id="result"></div>

    <script>
      document
        .getElementById("emailForm")
        .addEventListener("submit", async (e) => {
          e.preventDefault();

          const data = {
            recipientEmail: document.getElementById("recipientEmail").value,
            templateId: document.getElementById("templateId").value || null,
            dataId: document.getElementById("dataId").value || null,
          };

          try {
            const response = await fetch(
              "https://your-api-app.azurewebsites.net/api/notification",
              {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data),
              }
            );

            const result = await response.json();
            document.getElementById(
              "result"
            ).innerHTML = `<pre>${JSON.stringify(result, null, 2)}</pre>`;
          } catch (error) {
            document.getElementById(
              "result"
            ).innerHTML = `<p style="color:red">Error: ${error.message}</p>`;
          }
        });
    </script>
  </body>
</html>
```

### Test Response Example

```json
{
  "success": true,
  "message": "Email notification sent to email export app successfully",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "receivedAt": "2025-10-28T10:30:00Z",
  "status": "Forwarded",
  "recipient": "user@example.com",
  "templateId": "welcome-email",
  "dataId": "order-12345"
}
```

---

## User Data Feature (Unchanged)

The existing user data feature continues to work with Service Bus:

### User Data Flow

```
Static Web App → This API App → Service Bus Queue → Function App
```

### User Data Input Variables

- `name` (string, required)
- `age` (number, required)

The user data feature is **completely separate** and **unaffected** by the notification changes.

---

## Error Handling

### API App Returns Errors When:

1. **Validation Fails** (HTTP 400)

   - Missing `recipientEmail`
   - Invalid email format
   - `templateId` > 50 characters
   - `dataId` > 100 characters

2. **Email Export App Error** (HTTP 500)
   - Email export app returns non-success status
   - Connection timeout (30 seconds)
   - Network error

### Example Error Response

```json
{
  "success": false,
  "message": "Failed to forward email notification to email export app. Please try again later.",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

---

## Next Steps

1. ✅ Deploy this API app (user data feature already deployed)
2. ✅ Update `EmailExportApp__Url` in App Service configuration
3. ✅ Create/Deploy your email export app with `/api/send-email` endpoint
4. ✅ Configure email export app to access your database
5. ✅ Set up Azure Communication Services in email export app
6. ✅ Test end-to-end flow

---

## Summary

- **Simple Input**: Only 3 variables (`recipientEmail`, `templateId`, `dataId`)
- **Direct Connection**: HTTP POST to email export app (no Service Bus)
- **Email Export App**: Handles all database and email sending logic
- **User Data Safe**: Previous Service Bus integration unchanged
