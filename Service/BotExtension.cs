using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WpfElmaBot.Service
{
    public static class BotExtension
    {
        public delegate Task Command(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
        static Dictionary<long, Command> _step = new();
        static Dictionary<long, UserCache> _userHandlerData = new();

        #region Регистрация следущей выполняемой команды
        public static void RegisterNextStep(this ITelegramBotClient bot, long userId, Command command)
        {
            ClearStepUser(bot, userId);
            _step.Add(userId, command);
        }

        public static KeyValuePair<long, Command> GetStepOrNull(this ITelegramBotClient bot, long userId)
        {
            return _step.FirstOrDefault(x => x.Key == userId);
        }

        public static void ClearStepUser(this ITelegramBotClient bot, long userId)
        {
            if (HasStep(bot, userId))
            {
                _step.Remove(userId);
            }

        }

        public static bool HasStep(this ITelegramBotClient bot, long userId)
        {
            return _step.ContainsKey(userId);
        }
        #endregion

        #region кэш
        public static void CreateCacheData(this ITelegramBotClient bot, long userId)
        {
            ClearCacheData(bot, userId);
            _userHandlerData.Add(userId, new UserCache());
        }

        public static KeyValuePair<long, UserCache> GetCacheData(this ITelegramBotClient bot, long userId)
        {
            var data = _userHandlerData.FirstOrDefault(x => x.Key == userId);
            if (data.Equals(default(KeyValuePair<long, UserCache>)))
            {
                CreateCacheData(bot, userId);
                return _userHandlerData.FirstOrDefault(x => x.Key == userId);
            }
            else
            {
                return data;
            }
        }

        public static void ClearCacheData(this ITelegramBotClient bot, long userId)
        {
            if (HasCacheData(bot, userId))
            {
                _userHandlerData.Remove(userId);//если ошибка, то удалить класс
            }

        }

        public static bool HasCacheData(this ITelegramBotClient bot, long userId)
        {
            return _userHandlerData.ContainsKey(userId);
        }
        #endregion

        #region Дополнительный функционал
        /// <summary>
        /// Получает идентификатор чата
        /// </summary>
        public static long GetChatId(this Update update)
        {
            if (update.Message != null)
                return update.Message.Chat.Id;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.Message.Chat.Id;

            throw new Exception("Не удалось получить чат ID");
        }
        #endregion

    }

    public class UserCache
    {
        public string Login { get; set; }
        public string Password { get; set; }
        
        public string AuthToken { get; set; }
        public string SessionToken { get; set; }

        public bool StatusAuth  { get; set; }
        public int CountAttempt { get; set; }

        public Dictionary<long, DateTime> LastCommentId { get; set; } = new();
    }
}
