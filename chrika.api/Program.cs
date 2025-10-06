// Program.cs (کۆدی کۆتایی و تەواوکراو - وەشانی ٢)

using Chrika.Api.Data;
using Chrika.Api.Hubs;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- بەشی Serviceـەکان (وەک خۆی) ---
builder.Services.AddSignalR();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableStringComparisonTranslations()
    )
);
// ... (هەموو Serviceـەکانی تر وەک خۆیان)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFileService, FileService>();
// ... هتد

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => { /* ... */ });
builder.Services.AddCors(options => { options.AddPolicy("AllowAll", policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); }); });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { /* ... */ });

// --- بەشی دروستکردنی App ---
var app = builder.Build();

// --- بەشی Middleware (گۆڕانکاری گرنگ) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// === گۆڕانکاری سەرەکی لێرەدایە ===
// دڵنیابوونەوە لە بوونی فۆڵدەری wwwroot
var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwRootPath))
{
    Directory.CreateDirectory(wwwRootPath);
}

// بەکارهێنانی فایلە ستاتیکەکان لە wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(wwwRootPath),
    RequestPath = "" // بەتاڵ مانای وایە ڕاستەوخۆ لە ڕەگی سایتەکەوە دەست پێدەکات
});
// ==================================

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
