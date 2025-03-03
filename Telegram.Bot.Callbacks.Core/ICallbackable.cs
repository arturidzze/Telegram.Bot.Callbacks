using Telegram.Bot.Types;

namespace Telegram.Bot.Callbacks
{
    /// <summary>
    /// Маркерный интерфейс для обработчиков <see cref="CallbackQuery"/>. Все интерфейсы сервисов,
    /// предназначенных для обработки <see cref="CallbackQuery"/>, должны наследоваться от него.
    /// </summary>
    public interface ICallbackable { }
}