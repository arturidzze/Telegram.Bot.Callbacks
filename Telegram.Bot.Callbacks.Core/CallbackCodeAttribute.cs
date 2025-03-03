using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Callbacks
{
    /// <summary>
    /// Атрибут для явного указания короткого кода для интерфейса, унаследованного от <see cref="ICallbackable"/>,
    /// либо для метода в таком интерфейсе, который возвращает <see cref="Task"/>, и принимает в качестве первого
    /// аргумента <see cref="CallbackQuery"/>. Если атрибут применен к интерфейсу или его методу, значение атрибута
    /// используется вместо автоматически генерируемого кода. Если атрибут не указан, библиотека генерирует код для 
    /// таких интерфейсов и методов автоматически (например, "S0", "S1" для интерфейсов и "M0", "M1" для методов).
    /// </summary>
    public sealed class CallbackCodeAttribute : Attribute
    {
        public string Name { get; }
        public CallbackCodeAttribute(string name) => Name = name;
    }
}