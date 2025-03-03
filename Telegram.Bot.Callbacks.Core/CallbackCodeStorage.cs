using System;
using System.Collections.Concurrent;
using System.Reflection;
using Telegram.Bot.Types;

namespace Telegram.Bot.Callbacks
{
    /// <summary>
    /// Хранит сопоставления коротких кодов с интерфейсами и их методами, обрабатывающими <see cref="CallbackQuery"/>.
    /// </summary>
    public static class CallbackCodeStorage
    {
        /// <summary>
        /// Ключ - короткий код интерфейса, значение - <see cref="Type"/> интерфейса.
        /// </summary>
        public static readonly ConcurrentDictionary<string, Type>
            ServiceCodeToType =
                new ConcurrentDictionary<string, Type>();

        /// <summary>
        /// Ключ - <see cref="Type"/> интерфейса, значение - короткий код интерфейса.
        /// </summary>
        public static readonly ConcurrentDictionary<Type, string>
            TypeToServiceCode =
                new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Ключ - (<see cref="Type"/> интерфейса, короткий код метода), значение - <see cref="MethodInfo"/> метода.
        /// </summary>
        public static readonly ConcurrentDictionary<(Type interfaceType, string methodCode), MethodInfo>
            MethodInfoByInterfaceAndCode =
                new ConcurrentDictionary<(Type interfaceType, string methodCode), MethodInfo>();

        /// <summary>
        /// Ключ - (<see cref="Type"/> интерфейса, <see cref="MethodInfo"/> метода), значение - короткий код метода.
        /// </summary>
        public static readonly ConcurrentDictionary<(Type interfaceType, MethodInfo methodInfo), string>
            CodeByInterfaceAndMethodInfo =
                new ConcurrentDictionary<(Type interfaceType, MethodInfo), string>();
    }
}