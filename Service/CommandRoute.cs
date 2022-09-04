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
    public class CommandRoute
    {
        #region кнопки
        public const string AUTH = "/authorization";
        public const string MENU = "/menu";
        public const string START = "/start";


        //public const string 
        #endregion

        private delegate Task TypeCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
        private Dictionary<string, TypeCommand> _commands = new Dictionary<string, TypeCommand>();

        public Commands.Common CommonCommand;
        public Commands.Message MessageCommand;

        public CommandRoute()
        {
            CommonCommand = new Commands.Common(this);
            MessageCommand = new Commands.Message(this);
            RegisterUserCommand();
        }

        /// <summary>
        /// Регистрация доступных команд
        /// </summary>
        public void RegisterUserCommand()
        {
            _commands.Add(START, CommonCommand.GetMyId);
            _commands.Add(AUTH, CommonCommand.Auth);
            _commands.Add(MENU, CommonCommand.Menu);
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="command">Запрос команды</param>
        public async Task ExecuteCommand(string command, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var userId = update.GetChatId();
            if (botClient.HasStep(userId))
            {
                if (command == MENU || command == START)
                {
                    botClient.ClearStepUser(userId);
                    
                    return;
                }
                await botClient.GetStepOrNull(userId).Value(botClient, update, cancellationToken);
                return;
            }


            foreach (var item in _commands)
            {
                if (item.Key.ToLower() == command.ToLower())
                {
                    //Выполнение команды
                    await item.Value(botClient, update, cancellationToken);
               
                    return;
                }
            }
            //Сообщение что команда не найдена
            await CommandMissing(botClient, update, cancellationToken);
        }

        /// <summary>
        /// Не найдена команда
        /// </summary>
        public async Task CommandMissing(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await MessageCommand.Send(botClient, update.GetChatId(), $"Отсутствует команда '{update.Message.Text}'", cancellationToken);
        }
    }
}
