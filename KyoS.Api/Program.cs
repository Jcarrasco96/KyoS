using KyoS.Web.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer("name=KyoSApiConnection"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//system culture
var cultureInfo = new CultureInfo("en-US");
cultureInfo.DateTimeFormat.DateSeparator = "/";
cultureInfo.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
// Configure the Localization middleware
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new List<CultureInfo>
    {
        cultureInfo,
    },
    SupportedUICultures = new List<CultureInfo>
    {
        cultureInfo,
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
