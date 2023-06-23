using DiscGolfCourseApi.Models;
using DiscGolfCourseApi.Repositories;
using DiscGolfCourseApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DiscgolfDbContext>();
builder.Services.AddScoped<Repository>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<DetailsRepository>();
builder.Services.AddScoped<PdgaCourseScraper>();
builder.Services.AddScoped<GeographyRepository>();

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

const string myAllowOrigins = "_myAllowOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:7000").AllowAnyMethod().AllowAnyHeader();
        policy.WithOrigins("https://localhost:7000").AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors(myAllowOrigins);

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();