// using statements
using Chrika.Api.Data;
using Chrika.Api.Hubs;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; 
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
// --- Section 1: Configure Services ---

// 1. Add DbContext for Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableStringComparisonTranslations()
    )
);

// 2. Add Controllers
builder.Services.AddControllers();

// 3. Register Custom Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IGroupPostService, GroupPostService>();
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ئەمە وا دەکات کە Enumـەکان وەک وشە مامەڵەیان لەگەڵ بکرێت نەک ژمارە
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });





// 4. Add Authentication with JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });

// 5. Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 6. Add Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});


// --- Section 2: Build the Application ---
var app = builder.Build();


// --- Section 3: Configure the HTTP Request Pipeline (Middleware) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
// وا دەکات فایلە ستاتیکەکانی ناو wwwroot (وەک وێنە و ڤیدیۆ) بەردەست بن
app.UseStaticFiles();
// گرنگ: ڕیزبەندی ئەم دووانە نابێت بگۆڕدرێت
app.UseAuthentication(); // یەکەم: پشکنینی تۆکن
app.UseAuthorization();  // دووەم: پشکنینی دەسەڵات
app.MapHub<NotificationHub>("/notificationHub");


app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

app.Run();






//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    try
//    {
//        await dbContext.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS `__EFMigrationsHistory`;");
//        Console.WriteLine("SUCCESS: __EFMigrationsHistory table dropped.");
//    }
//    catch (Exception ex) { Console.WriteLine($"ERROR: {ex.Message}"); }
//}

