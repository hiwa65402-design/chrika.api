// Program.cs (کۆدی کۆتایی و ڕاستکراوە)

using Chrika.Api.Data;
using Chrika.Api.Hubs;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders; // <-- گرنگ: ئەمە زیاد بکە
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- بەشی Serviceـەکان ---
builder.Services.AddSignalR();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// زیادکردنی هەموو Serviceـەکان
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
// ... (هەموو Serviceـەکانی تری خۆت لێرە بن)
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFileService, FileService>();
// ... هتد

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* ... کۆدی تۆکن ... */ });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { /* ... کۆدی Swagger ... */ });

// --- بەشی دروستکردنی App ---
var app = builder.Build();

// --- بەشی Middleware (زۆر گرنگ) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// === گرنگترین گۆڕانکاری لێرەدایە ===
// ڕێگەدان بە بەکارهێنانی فایلە ستاتیکەکان لە فۆڵدەری wwwroot
app.UseStaticFiles();

// بۆ ئەوەی ڕێگە بدەین بە بینینی فایلەکانی ناو فۆڵدەری uploads
// ئەگەر wwwroot بوونی نەبوو، دروستی دەکەین
var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwRootPath))
{
    Directory.CreateDirectory(wwwRootPath);
}
var uploadsPath = Path.Combine(wwwRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});
// ==================================

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
// ... (هەر Hubێکی ترت هەیە)

app.Run();
