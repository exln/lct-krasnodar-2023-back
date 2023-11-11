using System.Text;
using LctKrasnodarWebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApiDbContext>(options => options
    .UseNpgsql(connectionString, b => b.MigrationsAssembly("LctKrasnodarWebApi")));

builder.Services.AddCors(); // Add CORS support here

builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration.GetSection("AppSettings:Token").Value!))
    };
});

var app = builder.Build();


// Configure the HTTP request pipeline.
var BasePath = "/api";
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        var basepath = "/api"; 
        options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            var paths = new OpenApiPaths();
            foreach (var path in swaggerDoc.Paths)
            {
                paths.Add(
                    $"{basepath}{path.Key}", 
                    path.Value
                    );
            } swaggerDoc.Paths = paths;
        });
    }); 
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseRouting(); // Add UseRouting before UseCors 
app.UseCors(builder =>
    builder
        .WithOrigins("http://localhost:3000", "http://localhost:8080", "http://localhost:8443",
            "https://lct-krasnodar-2023-frontend-barolad.vercel.app/", "https://lct.unitrip.ru")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod()
);

app.UsePathBase("/api"); // Add UsePathBase here

app.UseAuthorization();

app.MapControllers();

app.Run();