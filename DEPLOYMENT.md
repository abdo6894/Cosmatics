Default Admin Credentials:

Username: admin
Password: 12345
Email: admin@cosmatics.com










cd %systemroot%\system32\inetsrv && appcmd stop apppool /apppool.name:"training_cosmatics" && cd C:\Users\Administrator\cosmatics_api &&dotnet publish Cosmatics.csproj -c Release -o C:\inetpub\wwwroot\training_cosmatics && cd %systemroot%\system32\inetsrv && appcmd start apppool /apppool.name:"training_cosmatics"




# Deployment Guide: Windows Server (IIS)

Follow these steps to deploy the **Cosmatics** API to a Windows Server.

## 1. Prerequisites on Windows Server
- **Internet Information Services (IIS)**: Ensure IIS is enabled.
- **.NET 8 Hosting Bundle**: Download and install the [.NET 8 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0) (includes the .NET Runtime and IIS support).
    - *Important*: Restart IIS or the server after installing the bundle.
- **SQL Server**: Ensure SQL Server (Express or Standard) is installed and accessible.

## 2. Publish the Application
You can build locally and copy the files, or build directly on the server if the **.NET SDK** is installed.

### Option A: Build Locally (Standard)
On your local machine:
1.  Run the publish command:
    ```powershell
    dotnet publish -c Release -o ./publish
    ```
2.  Copy the contents of the `./publish` folder to the server (e.g., `C:\inetpub\wwwroot\Cosmatics`).

### Option B: Build on Server
**Requirement**: The server must have the **.NET 8 SDK** installed (check by running `dotnet --version` on the server).
1.  Copy the entire **Project Source Code** to the server.
2.  Open PowerShell on the server and navigate to the project folder.
3.  Run the publish command directly to your IIS folder:
    ```powershell
    dotnet publish Cosmatics.csproj -c Release -o C:\inetpub\wwwroot\Cosmatics
    ```

## 3. Setup Database
### Option A: SQL Server Management Studio (SSMS)
1.  Open **SSMS** and connect to your database instance (e.g., `(local)` or `.`).
2.  **File** > **Open** > **File...** and select `C:\inetpub\wwwroot\Cosmatics\Data\DbSchema.sql`.
3.  Click the **Execute** button (or press F5).

### Option B: Command Line (sqlcmd)
Open PowerShell and run:
```powershell
sqlcmd -S . -i C:\inetpub\wwwroot\Cosmatics\Data\DbSchema.sql
```
*Note: Replace `.` with your server name if it's not the local default instance.*
4.  **Security**: Ensure the user account used by the app (in the connection string) has read/write permissions to this database.

## 5. Configure IIS
1.  Open **IIS Manager**.
2.  Right-click **Sites** -> **Add Website**.
3.  **Site name**: `CosmaticsApi`.
4.  **Physical path**: `C:\inetpub\wwwroot\Cosmatics` (the folder you copied files to).
5.  **Port**: Choose a port (e.g., `8080`) or leave at `80` if using a dedicated domain/IP.
6.  Click **OK**.

## 6. Configure Application Pool
1.  In IIS Manager, click **Application Pools**.
2.  Find `CosmaticsApi`.
3.  Double-click it.
4.  Ensure **.NET CLR Version** is set to `No Managed Code` (since .NET 8 Core runs in a separate process).
5.  Ensure **Managed Pipeline Mode** is `Integrated`.

## 7. Update Configuration
1.  Go to `C:\inetpub\wwwroot\Cosmatics` on the server.
2.  Open `appsettings.json`.
3.  Update the **ConnectionStrings**:
    ```json
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=CosmaticsDb;User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;TrustServerCertificate=True;"
    ```
4.  Update **Jwt:Key** to a strong, secret key for production.

## 8. Test
Open a browser and navigate to `http://localhost:8080/swagger` (if Swagger is enabled for production in `Program.cs`) or test an API endpoint like `http://localhost:8080/api/Products`.

> **Note**: By default, Swagger is often disabled in production (`if (app.Environment.IsDevelopment())`). To see it in production, remove that check in `Program.cs` or set the environment variable `ASPNETCORE_ENVIRONMENT` to `Development` (not recommended for actual production).

## 9. Troubleshooting
### HTTP 500 Error
If you see a 500 error, enable stdout logging in `web.config` (set `stdoutLogEnabled="true"`) and check the `logs` folder.

### Login Failed for User '...'
If you see `Login failed for user 'DOMAIN\User'`, it means the IIS application identity (or your user) needs access to SQL Server.
Run this in PowerShell to grant access:
```powershell
sqlcmd -S . -Q "CREATE LOGIN [YOUR_USER] FROM WINDOWS; USE CosmaticsDb; CREATE USER [YOUR_USER] FOR LOGIN [YOUR_USER]; ALTER ROLE db_owner ADD MEMBER [YOUR_USER];"
```

## 10. Accessing from Outside
To access the API from another computer (not the server itself):

1.  **Find Server IP**: Run `ipconfig` in PowerShell on the server to get the IPv4 Address (e.g., `192.168.1.5` or a public IP).
2.  **Open Firewall Port**:
    *   Open **Windows Defender Firewall with Advanced Security**.
    *   **Inbound Rules** > **New Rule...**
    *   **Port** > TCP > Specific local ports: `8080` (or your chosen port).
    *   **Allow the connection**.
    *   Name it `Cosmatics API`.

**Your Base URL:**
`http://<SERVER_IP>:<PORT>/api`

**Example:**
`http://192.168.1.100:8080/api/Products`

## 11. Setting up a Domain (e.g., api.cosmatics.com)
If you want to use a real name instead of an IP address:

1.  **Buy a Domain**: Purchase a domain name (e.g., `cosmatics.com`) from a registrar like GoDaddy, Namecheap, or AWS Route53.
2.  **Configure DNS**:
    *   Log in to your registrar's dashboard.
    *   Create an **A Record**.
    *   **Host/Name**: `api` (for `api.cosmatics.com`) or `@` (for just `cosmatics.com`).
    *   **Value/Target**: Your Server's **Public IP Address** (Search "what is my ip" on the server).
3.  **Configure IIS Binding**:
    *   Go to **IIS Manager** on your server.
    *   Select your site `CosmaticsApi`.
    *   Click **Bindings...** (in the right panel).
    *   Click **Add**.
    *   **Type**: `http`.
    *   **Host name**: `api.cosmatics.com` (or whatever you set up).
    *   **Port**: `80` (Standard web port) or `8080`.
    *   Click **OK**.

*Note: If you use Port 80, you don't need to type the port in the URL (e.g., `http://api.cosmatics.com/api/Products`).*

## 12. Creating Subdomains
**Yes, you can create unlimited subdomains** (e.g., `admin.cosmatics.com`, `shop.cosmatics.com`) pointing to the same server or different apps.

1.  **DNS**: Create a new **A Record** for the subdomain (e.g., `admin`) pointing to your Server IP.
2.  **IIS**:
    *   Create a **New Website** (for a different app) OR select the **Same Website** (for the same app).
    *   Add a **Binding** with Host name: `admin.cosmatics.com`.

## 13. Finding Existing Domains on Server
To see what domains are currently being used by "old" or other websites on your server:

1.  Open **IIS Manager**.
2.  In the left connection pane, expand **Sites**.
3.  Click on any website name.
4.  In the right "Actions" pane, click **Bindings...**
5.  Look at the **Host Name** column. That is the domain name the site is listening for.

## 14. Your Specific Setup (dataocean-venus.com)
Since you already have `pdi.dataocean-venus.com`, you likely own `dataocean-venus.com`.

To make your new app accessible at `cosmatics.dataocean-venus.com`:

1.  **DNS (Registrar)**: Go to where you manage `dataocean-venus.com`.
    *   Add an **A Record**.
    *   Name: `cosmatics`
    *   Value: Your Server IP (same IP as `pdi.dataocean-venus.com`).
2.  **IIS (Server)**:
    *   Go to **IIS Manager** -> **CosmaticsApi**.
    *   **Bindings...** -> **Add**.
    *   Host name: `cosmatics.dataocean-venus.com`.
    *   Port: `80`.
    
    Now you can access via: `http://cosmatics.dataocean-venus.com/api/Products`

## 15. How to Upload an Update
When you make changes to your code (controllers, logic, models, etc.), follow this process to update the server:

## 15. How to Upload an Update
You do **NOT** need to delete the old folder. `dotnet publish` will overwrite the files, which is what you want.

3.  **Start App Pool**.

### Option C: Manual Deployment (No Scripts)
If you prefer not to use scripts, you **MUST** manually stop the application pool before publishing, otherwise you will get "File Locked" errors.

**Run these commands in Command Prompt (cmd) as Administrator:**

1.  **Stop the App Pool** (Releases the file lock):
    ```cmd
    %systemroot%\system32\inetsrv\appcmd stop apppool /apppool.name:"CosmaticsApi"
    ```

2.  **Publish**:
    ```cmd
    dotnet publish Cosmatics.csproj -c Release -o C:\inetpub\wwwroot\Cosmatics
    ```

3.  **Start the App Pool**:
    ```cmd
    %systemroot%\system32\inetsrv\appcmd start apppool /apppool.name:"CosmaticsApi"
    ```

### Option D: The `app_offline.htm` Method (Most Reliable)
If stopping the App Pool didn't work (or if the file is still locked), this method forces ASP.NET Core to release the files.

1.  **Run this command** to take the app offline:
    ```cmd
    echo "Updating..." > C:\inetpub\wwwroot\Cosmatics\app_offline.htm
    ```

2.  **Wait 5-10 seconds** for the lock to release.

3.  **Publish**:
    ```cmd
    dotnet publish Cosmatics.csproj -c Release -o C:\inetpub\wwwroot\Cosmatics
    ```

4.  **Bring app back online** (Delete the file):
    ```cmd
    del C:\inetpub\wwwroot\Cosmatics\app_offline.htm
    ```

### Option A: Build on Server (Recommended)
You are likely using **Command Prompt (cmd)** instead of PowerShell.

**IMPORTANT: You MUST Right-Click `cmd` or `PowerShell` and select "Run as Administrator" for this to work.**

**If using Command Prompt (cmd):**
Run this single command to **Stop**, **Clean**, **Publish**, and **Start** the application:

```cmd
cd %systemroot%\system32\inetsrv && appcmd stop apppool /apppool.name:"training_cosmatics" && (if exist C:\inetpub\wwwroot\training_cosmatics rmdir /s /q C:\inetpub\wwwroot\training_cosmatics) && dotnet publish "C:\Users\Administrator\cosmatics_api\Cosmatics.csproj" -c Release -o C:\inetpub\wwwroot\training_cosmatics && appcmd start apppool /apppool.name:"training_cosmatics"
```

**If using PowerShell:**
You must run `Import-Module WebAdministration` first, or just use the CMD commands above.

### Option B: Publish Locally & Upload
1.  **Publish Locally**: `dotnet publish ...`
2.  **Stop App Pool**: In IIS Manager.
3.  **Copy/Paste**: Overwrite files in `C:\inetpub\wwwroot\Cosmatics`.
4.  **Start App Pool**.

## 16. JSON Formatting
The API is already configured to return **JSON** by default for all responses. You don't need to do anything extra.
*   Dates: ISO 8601 format (e.g., `2023-10-25T14:30:00Z`).
*   Naming: camelCase (e.g., `productName` instead of `ProductName`).

## 17. FINAL Database Updates (Verification & Countries)
We added:
1.  **Verification**: Users can't login until they verify.
2.  **Country Codes**: A new table for country codes.

**Run this NEW script on the server:**
```cmd
sqlcmd -S . -i C:\inetpub\wwwroot\Cosmatics\Data\Update_Auth_Features.sql
```
*(This script includes the changes for both Verification and Country Codes. If you already ran the Phone Schema update, that's fine, this builds on top of it).*

## 18. Fix 403 Forbidden
If you still have the Admin permission issue:
```cmd
sqlcmd -S . -i C:\inetpub\wwwroot\Cosmatics\Data\Promote_Admin.sql
```

## 19. Profile Updates (Photo)
To add the Profile Photo column to the database, run this NEW script:
```cmd
sqlcmd -S . -i C:\inetpub\wwwroot\Cosmatics\Data\Update_Profile_Schema.sql
```

## 20. Notification Updates
To add the Notifications table to the database, run this NEW script:
```cmd
sqlcmd -S . -i C:\inetpub\wwwroot\Cosmatics\Data\Update_Notifications_Schema.sql
```

## 21. Connecting a Domain (growfet.com)

### Step 1: DNS Configuration (Where you bought the domain)
1.  Log in to your domain registrar (GoDaddy, Namecheap, etc.).
2.  Go to **DNS Management** for `growfet.com`.
3.  Add an **A Record**:
    *   **Type**: A
    *   **Name/Host**: @ (or blank)
    *   **Value/Points to**: `92.205.63.63`
    *   **TTL**: Automatic (or 3600)
4.  (Optional) Add another **A Record** or **CNAME** for `www`:
    *   **Type**: CNAME
    *   **Name**: www
    *   **Value**: `growfet.com`

### Step 2: Configure IIS (On the Server)
1.  Open **IIS Manager**.
2.  Select your site: **Sites -> Cosmatics**.
3.  In the right "Actions" pane, click **Bindings...**.
4.  Click **Add**:
    *   **Type**: http
    *   **IP Address**: All Unassigned (or select your IP)
    *   **Port**: 80
    *   **Host name**: `growfet.com`
5.  Click **OK**.
6.  (Optional) Add another binding for `www.growfet.com` if needed.



