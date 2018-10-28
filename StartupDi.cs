using System;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTraining
{
    public class StartupDi
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IVmWordMapper, VmWordMapper>();
        }
    }
}
