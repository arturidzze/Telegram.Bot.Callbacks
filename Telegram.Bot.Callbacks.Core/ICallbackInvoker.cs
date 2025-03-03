using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Callbacks
{
    /// <summary>
    /// Выполняет вызов обработчиков <see cref="CallbackQuery"/>
    /// </summary>
    public interface ICallbackInvoker
    {
        /// <summary>
        /// Разбирает <see cref="CallbackQuery"/>, находит соответствующий обработчик и выполняет его.
        /// </summary>
        /// <param name="query">Объект <see cref="CallbackQuery"/>.</param>
        /// <exception cref="ArgumentNullException">Если query или его данные null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Если формат callback-строки неверный, сервис или метод не найден,
        /// или аргументы метода не соответствуют полученной строке.
        /// </exception>
        Task Invoke(CallbackQuery query);
    }
}