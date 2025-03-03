using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Telegram.Bot.Callbacks
{
    public class CallbackInvoker : ICallbackInvoker
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CallbackInvoker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Invoke(CallbackQuery query)
        {
            if (query?.Data == null)
                throw new ArgumentNullException(nameof(query), "Пустые данные callback.");

            var parts = query.Data.Split(':');
            if (parts.Length < 2)
                throw new InvalidOperationException($"Некорректный формат callback: {query.Data}.");

            var serviceCode = parts[0];
            var methodCode = parts[1];
            var args = parts.Length > 2 ? parts[2].Split('|') : Array.Empty<string>();

            if (!CallbackCodeStorage.ServiceCodeToType.TryGetValue(serviceCode, out var iType))
                throw new InvalidOperationException($"Не найден интерфейс с кодом {serviceCode}.");

            var methodKey = (iType: iType, methodCode);
            if (!CallbackCodeStorage.MethodInfoByInterfaceAndCode.TryGetValue(methodKey, out var method))
                throw new InvalidOperationException(
                    $"Не найден метод с кодом {methodCode} в интерфейсе {iType.Name}.");

            using (var scope = _scopeFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService(iType);
                if (service == null)
                    throw new InvalidOperationException($"Сервис {iType.Name} не найден в DI-контейнере.");

                var paramInfos = method.GetParameters();
                if (paramInfos.Length == 0 || paramInfos[0].ParameterType != typeof(CallbackQuery))
                    throw new InvalidOperationException(
                        $"Метод {method.Name} должен первым аргументом принимать CallbackQuery.");

                if (paramInfos.Length - 1 != args.Length)
                    throw new InvalidOperationException(
                        $"Методу {method.Name} требуется {paramInfos.Length - 1} аргументов, а пришло {args.Length}.");

                var realParams = new object[paramInfos.Length];
                realParams[0] = query;

                for (var i = 1; i < paramInfos.Length; i++)
                {
                    var targetType = paramInfos[i].ParameterType;
                    realParams[i] = ConvertArg(args[i - 1], targetType);
                }
                
                var result = method.Invoke(service, realParams);
                if (!(result is Task task))
                    throw new InvalidOperationException($"Метод {method.Name} не возвращает Task!");
                
                await task;
            }
        }

        private static object ConvertArg(string arg, Type targetType)
        {
            if (targetType == typeof(int)) return int.Parse(arg, CultureInfo.InvariantCulture);
            if (targetType == typeof(long)) return long.Parse(arg, CultureInfo.InvariantCulture);
            if (targetType == typeof(float)) return float.Parse(arg, CultureInfo.InvariantCulture);
            if (targetType == typeof(double)) return double.Parse(arg, CultureInfo.InvariantCulture);
            if (targetType == typeof(bool)) return arg == "1";
            if (targetType.IsEnum) return Enum.ToObject(targetType, int.Parse(arg, CultureInfo.InvariantCulture));
            return arg;
        }
    }
}