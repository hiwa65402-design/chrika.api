// using statements to import necessary libraries
using Chrika.Api.Data;
using Microsoft.EntityFrameworkCore;
using Chrika.Api.Services; // Assuming your services are in this namespace

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// --- گۆڕانکارییەکە لێرەدایە ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        // زیادکردنی ئەم هێڵە کێشەکە چارەسەر دەکات
        mySqlOptions => mySqlOptions.EnableStringComparisonTranslations()
    )
);
// 2. Add Controllers
builder.Services.AddControllers();

// 3. Register your custom services (like UserService)
builder.Services.AddScoped<IUserService, UserService>();

// 4. Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. Add CORS to allow requests from your Blazor app
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

var app = builder.Build();
if (app.Environment.IsDevelopment())
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication(); // We will add this later for login
app.UseAuthorization();

app.MapControllers();
app.Run();
