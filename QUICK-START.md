# Quick Start Guide - Email Notification Feature

## What Changed? 🔄

The **notification feature** now connects **directly to your email export app via HTTP** instead of using Service Bus.

❗ **User data feature is unchanged** - still uses Service Bus as before.

---

## Input Variables (from your static web app)

Send POST to: `https://your-api-app.azurewebsites.net/api/notification`

```json
{
  "recipientEmail": "user@example.com", // ✅ Required
  "templateId": "welcome-email", // ❌ Optional
  "dataId": "order-12345" // ❌ Optional
}
```

| Variable         | Required | Description                                  |
| ---------------- | -------- | -------------------------------------------- |
| `recipientEmail` | ✅ Yes   | Email address of recipient                   |
| `templateId`     | ❌ No    | Template ID for your email export app        |
| `dataId`         | ❌ No    | Database record ID for your email export app |

---

## What This API Does

1. ✅ Receives request from your static web app
2. ✅ Validates the email address
3. ✅ Forwards to your email export app via HTTP POST
4. ✅ Returns success/error response

---

## What Your Email Export App Should Do

Your email export app receives HTTP POST at `/api/send-email`:

```json
{
  "recipientEmail": "user@example.com",
  "templateId": "welcome-email",
  "dataId": "order-12345"
}
```

Then your email export app should:

1. Use `dataId` to fetch data from database
2. Use `templateId` to get email template
3. Send email to `recipientEmail` via Azure Communication Services
4. Return HTTP 200 on success

---

## Configuration Required

### In Azure Portal (App Service → Configuration)

Add this new setting:

```
Name:  EmailExportApp__Url
Value: https://your-email-export-app.azurewebsites.net
```

---

## Quick Test

### Option 1: Use cURL

```bash
curl -X POST https://your-api-app.azurewebsites.net/api/notification \
  -H "Content-Type: application/json" \
  -d '{"recipientEmail":"test@example.com","templateId":"welcome","dataId":"123"}'
```

### Option 2: Use the test HTML file

Open `example-static-notification.html` in a browser and fill in the form.

---

## Deployment

### If already deployed:

```powershell
# 1. Update configuration in Azure Portal (add EmailExportApp__Url)
# 2. Deploy new code
dotnet publish -c Release -o ./publish
Compress-Archive -Path ./publish/* -DestinationPath app.zip -Force
az webapp deployment source config-zip --resource-group <rg> --name <app> --src app.zip
```

### If not deployed yet:

```powershell
# 1. Edit deploy.ps1 and set $EMAIL_EXPORT_APP_URL
# 2. Run deployment
.\deploy.ps1
```

---

## ✅ Status

- ✅ Code updated and tested
- ✅ Build successful (0 errors, 0 warnings)
- ✅ User data feature safe (no changes)
- ✅ Ready to deploy

---

## 📚 More Info

- **CHANGES-SUMMARY.md** - Complete summary of changes
- **NOTIFICATION-INTEGRATION-GUIDE.md** - Detailed integration guide
- **VARIABLE-REFERENCE.md** - All variables with examples
- **example-static-notification.html** - Test page

---

## Need Help?

Check the documentation files above for:

- Complete API examples
- Email export app implementation examples
- Architecture diagrams
- Troubleshooting tips
