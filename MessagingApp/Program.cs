using System;
using System.Security.Claims;
using Azure.Identity;
using Azure.Storage.Blobs;
using MessagingApp.Controllers;
using MessagingApp.Data;
using MessagingApp.Models;
using MessagingApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// Add services to the container.
// =====================================
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);

        // FORCE authorization code flow (not implicit id_token)
        options.ResponseType = "code";
        options.UsePkce = true;          // default true in recent versions, set explicitly to be clear
        options.SaveTokens = true;       // optional: keep tokens in auth cookie if you ever need them
    });

builder.Services.AddAuthorization();

// Blob Storage:
// Keep your existing connection-string approach (works with local emulator or prod).
// You can move to Managed Identity later without touching the app code that consumes IFileStorageService.
builder.Services.AddSingleton(sp =>
{
    var cs = builder.Configuration.GetConnectionString("AzureBlobStorage")
             ?? builder.Configuration["Azure:StorageConnectionString"]
             ?? builder.Configuration["Azure:Storage:ConnectionString"];

    // For Azurite/local emulator, you can use: "UseDevelopmentStorage=true"
    return new BlobServiceClient(cs);
});

// Use Azure-backed implementation (remove the Local one)
builder.Services.AddTransient<IFileStorageService, AzureBlobStorageService>();

// (Optional but recommended) raise body size limits for large PDFs/images
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50L * 1024 * 1024; // 50 MB
});

// Configure SQL Server (still using your connection string).
// Later, you can flip to Entra auth / Managed Identity by changing only the connection string.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// SignalR with fallback for local dev
var asrs = builder.Configuration.GetConnectionString("AzureSignalR")
          ?? builder.Configuration["Azure:SignalR:ConnectionString"];

if (!string.IsNullOrWhiteSpace(asrs) && !builder.Environment.IsDevelopment())
{
    builder.Services.AddSignalR().AddAzureSignalR(asrs);
}
else
{
    builder.Services.AddSignalR();
}

/*
 * Claims transformer:
 * - Ensures a local User row exists on first Entra sign-in (creates it if missing).
 * - Adds "UserId" claim so existing controllers/hub continue to work unchanged.
 * - Optionally auto-enrolls new users into all existing courses for easy demos.
 */
builder.Services.AddTransient<IClaimsTransformation, UserIdClaimTransformer>();

var app = builder.Build();

// =====================================
// Configure the HTTP request pipeline.
// =====================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map the SignalR hub (now requires auth)
app.MapHub<MessagingApp.Hubs.ChatHub>("/chathub").RequireAuthorization();

/*
 * Database initialization:
 * - Keep automatic migrations.
 * - Remove people/password seeding.
 * - Optionally seed Courses only, so new Entra users can be enrolled quickly (auto or manual).
 */
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (env.IsDevelopment())
    {
        // Always ensure schema + seed Courses when running locally
        db.Database.Migrate();
        SeedCoursesIfEmpty(db);
    }
    else if (Environment.GetEnvironmentVariable("RUN_AZURE_INIT") == "true")
    {
        // Same behavior you wanted for Azure
        db.Database.Migrate();
        SeedCoursesIfEmpty(db);
    }
}


app.Run();


// =====================
// Helpers & Transformers
// =====================

void SeedCoursesIfEmpty(AppDbContext db)
{
    // 1) Ensure an instructor exists (FK target)
    var instructor = db.Users.FirstOrDefault(u => u.UserType == "instructor");
    if (instructor == null)
    {
        instructor = new MessagingApp.Models.User(
            name: "System Instructor",
            email: "instructor@local",
            password: ""  // not used with Entra, but column is NOT NULL
        )
        {
            UserType = "instructor",
            ExternalObjectId = null
        };
        db.Users.Add(instructor);
        db.SaveChanges(); // get real UserId
    }

    // 2) Seed courses only if none exist (NOTE: Course has Name, not Title)
    if (!db.Courses.Any())
    {
        var i = instructor.UserId;
        db.Courses.AddRange(
            new MessagingApp.Models.Course("Web Programming", i),
            new MessagingApp.Models.Course("C# Programming", i),
            new MessagingApp.Models.Course("Mobile Development", i),
            new MessagingApp.Models.Course("User Experience", i),
            new MessagingApp.Models.Course("Programming Concepts II", i),
            new MessagingApp.Models.Course("Database:SQL", i)
        );
        db.SaveChanges();
    }
}


/*
 * UserIdClaimTransformer
 * - Creates local user record mapped to Entra account (by OID/email).
 * - Injects "UserId" (and "UserType") claims expected by your existing controllers/hub.
 * - Optionally auto-enrolls newly-created users into existing courses for demo convenience.
 */
public class UserIdClaimTransformer : IClaimsTransformation
{
    private readonly IServiceProvider _sp;

    public UserIdClaimTransformer(IServiceProvider sp) => _sp = sp;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!(principal.Identity?.IsAuthenticated ?? false)) return principal;
        if (principal.HasClaim(c => c.Type == "UserId")) return principal;

        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Extract OIDC claims
        var oid = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("oid");
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var name = principal.Identity?.Name ?? principal.FindFirstValue("name") ?? email ?? "User";

        // Find or create local user row
        MessagingApp.Models.User? user = null;

        if (!string.IsNullOrEmpty(oid))
            user = await db.Users.FirstOrDefaultAsync(u => u.ExternalObjectId == oid);

        if (user == null && !string.IsNullOrEmpty(email))
            user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        var isNew = false;

        if (user == null)
        {
            user = new MessagingApp.Models.User(name, email ?? "", password: "", userType: "student")
            {
                ExternalObjectId = oid
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
            isNew = true;
        }
        else
        {
            // ensure linkage is stored
            if (string.IsNullOrEmpty(user.ExternalObjectId) && !string.IsNullOrEmpty(oid))
            {
                user.ExternalObjectId = oid;
                await db.SaveChangesAsync();
            }
        }

        var autoEnroll = config.GetValue<bool?>("Admin:AutoEnrollAllCourses") ?? false;
        if (autoEnroll)
        {
            var existingCourseIds = await db.Courses.Select(c => c.CourseId).ToListAsync();
            var already = await db.Enrollments
                                  .Where(e => e.UserId == user.UserId)
                                  .Select(e => e.CourseId)
                                  .ToListAsync();

            // Enroll if they're new OR they currently have nothing
            if (isNew || already.Count == 0)
            {
                var toAdd = existingCourseIds.Except(already).ToList();
                if (toAdd.Count > 0)
                {
                    foreach (var cid in toAdd)
                        db.Enrollments.Add(new MessagingApp.Models.Enrollment(user.UserId, cid));
                    await db.SaveChangesAsync();
                }
            }
        }


        // Add claims consumed by your app today
        var id = (ClaimsIdentity)principal.Identity!;
        id.AddClaim(new Claim("UserId", user.UserId.ToString()));
        id.AddClaim(new Claim("UserType", user.UserType ?? "student"));

        // Lightweight admin: mark role if email is in config list
        var adminEmails = config.GetSection("Admin:Emails").Get<string[]>() ?? Array.Empty<string>();
        if (!string.IsNullOrEmpty(email) && adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
        {
            id.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        }

        return principal;
    }
}
