using Microsoft.Extensions.DependencyInjection;

namespace ERP_AI.Core
{
    public static class ViewModelLocator
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;

        public static void Init(ServiceCollection services)
        {
            ServiceProvider = services.BuildServiceProvider();
        }

        public static T Get<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}
