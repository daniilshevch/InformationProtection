using Swashbuckle.AspNetCore;
using Microsoft.OpenApi.Models;
using InformationProtection1.Services.Lab1;
using InformationProtection1.Services.Lab2;
using InformationProtection1.Services.Lab3;
using InformationProtection1.Services.Lab4;

class Server
{
    public static void PrintList<T>(List<T> list)
    {
        foreach(T item in list)
        {
            Console.WriteLine(item);
        }
    }
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IServiceCollection services = builder.Services;

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontEnd", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("Content-Disposition"));
        });
        services.AddEndpointsApiExplorer();
        services.AddControllers();

        services.AddSwaggerGen(options =>
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Information Protection",
                Version = "lab1",
            })
        );

        services.AddScoped<IRandomSequenceGeneratorService, RandomSequenceGeneratorService>();
        services.AddScoped<IGcdEstimator, GcdEstimator>();
        services.AddScoped<IMd5HashService, Md5HashService>();
        services.AddScoped<ICesaroTesterService, CesaroTesterService>();
        services.AddScoped<PeriodCheckerService>();
        services.AddScoped<RC5EncryptionService>();
        services.AddScoped<RSAService>();

        WebApplication app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
                options.RoutePrefix = string.Empty; 
            });
        }
        app.UseCors("AllowFrontEnd");
        app.MapControllers();
        app.Run();
    }
}