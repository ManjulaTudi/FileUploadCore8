using FileUploadCore.Data;
using FileUploadCore.Services;
using log4net.Config;
using log4net;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddAntiforgery();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DbContextClass>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddLog4net();

//builder.Services.Configure<FormOptions>(options =>
//{
//    // Set the limit to 256 MB
//    options.ValueLengthLimit = int.MaxValue;
//    options.MultipartBodyLengthLimit = 268435456;
//    options.MultipartBoundaryLengthLimit = int.MaxValue;
//    options.BufferBodyLengthLimit = int.MaxValue;
    
//});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();
app.MapControllers();
app.UseFileUploadValidator();

app.Run();
public static class Log4netExtensions
{
    public static void AddLog4net(this IServiceCollection services)
    {
        XmlConfigurator.Configure(new FileInfo("log4net.config"));
        services.AddSingleton(LogManager.GetLogger(typeof(Program)));
    }
}
