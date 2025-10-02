# Runbook

This runbook covers day-to-day ops and the fastest fixes for our ASP.NET Core MVC + SignalR app on Azure with Microsoft Entra External ID (OIDC), Azure SQL, Blob Storage, and Application Insights.

---

## 0) Smoke Test (Post-deploy or after incident)
1. Browse `lms-msg-dev-beawckefbjapcpd0.eastus2-01.azurewebsites.net` → redirects to Entra → sign in with a test user.
2. Landing page shows **courses**. If none, confirm fallback shows all courses otherwise check auto-enroll.
3. Open a course → **Class List** renders users (names left, emails right).
4. Open a direct message → send a message → verify it appears in both browsers (normal + incognito).
5. Upload a small image/PDF → thumbnail/link renders → download works.
6. Logout from header → re-login works.

---

## 1) Daily/Weekly Tasks
**Daily**
- App Insights → **Failures**: top exceptions, new spikes.
- App Insights → **Availability** (if configured): check probes.
- Azure SQL DTU/CPU → ensure < 70% sustained.

**Weekly**
- App Insights → **Performance**: slowest requests; investigate > P95 endpoints.
- SignalR connections/traffic (Service metrics).
- Storage lifecycle: confirm old uploads are moving to Cool tier.

---

## 2) Config Cheat-Sheet (App Service)
Application Settings (keys use `__` on Azure):
- `AzureAd__Instance=https://login.microsoftonline.com/`
- `AzureAd__Domain=<tenant>.onmicrosoft.com`
- `AzureAd__TenantId=<GUID>`
- `AzureAd__ClientId=<GUID>`
- `AzureAd__ClientSecret=<secret>`
- `ASPNETCORE_ENVIRONMENT=Production`
- `Admin__AutoEnrollAllCourses=true`
- `RUN_AZURE_INIT=false`  *(set `true` only for first boot to run EF migrate + seed, then set back to `false` and restart)*

**Connection strings** (Configuration → Connection strings):
- `DefaultConnection` **(SQLAzure)** = Azure SQL connection string
- `AzureBlobStorage` **(Custom)** = Blob connection string
- `AzureSignalR` **(Custom)** = SignalR connection string

After edits: **Save** → **Restart** the app.

---

## 3) Common Ops

### Scale
- App Service Plan → **Scale up/out**. For spikes: go S1 and/or add instances (Not directly applicable for small-scale portfolio).
- SignalR Service → bump from F1 to S1 if concurrent connections approach limit (not planned).

### Restart / Clear stuck state
- App Service → **Restart**.
- If auth/session weirdness: **Logout** then hard refresh. If necessary, clear cookies for site.

### EF Core Migration (manual)
From the web project folder (local or in a pipeline agent):
```powershell
dotnet ef database update
```
(Prod uses `RUN_AZURE_INIT=true` once to run `Migrate();` then set back to false.)

### Rotate Keys
- Regenerate **Storage** or **SignalR** keys → update App Service **Connection strings** → **Restart**.
- **ClientSecret**: create new secret in Entra → update App Settings → **Restart**.

---

## 4) SQL Quick Fixes (SSMS or Azure SQL Query Editor)

**Check courses, users, enrollments**
```sql
SELECT COUNT(*) AS Courses FROM dbo.Courses;
SELECT TOP 6 CourseId, Name FROM dbo.Courses ORDER BY CourseId;

SELECT TOP 10 UserId, Name, Email, ExternalObjectId, UserType
FROM dbo.Users ORDER BY UserId DESC;

-- Replace with my/test email to inspect
DECLARE @uid INT = (SELECT TOP 1 UserId FROM dbo.Users WHERE Email='you@yourdomain.com');
SELECT @uid AS UserId;
SELECT * FROM dbo.Enrollments WHERE UserId=@uid;
```

**Enroll everyone (prod-safe, idempotent)**
```sql
WITH Students AS (
  SELECT U.UserId FROM dbo.Users U WHERE ISNULL(U.UserType,'student') <> 'instructor'
),
Courses AS ( SELECT CourseId FROM dbo.Courses )
INSERT INTO dbo.Enrollments (UserId, CourseId)
SELECT S.UserId, C.CourseId
FROM Students S CROSS JOIN Courses C
WHERE NOT EXISTS (SELECT 1 FROM dbo.Enrollments E WHERE E.UserId=S.UserId AND E.CourseId=C.CourseId);
```

**Purge legacy (local) users** (keeps System Instructor)
```sql
BEGIN TRAN;
DECLARE @SystemInstructorId INT = (
  SELECT TOP 1 UserId FROM dbo.Users WHERE Email='instructor@local' OR UserType='instructor' ORDER BY UserId
);
IF OBJECT_ID('tempdb..#LegacyUserIds') IS NOT NULL DROP TABLE #LegacyUserIds;
SELECT UserId INTO #LegacyUserIds
FROM dbo.Users
WHERE (ExternalObjectId IS NULL OR LTRIM(RTRIM(ExternalObjectId))='') AND (UserId <> @SystemInstructorId);

DELETE M FROM dbo.Messages M
WHERE M.SenderId IN (SELECT UserId FROM #LegacyUserIds) OR M.ReceiverId IN (SELECT UserId FROM #LegacyUserIds);
DELETE E FROM dbo.Enrollments E WHERE E.UserId IN (SELECT UserId FROM #LegacyUserIds);
IF OBJECT_ID('dbo.ConversationParticipants','U') IS NOT NULL
  DELETE CP FROM dbo.ConversationParticipants CP WHERE CP.UserId IN (SELECT UserId FROM #LegacyUserIds);
DELETE U FROM dbo.Users U WHERE U.UserId IN (SELECT UserId FROM #LegacyUserIds);
COMMIT;
```

---

## 6) When “Class List is Empty”
Checklist:
1. **Courses exist?** `SELECT COUNT(*) FROM dbo.Courses;` (should be 6)
2. **User created on sign-in?** Check `dbo.Users` for your email (has `ExternalObjectId`).
3. **Auto-enroll enabled?** App Setting `Admin__AutoEnrollAllCourses=true` and app restarted.
4. **Controller fallback present?** If user has zero enrollments, show all courses:
   ```csharp
   var myCourses = await GetStudentCourses(userId);
   if (myCourses == null || myCourses.Count == 0)
       myCourses = await _context.Courses.AsNoTracking().ToListAsync();
   ```
5. To unblock immediately, run **Enroll everyone** SQL (above), then fix the root cause.

---

## 7) SignalR “No Messages / Not Live”
- Check SignalR **service connection string** present.
- If running locally vs Azure, my Program.cs already swaps to Azure SignalR when not Development.
- App Insights → **Dependencies**: look for failed SignalR negotiate/send.

---

## 8) Storage (Uploads)
- Verify **container** exists and app has a valid connection string.
- Lifecycle policy moves old files to Cool tier.

---

## 9) Logging & Diagnostics
**Log Stream**
- App Service → **Log stream** → capture top exception lines for 500s.

---

## 10) On-Call Playbook (first 15 minutes)
1. **User impact?** Check App Insights Failures + Availability.
2. **Auth errors?** Look for AADSTS in traces; verify App Settings (`ClientSecret`, redirect URI).
3. **DB errors?** Check `DefaultConnection`, run health query on SQL (counts).
4. **Empty screens?** Run **Enroll everyone** SQL to unblock; then fix auto-enroll setting.
5. **Restart App Service** if stuck.
6. If still broken, capture stack from **Log stream** and create an incident note with:
   - Time window, request IDs, top exception, last successful deploy ID.
   - Mitigation attempted.

---

## 11) Deploy / Rollback
- Deploy via my pipeline to App Service.
- Keep last known good package; App Service → **Deployment Center** supports quick rollback.
- After deploy, run **Smoke Test** (section 0).

---

## 12) User Admin (Sysadmin Flow)
- Entra → **Users**: create **Member** or invite **Guest**.
- Optional: Assign app role/group (future).
- User signs in once → `UserIdClaimTransformer` creates local user + auto-enroll (if enabled).
- To force course visibility now: run **Enroll everyone**.
