using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SA.Application;
using SA.Infrastructure.Persistence;

namespace SA.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton((LiteDatabase)new LiteDatabase("data.db"));
            services.AddSingleton<IDriverRepository, DriverRepository>();
            services.AddSingleton<IInputFileImporterRepository, InputFileImporterRepository>();
            services.AddSingleton<ITripRepository, TripRepository>();
            services.AddSingleton<ITripSummaryRepository, TripSummaryRepository>();
            services.AddTransient<IInputFileProcessor, InputFileProcessor>();
            services.AddTransient<TripSummaryComputation>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
