# 🎴 Quick Reference Card - Variable Names

## 📥 INPUT Variables (From Static Web Apps to API)

### User Data Endpoint: `POST /api/userdata`

```
name          string    REQUIRED    User's full name
age           number    REQUIRED    User's age (1-150)
```

### Notification Endpoint: `POST /api/notification`

```
recipientEmail    string    REQUIRED    Email address
subject           string    REQUIRED    Email subject (1-200 chars)
message           string    REQUIRED    Email body (1-5000 chars)
senderName        string    OPTIONAL    Sender name (max 100 chars)
priority          string    OPTIONAL    "Low" | "Normal" | "High"
metadata          string    OPTIONAL    JSON string for custom data
```

---

## 📤 OUTPUT Variables (From API to Other Applications via Service Bus)

### Queue: `userdata-queue`

```
name            string     User's name
age             number     User's age
receivedAt      string     ISO 8601 timestamp
correlationId   string     Tracking GUID
```

### Queue: `notification-queue`

```
recipientEmail    string     Email address to send to
subject           string     Email subject
message           string     Email body content
senderName        string     Sender name (nullable)
priority          string     "Low" | "Normal" | "High"
metadata          string     Custom metadata (nullable)
receivedAt        string     ISO 8601 when received
correlationId     string     Tracking GUID
status            string     "Queued" | "Processing" | "Sent" | "Failed"
queuedAt          string     ISO 8601 when queued
```

---

## 🌐 API Endpoints

```
POST   /api/userdata              Submit user data
GET    /api/userdata/health       User data health check
POST   /api/notification          Send email notification
GET    /api/notification/health   Notification health check
GET    /health                    Overall health check
```

---

## 📊 Service Bus Queues

```
userdata-queue         For user data processing
notification-queue     For email notifications
```

---

## 📝 JavaScript Example (Static Web App)

```javascript
// User Data
const userData = {
  name: "John Doe",
  age: 25,
};

// Email Notification
const notification = {
  recipientEmail: "user@example.com",
  subject: "Welcome",
  message: "Thank you!",
  priority: "Normal",
};

// Send to API
fetch("https://your-api.azurewebsites.net/api/notification", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify(notification),
});
```

---

## 🔧 C# Consumer Example

```csharp
// Notification Queue Consumer
public class EmailNotificationMessage
{
    public string RecipientEmail { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public string Priority { get; set; }
    public string CorrelationId { get; set; }
}

[Function("SendEmail")]
public async Task Run(
    [ServiceBusTrigger("notification-queue")]
    string messageBody)
{
    var notification = JsonSerializer
        .Deserialize<EmailNotificationMessage>(messageBody);

    await SendEmailAsync(notification);
}
```

---

## ⚙️ Configuration

```json
{
  "ServiceBus": {
    "Namespace": "your-namespace.servicebus.windows.net",
    "UserDataQueue": "userdata-queue",
    "NotificationQueue": "notification-queue"
  }
}
```

---

## ✅ Response Format

```json
{
  "success": true,
  "message": "Email notification queued successfully",
  "correlationId": "12345678-1234-1234-1234-123456789abc",
  "receivedAt": "2025-10-28T10:30:00.123Z",
  "status": "Queued",
  "recipient": "user@example.com",
  "priority": "Normal"
}
```

---

## 🚀 Quick Test

```powershell
# Test Notification
$body = @{
  recipientEmail="test@example.com"
  subject="Test"
  message="Hello World"
  priority="Normal"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://your-api.azurewebsites.net/api/notification" `
  -Method Post -Body $body -ContentType "application/json"
```

---

**📚 Full Documentation:** See API-INTEGRATION-GUIDE.md
