using Microsoft.Extensions.DependencyInjection;

namespace Telegram.Bot.Callbacks
{
    public static class CallbackServiceCollectionExtensions
    {
        public static IServiceCollection AddCallbackInvokerService(this IServiceCollection services)
        {
            CallbackCodeScanner.Init();

            services.AddSingleton<ICallbackInvoker, CallbackInvoker>();

            return services;
        }
    }
}