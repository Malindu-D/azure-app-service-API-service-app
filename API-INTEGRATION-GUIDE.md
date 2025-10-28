# 📋 API Integration Guide - Variable Names & Contracts

This document provides all the variable names and data contracts you need to integrate with this API from your static web applications and other consuming applications.

---

## 🔹 Feature 1: User Data Submission

### Static Web App → API (Input Variables)

**Endpoint:** `POST /api/userdata`

**Request Body (JSON):**

```json
{
  "name": "string",     // Variable name: name
  "age": number         // Variable name: age
}
```

**Input Variable Details:**

| Variable Name | Type   | Required | Validation       | Description      |
| ------------- | ------ | -------- | ---------------- | ---------------- |
| `name`        | string | ✅ Yes   | 1-100 characters | User's full name |
| `age`         | number | ✅ Yes   | 1-150            | User's age       |

**Example Request (JavaScript):**

```javascript
const userData = {
  name: "John Doe", // Use variable name: name
  age: 25, // Use variable name: age
};

fetch("https://your-api.azurewebsites.net/api/userdata", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify(userData),
});
```

**Example Request (Python):**

```python
import requests

user_data = {
    "name": "John Doe",  # Variable name: name
    "age": 25            # Variable name: age
}

response = requests.post(
    'https://your-api.azurewebsites.net/api/userdata',
    json=user_data
)
```

### API → Function App (Output Variables via Service Bus)

**Queue Name:** `userdata-queue`

**Message Body (JSON):**

```json
{
  "name": "string",           // Variable name: name
  "age": number,              // Variable name: age
  "receivedAt": "datetime",   // Variable name: receivedAt (ISO 8601)
  "correlationId": "string"   // Variable name: correlationId
}
```

**Output Variable Details:**

| Variable Name   | Type              | Description                             | Example                                  |
| --------------- | ----------------- | --------------------------------------- | ---------------------------------------- |
| `name`          | string            | User's name from input                  | `"John Doe"`                             |
| `age`           | number            | User's age from input                   | `25`                                     |
| `receivedAt`    | string (ISO 8601) | Timestamp when API received the request | `"2025-10-28T10:30:00.123Z"`             |
| `correlationId` | string (GUID)     | Unique tracking ID                      | `"12345678-1234-1234-1234-123456789abc"` |

**Service Bus Message Properties:**

| Property Name   | Value                | Description                 |
| --------------- | -------------------- | --------------------------- |
| `MessageType`   | `"UserData"`         | Use this to filter messages |
| `ReceivedAt`    | datetime             | When message was received   |
| `ContentType`   | `"application/json"` | Message format              |
| `CorrelationId` | GUID string          | Tracking ID                 |

**Example Consumer (C# Function App):**

```csharp
public class UserDataMessage
{
    public string Name { get; set; }              // Variable: name
    public int Age { get; set; }                  // Variable: age
    public DateTime ReceivedAt { get; set; }      // Variable: receivedAt
    public string CorrelationId { get; set; }     // Variable: correlationId
}

[Function("ProcessUserData")]
public async Task Run(
    [ServiceBusTrigger("userdata-queue", Connection = "ServiceBusConnection")]
    string messageBody)
{
    var userData = JsonSerializer.Deserialize<UserDataMessage>(messageBody);
    // Access: userData.Name, userData.Age, userData.ReceivedAt, userData.CorrelationId
}
```

---

## 🔹 Feature 2: Email Notification

### Static Web App → API (Input Variables)

**Endpoint:** `POST /api/notification`

**Request Body (JSON):**

```json
{
  "recipientEmail": "string", // Variable name: recipientEmail
  "subject": "string", // Variable name: subject
  "message": "string", // Variable name: message
  "senderName": "string", // Variable name: senderName (optional)
  "priority": "string", // Variable name: priority (optional)
  "metadata": "string" // Variable name: metadata (optional)
}
```

**Input Variable Details:**

| Variable Name    | Type   | Required | Validation                        | Description                         |
| ---------------- | ------ | -------- | --------------------------------- | ----------------------------------- |
| `recipientEmail` | string | ✅ Yes   | Valid email format, max 254 chars | Email address to send to            |
| `subject`        | string | ✅ Yes   | 1-200 characters                  | Email subject line                  |
| `message`        | string | ✅ Yes   | 1-5000 characters                 | Email body content                  |
| `senderName`     | string | ❌ No    | Max 100 characters                | Name of sender                      |
| `priority`       | string | ❌ No    | "Low", "Normal", or "High"        | Email priority (default: "Normal")  |
| `metadata`       | string | ❌ No    | Any valid string                  | Custom metadata/tags as JSON string |

**Example Request (JavaScript):**

```javascript
const notification = {
  recipientEmail: "user@example.com", // Variable: recipientEmail
  subject: "Welcome to Our Service", // Variable: subject
  message: "Thank you for signing up!", // Variable: message
  senderName: "Support Team", // Variable: senderName
  priority: "High", // Variable: priority
  metadata: '{"campaign":"welcome"}', // Variable: metadata
};

fetch("https://your-api.azurewebsites.net/api/notification", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify(notification),
});
```

**Example Request (Python):**

```python
import requests

notification = {
    "recipientEmail": "user@example.com",   # Variable: recipientEmail
    "subject": "Welcome to Our Service",    # Variable: subject
    "message": "Thank you for signing up!", # Variable: message
    "senderName": "Support Team",           # Variable: senderName
    "priority": "High",                     # Variable: priority
    "metadata": '{"campaign":"welcome"}'    # Variable: metadata
}

response = requests.post(
    'https://your-api.azurewebsites.net/api/notification',
    json=notification
)
```

**Example Request (cURL):**

```bash
curl -X POST https://your-api.azurewebsites.net/api/notification \
  -H "Content-Type: application/json" \
  -d '{
    "recipientEmail": "user@example.com",
    "subject": "Welcome",
    "message": "Thank you!",
    "priority": "Normal"
  }'
```

### API → Other Application (Output Variables via Service Bus)

**Queue Name:** `notification-queue`

**Message Body (JSON):**

```json
{
  "recipientEmail": "string", // Variable name: recipientEmail
  "subject": "string", // Variable name: subject
  "message": "string", // Variable name: message
  "senderName": "string", // Variable name: senderName
  "priority": "string", // Variable name: priority
  "metadata": "string", // Variable name: metadata
  "receivedAt": "datetime", // Variable name: receivedAt (ISO 8601)
  "correlationId": "string", // Variable name: correlationId
  "status": "string", // Variable name: status
  "queuedAt": "datetime" // Variable name: queuedAt (ISO 8601)
}
```

**Output Variable Details:**

| Variable Name    | Type              | Description                   | Example                                          |
| ---------------- | ----------------- | ----------------------------- | ------------------------------------------------ |
| `recipientEmail` | string            | Email address to send to      | `"user@example.com"`                             |
| `subject`        | string            | Email subject                 | `"Welcome to Our Service"`                       |
| `message`        | string            | Email body content            | `"Thank you for signing up!"`                    |
| `senderName`     | string            | Sender name (nullable)        | `"Support Team"`                                 |
| `priority`       | string            | Priority level                | `"Low"`, `"Normal"`, or `"High"`                 |
| `metadata`       | string            | Custom metadata (nullable)    | `'{"campaign":"welcome"}'`                       |
| `receivedAt`     | string (ISO 8601) | When API received the request | `"2025-10-28T10:30:00.123Z"`                     |
| `correlationId`  | string (GUID)     | Unique tracking ID            | `"12345678-1234-1234-1234-123456789abc"`         |
| `status`         | string            | Message status                | `"Queued"`, `"Processing"`, `"Sent"`, `"Failed"` |
| `queuedAt`       | string (ISO 8601) | When message was queued       | `"2025-10-28T10:30:00.456Z"`                     |

**Service Bus Message Properties:**

| Property Name    | Value/Variable         | Description                           |
| ---------------- | ---------------------- | ------------------------------------- |
| `MessageType`    | `"EmailNotification"`  | Use this to filter messages           |
| `Priority`       | `priority` value       | Email priority for routing            |
| `ReceivedAt`     | `receivedAt` value     | When received                         |
| `RecipientEmail` | `recipientEmail` value | Recipient for filtering               |
| `Status`         | `status` value         | Current status                        |
| `ContentType`    | `"application/json"`   | Message format                        |
| `CorrelationId`  | `correlationId` value  | Tracking ID                           |
| `Subject`        | `subject` value        | Email subject (in Service Bus header) |

**Example Consumer (C# Function App):**

```csharp
public class EmailNotificationMessage
{
    public string RecipientEmail { get; set; }    // Variable: recipientEmail
    public string Subject { get; set; }           // Variable: subject
    public string Message { get; set; }           // Variable: message
    public string SenderName { get; set; }        // Variable: senderName
    public string Priority { get; set; }          // Variable: priority
    public string Metadata { get; set; }          // Variable: metadata
    public DateTime ReceivedAt { get; set; }      // Variable: receivedAt
    public string CorrelationId { get; set; }     // Variable: correlationId
    public string Status { get; set; }            // Variable: status
    public DateTime QueuedAt { get; set; }        // Variable: queuedAt
}

[Function("ProcessEmailNotification")]
public async Task Run(
    [ServiceBusTrigger("notification-queue", Connection = "ServiceBusConnection")]
    ServiceBusReceivedMessage sbMessage,
    ServiceBusMessageActions messageActions)
{
    var messageBody = sbMessage.Body.ToString();
    var notification = JsonSerializer.Deserialize<EmailNotificationMessage>(messageBody);

    // Access properties:
    var email = notification.RecipientEmail;
    var subject = notification.Subject;
    var body = notification.Message;
    var priority = notification.Priority;
    var correlationId = notification.CorrelationId;

    // Access Service Bus properties:
    var messageType = sbMessage.ApplicationProperties["MessageType"];  // "EmailNotification"
    var priorityProp = sbMessage.ApplicationProperties["Priority"];

    // Process the email notification...
    await SendEmailAsync(notification);

    await messageActions.CompleteMessageAsync(sbMessage);
}
```

**Example Consumer (Node.js/TypeScript):**

```typescript
interface EmailNotificationMessage {
  recipientEmail: string; // Variable: recipientEmail
  subject: string; // Variable: subject
  message: string; // Variable: message
  senderName?: string; // Variable: senderName
  priority: string; // Variable: priority
  metadata?: string; // Variable: metadata
  receivedAt: string; // Variable: receivedAt (ISO 8601)
  correlationId: string; // Variable: correlationId
  status: string; // Variable: status
  queuedAt: string; // Variable: queuedAt (ISO 8601)
}

// Service Bus receiver
receiver.subscribe({
  processMessage: async (message) => {
    const notification: EmailNotificationMessage = message.body;

    console.log(`Processing: ${notification.recipientEmail}`);
    console.log(`Subject: ${notification.subject}`);
    console.log(`Priority: ${notification.priority}`);
    console.log(`CorrelationId: ${notification.correlationId}`);

    // Send email using your email service...
    await sendEmail(notification);
  },
});
```

**Example Consumer (Python):**

```python
from azure.servicebus import ServiceBusClient, ServiceBusReceiver
import json

class EmailNotificationMessage:
    def __init__(self, data):
        self.recipient_email = data['recipientEmail']      # Variable: recipientEmail
        self.subject = data['subject']                     # Variable: subject
        self.message = data['message']                     # Variable: message
        self.sender_name = data.get('senderName')         # Variable: senderName
        self.priority = data['priority']                   # Variable: priority
        self.metadata = data.get('metadata')              # Variable: metadata
        self.received_at = data['receivedAt']             # Variable: receivedAt
        self.correlation_id = data['correlationId']       # Variable: correlationId
        self.status = data['status']                      # Variable: status
        self.queued_at = data['queuedAt']                 # Variable: queuedAt

# Process messages
with ServiceBusClient(...) as client:
    with client.get_queue_receiver("notification-queue") as receiver:
        for msg in receiver:
            body = json.loads(str(msg))
            notification = EmailNotificationMessage(body)

            print(f"To: {notification.recipient_email}")
            print(f"Subject: {notification.subject}")
            print(f"Priority: {notification.priority}")

            # Send email...
            send_email(notification)

            receiver.complete_message(msg)
```

---

## 📊 Response Formats

### Success Response (200 OK)

**User Data Endpoint:**

```json
{
  "success": true,
  "message": "User data received and queued for processing",
  "correlationId": "12345678-1234-1234-1234-123456789abc",
  "receivedAt": "2025-10-28T10:30:00.123Z"
}
```

**Notification Endpoint:**

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

### Validation Error Response (400 Bad Request)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "recipientEmail": ["Invalid email address format"],
    "subject": ["Subject is required"],
    "message": ["Message must be between 1 and 5000 characters"]
  }
}
```

### Error Response (500 Internal Server Error)

```json
{
  "success": false,
  "message": "Failed to process request. Please try again later.",
  "correlationId": "12345678-1234-1234-1234-123456789abc"
}
```

---

## 🔧 Configuration Variables

### For Sending Application (Static Web App)

```javascript
// Configuration object
const API_CONFIG = {
  baseUrl: "https://your-api.azurewebsites.net",
  endpoints: {
    userData: "/api/userdata",
    notification: "/api/notification",
  },
};
```

### For Receiving Application (Function App / Service)

```json
{
  "ServiceBusConnection": "your-servicebus-namespace.servicebus.windows.net",
  "Queues": {
    "UserData": "userdata-queue",
    "Notification": "notification-queue"
  }
}
```

---

## 📚 Quick Reference

### All Input Variables (for Static Web Apps)

**User Data:**

- `name` (string, required)
- `age` (number, required)

**Email Notification:**

- `recipientEmail` (string, required)
- `subject` (string, required)
- `message` (string, required)
- `senderName` (string, optional)
- `priority` (string, optional: "Low"/"Normal"/"High")
- `metadata` (string, optional)

### All Output Variables (for Consuming Applications)

**User Data Queue (`userdata-queue`):**

- `name` (string)
- `age` (number)
- `receivedAt` (ISO 8601 datetime)
- `correlationId` (GUID string)

**Notification Queue (`notification-queue`):**

- `recipientEmail` (string)
- `subject` (string)
- `message` (string)
- `senderName` (string, nullable)
- `priority` (string: "Low"/"Normal"/"High")
- `metadata` (string, nullable)
- `receivedAt` (ISO 8601 datetime)
- `correlationId` (GUID string)
- `status` (string: "Queued"/"Processing"/"Sent"/"Failed")
- `queuedAt` (ISO 8601 datetime)

---

## 🎯 Testing

### Test User Data Endpoint

```powershell
$userData = @{name="John Doe"; age=25} | ConvertTo-Json
Invoke-RestMethod -Uri "https://your-api.azurewebsites.net/api/userdata" -Method Post -Body $userData -ContentType "application/json"
```

### Test Notification Endpoint

```powershell
$notification = @{
  recipientEmail="user@example.com"
  subject="Test Email"
  message="This is a test message"
  priority="Normal"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://your-api.azurewebsites.net/api/notification" -Method Post -Body $notification -ContentType "application/json"
```

---

## 📖 Related Documentation

- Complete setup guide: [README.md](README.md)
- Azure Portal setup: [AZURE-PORTAL-SETUP-GUIDE.md](AZURE-PORTAL-SETUP-GUIDE.md)
- Service Bus tiers: [SERVICE-BUS-TIERS.md](SERVICE-BUS-TIERS.md)
- Function App examples: [FUNCTION-APP-EXAMPLE.md](FUNCTION-APP-EXAMPLE.md)
