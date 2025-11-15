using AutoMapper;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Utils.DtoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace ASI.Basecode.WebApp
{
    // AutoMapper configurations
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configure auto mapper
        /// </summary>
        private void ConfigureAutoMapper()
        {
            // Use MapConfigurationFactory to scan and discover all ICustomMap implementations
            var mapper = MapConfigurationFactory.Scan<UserViewModel>(
                assemblyFilter: a => a.FullName.StartsWith("ASI.Basecode")
            );
            
            this._services.AddSingleton<IMapper>(mapper);
        }
    }
}
