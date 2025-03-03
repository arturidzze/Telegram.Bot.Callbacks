using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Callbacks
{
    /// <summary>
    /// Сканер, который проходит по всем сборкам и заполняет <see cref="CallbackCodeStorage"/> сопоставлениями между
    /// интерфейсами (наследниками <see cref="ICallbackable"/>), методами в этих интерфейсах, обрабатывающими
    /// <see cref="CallbackQuery"/> и короткими кодами, либо генерируемыми, либо явно заданными с помощью
    /// <see cref="CallbackCodeAttribute"/>.
    /// </summary>
    internal static class CallbackCodeScanner
    {
        private static bool _isInitialized;

        public static void Init()
        {
            if (_isInitialized) return;
            var callbackInterfaces = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        return Type.EmptyTypes;
                    }
                })
                .Where(t => t.IsInterface && typeof(ICallbackable).IsAssignableFrom(t))
                .ToArray();

            var iCounter = 0;
            foreach (var iType in callbackInterfaces)
            {
                var iAttribute = iType.GetCustomAttributes(typeof(CallbackCodeAttribute), false)
                    .Cast<CallbackCodeAttribute>()
                    .FirstOrDefault();
                var implementsICallbackable = typeof(ICallbackable).IsAssignableFrom(iType);
                if (iAttribute != null && !implementsICallbackable)
                    throw new InvalidOperationException($"Интерфейс '{iType.FullName}' помечен атрибутом " +
                                                        $"CallbackCode, но не реализует ICallbackable.");

                if (!implementsICallbackable) continue;

                var iCode = iAttribute != null
                    ? iAttribute.Name
                    : "S" + iCounter++; // если атрибут задан - используем его значение, иначе - создаем

                if (!CallbackCodeStorage.TypeToServiceCode.TryAdd(iType, iCode))
                    throw new InvalidOperationException($"Интерфейс {iType.FullName} уже зарегистрирован.");
                if (!CallbackCodeStorage.ServiceCodeToType.TryAdd(iCode, iType))
                    throw new InvalidOperationException($"Код '{iCode}' уже используется.");

                var mCounter = 0;
                foreach (var mInfo in iType.GetMethods())
                {
                    var mAttribute = mInfo.GetCustomAttributes(typeof(CallbackCodeAttribute), false)
                        .Cast<CallbackCodeAttribute>()
                        .FirstOrDefault();

                    // проверка метода, если он помечен атрибутом
                    if (mAttribute != null)
                    {
                        if (string.IsNullOrEmpty(mAttribute.Name))
                            throw new InvalidOperationException(
                                $"Метод {mInfo.Name} в интерфейсе {iType.FullName} помечен CallbackCode, " +
                                $"но код - пустая строка.");
                        if (!typeof(Task).IsAssignableFrom(mInfo.ReturnType))
                            throw new InvalidOperationException(
                                $"Метод {mInfo.Name} в интерфейсе {iType.FullName} помечен CallbackCode, " +
                                "но не возвращает Task.");
                        var parameters = mInfo.GetParameters();
                        if (parameters.Length == 0 || parameters[0].ParameterType != typeof(CallbackQuery))
                            throw new InvalidOperationException(
                                $"Метод {mInfo.Name} в интерфейсе {iType.FullName} помечен CallbackCode, " +
                                "но не принимает CallbackQuery в качестве первого аргумента.");
                    }

                    /* если метод не помечен атрибутами и не соответствует условиям
                    (возвращать Task, принимать CallbackQuery) - игнорируем */
                    if (mAttribute == null &&
                        (!typeof(Task).IsAssignableFrom(mInfo.ReturnType) ||

                         mInfo.GetParameters().Length == 0 ||
                         mInfo.GetParameters()[0].ParameterType != typeof(CallbackQuery)))
                    {
                        continue;
                    }

                    // если атрибут задан - используем его значение, иначе - создаем
                    var methodCode = mAttribute != null ? mAttribute.Name : "M" + mCounter++;

                    var key = (iType, methodCode);
                    if (!CallbackCodeStorage.MethodInfoByInterfaceAndCode.TryAdd(key, mInfo))
                        throw new InvalidOperationException(
                            $"Дублирование кода метода {mInfo.Name} в интерфейсе {iType.FullName}. " +
                            $"Код: {methodCode}.");
                    var reverseKey = (iType, mInfo);
                    if (!CallbackCodeStorage.CodeByInterfaceAndMethodInfo.TryAdd(reverseKey, methodCode))
                        throw new InvalidOperationException(
                            $"Дублирование кода метода {mInfo.Name} в интерфейсе {iType.FullName}.");
                }
            }

            _isInitialized = true;
        }
    }
}