using Practice.Models;
using Practice.Controllers;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using Practice.Helpers;
using Practice.Configuration;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Connect to db
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddControllers();
/*builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Products API",
        Version = "v1",
        Description = "API for product management"
    });
*//*    c.UseInlineDefinitionsForEnums();
    c.SchemaGeneratorOptions.UseInlineDefinitionsForEnums = true;
    c.SchemaGeneratorOptions.UseAllOfForInheritance = true;*//*
});*/

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://musical-cactus-08f1b2.netlify.app");
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });

});

var app = builder.Build();

/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

app.UseCors();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();


app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
