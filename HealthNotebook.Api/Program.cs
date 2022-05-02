using System.Text;
using HealthNotebook.Authentication.Configuration;
using HealthNotebook.DataService.Data;
using HealthNotebook.DataService.IConfiguration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Update the Jwt config from the settings
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddControllers();

// Enable Api versionaning
builder.Services.AddApiVersioning(opt =>
{
  // Provide to the client the different Api version we have
  opt.ReportApiVersions = true;
  // this allow the api to automatically provide a default version
  opt.AssumeDefaultVersionWhenUnspecified = true;
  // Define the default Api version
  opt.DefaultApiVersion = ApiVersion.Default;
});

// Getting the secret from the config
var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]);

var tokenValidationParameters = new TokenValidationParameters
{
  ValidateIssuerSigningKey = true,
  IssuerSigningKey = new SymmetricSecurityKey(key),
  ValidateIssuer = false, // ToDo Update
  ValidateAudience = false, // ToDo Update
  RequireExpirationTime = false, // ToDoUpdate
  ValidateLifetime = true
};

// Injecting into our DI container
builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(option =>
{
  option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt =>
{
  jwt.SaveToken = true;
  jwt.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
  options.SignIn.RequireConfirmedAccount = true)
  .AddEntityFrameworkStores<AppDbContext>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
