

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders; 
using OfficeOpenXml;
using System.Text.Json;
using top.ebiz.helper;
using top.ebiz.service;
using top.ebiz.service.Service.AzureAD;
using top.ebiz.service.SwaggerHeader;

var builder = WebApplication.CreateBuilder(args); 

// การตั้งค่า CORS เพื่ออนุญาตเฉพาะโดเมนที่ระบุ
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(
             "https://dev-e-biztravel.thaioilgroup.com"
             , "https://qas-e-biztravel.thaioilgroup.com"
             , "https://e-biztravel.thaioilgroup.com"
             , "https://outlook.office365.com"
             , "https://localhost:7098"
             , "https://localhost:7052"
             , "https://localhost:4200"
             , "http://localhost:4200"
             , "http://localhost:4800"
             , "http://localhost:4405"
             )
            .AllowCredentials()
            .WithHeaders("Content-Type", "X-CSRF-TOKEN")
            .WithMethods("GET", "POST");
    });
});

// ปรับให้ใช้ AppEnvironment ดึง ConnectionStrings
builder.Services.AddDbContext<TOPEBizEntities>(options =>
    options.UseOracle(AppEnvironment.GeteConnectionString())
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine)
);
builder.Services.AddDbContext<TOPEBizCreateTripEntities>(options =>
    options.UseOracle(AppEnvironment.GeteTripConnectionString())
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine)
);
builder.Services.AddDbContext<TOPEBizTravelerProfileEntitys>(options =>
    options.UseOracle(AppEnvironment.GeteTravelerProfileConnectionString())
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine)
);

// เพิ่มการตั้งค่า HttpClient สำหรับ SSL ที่ไม่เชื่อถือ
builder.Services.AddHttpClient("HttpClientWithSSLUntrusted").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
});

// การตั้งค่า Kestrel เพื่อจัดการขนาดไฟล์คำขอ
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB

});

// การตั้งค่า Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";  // ชื่อ Header ที่ใช้ในการส่ง CSRF token
    options.Cookie.Name = "X-CSRF-TOKEN"; // ชื่อ Cookie ที่ใช้เก็บ CSRF token
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ใช้เฉพาะกับ HTTPS เท่านั้น
    options.Cookie.SameSite = SameSiteMode.None; // อนุญาตให้ใช้ข้ามไซต์
    options.Cookie.HttpOnly = false; // อนุญาตให้เข้าถึง Cookie ผ่าน JavaScript
});

// การตั้งค่า Cookie Policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.HttpOnly = HttpOnlyPolicy.None; // อนุญาตให้เข้าถึง Cookie ผ่าน JavaScript
    options.Secure = CookieSecurePolicy.Always; // ใช้เฉพาะกับ HTTPS
    options.MinimumSameSitePolicy = SameSiteMode.None; // อนุญาตให้ใช้ข้ามไซต์
});

// เพิ่มการตั้งค่า Controllers กับ Views (รวม ViewFeatures)
builder.Services.AddControllersWithViews(); // เปลี่ยนจาก AddControllers() เป็น AddControllersWithViews()

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // รองรับ camelCase
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // ข้ามฟิลด์ null
    });

// เพิ่มการตั้งค่า Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
        {
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Please enter 'Bearer' followed by your JWT",
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // เพิ่ม HeaderFilter สำหรับ CSRF
    c.OperationFilter<SwaggerHeaderFilter>();
});

// เพิ่มการตั้งค่า Directory Browser และการจัดการไฟล์
builder.Services.AddDirectoryBrowser();

// เพิ่มบริการ IHttpContextAccessor เพื่อให้สามารถเข้าถึง HttpContext ได้ในที่อื่น ๆ
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // ขนาดสูงสุด 100 MB
});
builder.Services.AddHttpContextAccessor();

// ตั้งค่าให้ใช้ ExcelPackage 
var envName = AppEnvironment.GeteServerWebString();
if (envName == "PROD")
{
    var epplusKey = Environment.GetEnvironmentVariable("EPPlusLicenseKey")
                    ?? builder.Configuration["EPPlusLicenseKey"];

    try
    {
        if (!string.IsNullOrWhiteSpace(epplusKey))
        { 
            ExcelPackage.License.SetCommercial(epplusKey);
            LoggerFile.WarnLog("EPPlus license key has been set successfully.");
        }
        else
        { 
            LoggerFile.WarnLog("EPPlus license key is missing or empty.");
        }
    }
    catch (Exception ex)
    { 
        LoggerFile.WarnLog("Failed to set EPPlus license.", ex);
    }
}



GraphMicrosoftClient.InitializeGraph();

var app = builder.Build();

// ลำดับของ middleware pipeline  
// 1. เปิดใช้งานการเปลี่ยนเส้นทางไปยัง HTTPS
app.UseHttpsRedirection();

// 2. ให้บริการ Static Files ก่อน เพื่อให้สามารถเข้าถึงไฟล์สาธารณะได้ เช่น CSS, JS, รูปภาพ  
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "",// ใช้ Root Path (เข้าถึงโดยตรง)
    ServeUnknownFileTypes = true, // รองรับไฟล์ทุกประเภท
    DefaultContentType = "application/octet-stream", // MIME type เริ่มต้นสำหรับไฟล์ที่ไม่รู้จัก
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings =
        {
            [".jpg"] = "image/jpeg",
            [".pdf"] = "application/pdf", // เพิ่ม MIME type สำหรับ PDF
            [".txt"] = "text/plain"    ,   // เพิ่ม MIME type สำหรับ TXT 
            [".zip"] = "application/zip",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        }
    }
});


// 3. เปิดใช้งาน Routing ก่อน เพื่อจัดการเส้นทางของการร้องขอ
app.UseRouting();

// 4. เปิดใช้งาน CORS หลังจากการใช้ Routing แต่ก่อนการ Authentication
app.UseCors(MyAllowSpecificOrigins);

// 5. เปิดใช้งาน CSRF Middleware
app.Use(async (context, next) =>
{
    var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

    if (HttpMethods.IsPost(context.Request.Method) ||
        HttpMethods.IsPut(context.Request.Method) ||
        HttpMethods.IsDelete(context.Request.Method))
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context); // ตรวจสอบ CSRF token
        }
        catch (Exception ex) { LoggerFile.WarnLog("WarnLog", ex); }

    }

    await next(); // เรียกใช้คำขอต่อไปยัง Controller
});

// 6. เปิดใช้งาน Authentication และ Authorization
app.UseAuthentication();
app.UseAuthorization();

// ตรวจสอบว่าโฟลเดอร์ Logs มีอยู่หรือไม่ และตั้งค่าให้บริการไฟล์
string logPath = app.Configuration["appsettings:folder_Logs"] ?? "";
if (Directory.Exists(logPath))
{
    // ให้บริการไฟล์จากโฟลเดอร์ "folder_Log" ผ่าน URL "/log"
    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(logPath, "folder_Log")),
        RequestPath = "/log",
        EnableDirectoryBrowsing = false // ปิดการแสดงรายการไฟล์
    });

    // ให้บริการไฟล์จากโฟลเดอร์ "pic" ผ่าน URL "/pic"
    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(logPath, "pic")),
        RequestPath = "/pic",
        EnableDirectoryBrowsing = false // ปิดการแสดงรายการไฟล์
    });
}

// เปิดการใช้งาน Swagger และกำหนดเส้นทาง 
var envNameSwagger = AppEnvironment.GeteServerWebString();
if (envNameSwagger != "PROD")
{
    app.UseSwagger(o =>
    {
        o.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
        c.DefaultModelsExpandDepth(-1);
    });
}


// 7. กำหนด Endpoint Routing เป็นลำดับสุดท้ายเพื่อจับการร้องขอ 
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});


// กำหนดเส้นทางเริ่มต้นของ Controller
app.MapDefaultControllerRoute(
    );

// เริ่มต้นการทำงานของแอป
app.Run();
