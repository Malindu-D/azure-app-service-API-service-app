# 🔗 Connect App Service to Service Bus via Azure Portal UI

Complete step-by-step guide using only the Azure Portal web interface.

---

## Part 1: Create Service Bus Resources

### Step 1: Create Service Bus Namespace

1. **Navigate to Service Bus**

   - Go to [Azure Portal](https://portal.azure.com)
   - Click **"Create a resource"** (top-left)
   - Search for **"Service Bus"**
   - Click **"Service Bus"** → **"Create"**

2. **Configure Service Bus Namespace**

   - **Subscription**: Select your subscription
   - **Resource Group**: Select existing or create new (e.g., `rg-userdata-api`)
   - **Namespace name**: Enter unique name (e.g., `sb-userdata-12345`)
   - **Location**: Choose region (e.g., `East US`)
   - **Pricing tier**: Select **"Basic"** (cheapest, works for queues) or **"Standard"** (if you need topics)
     - 💡 **Basic tier is sufficient** for this application - we only use queues
     - Standard/Premium only needed for: Topics, Transactions, Duplicate Detection, Auto-forwarding
   - Click **"Review + create"**
   - Click **"Create"**

3. **Wait for Deployment**
   - Wait 2-3 minutes for deployment to complete
   - Click **"Go to resource"**

### Step 2: Create Service Bus Queue

1. **In Service Bus Namespace**

   - In left menu, under **"Entities"**, click **"Queues"**
   - Click **"+ Queue"** (top toolbar)

2. **Configure Queue**

   - **Name**: Enter `userdata-queue`
   - **Max queue size**: Leave default (1 GB)
   - **Message time to live**: Leave default (14 days)
   - **Lock duration**: Leave default (30 seconds)
   - **Enable duplicate detection**: Leave unchecked
   - **Enable dead lettering**: Check this (recommended)
   - **Enable sessions**: Leave unchecked
   - Click **"Create"**

3. **Verify Queue Created**
   - You should see `userdata-queue` in the list
   - Click on it to view details

---

## Part 2: Create and Configure App Service

### Step 3: Create App Service

1. **Create App Service**

   - Click **"Create a resource"**
   - Search for **"Web App"**
   - Click **"Web App"** → **"Create"**

2. **Configure Basics Tab**

   - **Subscription**: Select your subscription
   - **Resource Group**: Use same as Service Bus (e.g., `rg-userdata-api`)
   - **Name**: Enter unique name (e.g., `app-userdata-12345`)
   - **Publish**: Select **"Code"**
   - **Runtime stack**: Select **".NET 8 (LTS)"**
   - **Operating System**: Select **"Linux"** (recommended) or **"Windows"**
   - **Region**: Same as Service Bus (e.g., `East US`)
   - **Pricing plan**: Click **"Create new"** or select existing
     - Name: `asp-userdata-api`
     - Pricing tier: Click **"Explore pricing plans"** → Select **"Basic B1"** or higher
     - Click **"Apply"**

3. **Configure Deployment Tab** (Optional)

   - Skip for now, we'll deploy manually

4. **Configure Monitoring Tab**

   - **Enable Application Insights**: Select **"Yes"**
   - **Application Insights**: Create new or select existing
   - Click **"Review + create"**
   - Click **"Create"**

5. **Wait for Deployment**
   - Wait 2-3 minutes
   - Click **"Go to resource"**

---

## Part 3: Enable Managed Identity

### Step 4: Enable System-Assigned Managed Identity

1. **In App Service**
   - In left menu, under **"Settings"**, click **"Identity"**
2. **System Assigned Tab**

   - **Status**: Toggle to **"On"**
   - Click **"Save"**
   - Click **"Yes"** to confirm
   - Wait for confirmation message

3. **Copy Object (Principal) ID**
   - After enabling, you'll see an **Object (principal) ID** (a GUID)
   - **Copy this ID** - you'll need it in the next step
   - Example: `12345678-1234-1234-1234-123456789abc`

---

## Part 4: Grant Service Bus Permissions

### Step 5: Assign Role to Managed Identity

1. **Navigate to Service Bus Namespace**

   - Go back to your Service Bus namespace
   - Or search for it in the top search bar

2. **Open Access Control (IAM)**
   - In left menu, click **"Access control (IAM)"**
3. **Add Role Assignment**

   - Click **"+ Add"** → **"Add role assignment"**

4. **Select Role (Tab 1)**

   - In the search box, type: **"Azure Service Bus Data Sender"**
   - Click on **"Azure Service Bus Data Sender"** role
   - Click **"Next"**

5. **Assign Access (Tab 2)**

   - **Assign access to**: Select **"Managed identity"**
   - Click **"+ Select members"**
   - In the right panel:
     - **Subscription**: Select your subscription
     - **Managed identity**: Select **"App Service"**
     - You should see your app service listed (e.g., `app-userdata-12345`)
   - Click on your App Service name
   - Click **"Select"** (bottom of panel)
   - Click **"Next"**

6. **Review + Assign (Tab 3)**
   - Review the settings
   - Click **"Review + assign"**
   - Wait for confirmation (green checkmark)

✅ **Your App Service now has permission to send messages to Service Bus!**

---

## Part 5: Configure App Service Settings

### Step 6: Add Application Settings

1. **Navigate to App Service**

   - Go to your App Service resource
   - In left menu, under **"Settings"**, click **"Environment variables"** (or **"Configuration"** in older portal)

2. **Add Service Bus Namespace Setting**

   - Click **"+ Add"** (under Application settings)
   - **Name**: `ServiceBus__Namespace`
   - **Value**: Your Service Bus namespace FQDN
     - Format: `sb-yourname-12345.servicebus.windows.net`
     - To find this:
       1. Go to Service Bus namespace
       2. Copy **"Host name"** from Overview page
       3. Or use format: `[namespace-name].servicebus.windows.net`
   - Click **"OK"**

3. **Add Queue Name Setting**

   - Click **"+ Add"** again
   - **Name**: `ServiceBus__QueueOrTopicName`
   - **Value**: `userdata-queue`
   - Click **"OK"**

4. **Add CORS Setting for Static Web App**

   - Click **"+ Add"** again
   - **Name**: `Cors__AllowedOrigins__0`
   - **Value**: Your static web app URL
     - Example: `https://your-static-web-app.azurestaticapps.net`
     - Or for testing: `http://localhost:3000`
   - Click **"OK"**

5. **Optional: Add More CORS Origins**

   - For multiple origins, add more settings:
   - Click **"+ Add"**
   - **Name**: `Cors__AllowedOrigins__1`
   - **Value**: `http://localhost:5173`
   - Click **"OK"**

6. **Save Configuration**
   - Click **"Apply"** (at the bottom)
   - Click **"Confirm"** when prompted
   - ⚠️ **App will restart automatically**

---

## Part 6: Deploy Your Application

### Step 7: Deploy via Visual Studio Code

1. **Install Azure Extension**

   - Open VS Code
   - Install **"Azure App Service"** extension

2. **Deploy**
   - Right-click on your project folder
   - Select **"Deploy to Web App..."**
   - Select your subscription
   - Select your App Service
   - Confirm deployment

### Step 7 (Alternative): Deploy via ZIP File

1. **Publish Your App**

   - Open PowerShell in your project folder

   ```powershell
   cd c:\Users\malin\OneDrive\Desktop\test-api-service-app
   dotnet publish -c Release -o ./publish
   Compress-Archive -Path ./publish/* -DestinationPath app.zip -Force
   ```

2. **Deploy via Portal**

   - In App Service, go to **"Deployment Center"** (left menu)
   - Click **"FTPS credentials"** tab (or use other methods)

   **OR use Kudu (easier):**

   - In your browser, navigate to:
     ```
     https://[your-app-name].scm.azurewebsites.net/ZipDeployUI
     ```
   - Replace `[your-app-name]` with your App Service name
   - Drag and drop `app.zip` file onto the page
   - Wait for deployment to complete

### Step 7 (Alternative): Deploy via Azure Portal

1. **In App Service**

   - Click **"Deployment Center"** (left menu)
   - Click **"Manual Deployment (Push/Sync)"**
   - Choose deployment source:
     - **Local Git**: Set up Git repository
     - **GitHub**: Connect GitHub repository
     - **Bitbucket**: Connect Bitbucket repository
     - **External Git**: Use external Git URL

2. **For GitHub (example)**
   - Click **"GitHub"**
   - Authorize Azure to access GitHub
   - Select **Organization**, **Repository**, **Branch**
   - Click **"Save"**
   - Push code to GitHub → Auto-deploys to Azure

---

## Part 7: Verify Connection

### Step 8: Test the Configuration

1. **Check App Service is Running**

   - Go to App Service **"Overview"** page
   - Check **Status** shows **"Running"**
   - Copy the **URL** (e.g., `https://app-userdata-12345.azurewebsites.net`)

2. **Test Health Endpoint**

   - In your browser, visit:
     ```
     https://your-app-name.azurewebsites.net/health
     ```
   - You should see: `{"status":"Healthy",...}`

3. **Test API Endpoint**

   - Open PowerShell and run:

   ```powershell
   $url = "https://your-app-name.azurewebsites.net/api/userdata"
   $body = @{name="Test User"; age=25} | ConvertTo-Json

   Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType "application/json"
   ```

   - Expected response:

   ```json
   {
     "success": true,
     "message": "User data received and queued for processing",
     "correlationId": "...",
     "receivedAt": "..."
   }
   ```

4. **Verify Message in Service Bus Queue**
   - Go to **Service Bus namespace**
   - Click **"Queues"** → **"userdata-queue"**
   - Check **"Active message count"**
   - Should show **1** (or more) active messages
   - Click **"Service Bus Explorer"** to view messages

---

## Part 8: Monitor and Troubleshoot

### Step 9: View Logs

1. **Enable Logging**

   - In App Service, go to **"App Service logs"** (left menu)
   - **Application logging**: Turn **"On"**
   - **Level**: Select **"Information"** or **"Verbose"**
   - Click **"Save"**

2. **View Live Logs**

   - Click **"Log stream"** (left menu)
   - You'll see real-time logs
   - Send a test request to see log entries

3. **View Application Insights**
   - Click **"Application Insights"** (left menu)
   - Click on your Application Insights resource
   - Explore:
     - **Live Metrics**: Real-time monitoring
     - **Failures**: View exceptions
     - **Performance**: View request duration
     - **Logs**: Query detailed logs

---

## 🎯 Configuration Summary

After completing all steps, your configuration should look like this:

### Service Bus Namespace

- ✅ Name: `sb-yourname-12345`
- ✅ Tier: Basic (or Standard if you need topics/subscriptions)
- ✅ Queue: `userdata-queue`

### App Service

- ✅ Name: `app-userdata-12345`
- ✅ Runtime: .NET 8
- ✅ Managed Identity: Enabled
- ✅ Application Settings:
  - `ServiceBus__Namespace` = `sb-yourname-12345.servicebus.windows.net`
  - `ServiceBus__QueueOrTopicName` = `userdata-queue`
  - `Cors__AllowedOrigins__0` = `https://your-static-web-app.azurestaticapps.net`

### IAM Role Assignment

- ✅ Role: Azure Service Bus Data Sender
- ✅ Assigned to: App Service Managed Identity
- ✅ Scope: Service Bus Namespace

---

## 🔍 Troubleshooting

### Issue: App shows "Application Error"

**Solution:**

1. Go to **Log stream** to see error details
2. Check that all environment variables are set correctly
3. Verify app was deployed successfully

### Issue: 403 Forbidden when sending to Service Bus

**Solution:**

1. Verify Managed Identity is enabled (Identity → System assigned → Status: On)
2. Check IAM role assignment:
   - Go to Service Bus → Access control (IAM) → Role assignments
   - Search for your App Service name
   - Should have "Azure Service Bus Data Sender" role

### Issue: Messages not appearing in queue

**Solution:**

1. Check Application Insights for exceptions
2. Verify queue name matches exactly: `userdata-queue`
3. Check Service Bus namespace URL is correct (must end with `.servicebus.windows.net`)

### Issue: CORS errors from static web app

**Solution:**

1. Verify CORS origin URL is exact (including `https://` and no trailing slash)
2. Check configuration setting name: `Cors__AllowedOrigins__0` (double underscore)
3. Restart App Service after changing configuration

---

## 📋 Quick Checklist

Use this checklist to verify everything is configured:

- [ ] Service Bus namespace created (Basic or Standard tier)
- [ ] Queue `userdata-queue` created
- [ ] App Service created (.NET 8 runtime)
- [ ] Managed Identity enabled on App Service
- [ ] Role "Azure Service Bus Data Sender" assigned to App Service
- [ ] Application setting `ServiceBus__Namespace` configured
- [ ] Application setting `ServiceBus__QueueOrTopicName` configured
- [ ] Application setting `Cors__AllowedOrigins__0` configured
- [ ] Application deployed to App Service
- [ ] Health endpoint returns healthy status
- [ ] Test message successfully queued in Service Bus

---

## 🎓 Next Steps

1. **Update your static web app** with the App Service URL
2. **Create a Function App** to process messages (see FUNCTION-APP-EXAMPLE.md)
3. **Set up monitoring alerts** in Application Insights
4. **Configure custom domain** (optional)
5. **Enable authentication** (optional)

---

## 📞 Need Help?

- **App Service issues**: Check Log stream and Application Insights
- **Service Bus issues**: Check Metrics and Message count
- **Permission issues**: Verify IAM role assignments
- **Deployment issues**: Check Deployment Center logs

**Documentation:**

- [Azure App Service Docs](https://docs.microsoft.com/azure/app-service/)
- [Azure Service Bus Docs](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Managed Identity Docs](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/)
