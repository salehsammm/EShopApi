using EShopApi.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

/*Add CORS service to the DI container
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:4200")  // The origin of your Angular app
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});*/


// Add services to the container.
builder.Services.AddDbContext<Eshop2DbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

/*Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });*/


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


var app = builder.Build();

// Enable CORS globally
//app.UseCors("AllowLocalhost");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ensure CORS comes before Authorization/Authentication middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

//Scaffold - DbContext "Data Source=(local)\MSSQLSERVER2;Initial Catalog=EShop2_Db;" "TrustServerCertificate=True;Integrated Security=True" Microsoft.EntityFrameworkCore.SqlServer - ContextDir Data - OutputDir Models

// for update :
//Scaffold-DbContext "Data Source=(local)\MSSQLSERVER2;Initial Catalog=EShop2_Db;TrustServerCertificate=True;Integrated Security=True" Microsoft.EntityFrameworkCore.SqlServer -ContextDir Data -OutputDir Models -Force
