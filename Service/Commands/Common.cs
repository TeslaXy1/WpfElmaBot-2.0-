using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using WpfElmaBot.Models;

namespace WpfElmaBot.Service.Commands
{
    public class Common
    {
        private CommandRoute route;
       
        public Common(CommandRoute route)
        {
            
            this.route = route;
            
        }
        public async Task GetMyId(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Ваш id {update.Message.Chat.Id}", cancellationToken);
            }
            catch (Exception ex)
            {
               //TODO Вывод exception
            }
        }

        public async Task Auth(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите логин", cancellationToken);
                botClient.RegisterNextStep(update.Message.Chat.Id, Login);
            }
            catch (Exception ex)
            {
                //TODO Вывод exception
            }
        }

        public async Task Login(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите пароль", cancellationToken);
                botClient.GetCacheData(update.GetChatId()).Value.Login =  update.Message.Text;
                botClient.RegisterNextStep(update.Message.Chat.Id, LoginPasswordHandler);
            }
            catch (Exception ex)
            {
                //TODO Вывод exception
            }
        }
      
        public async Task LoginPasswordHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                botClient.GetCacheData(update.GetChatId()).Value.Password = update.Message.Text;
                KeyValuePair<long,UserCache> loginpas= BotExtension.GetCacheData(botClient, update.Message.Chat.Id);
                string login= loginpas.Value.Login;
                string pasw = loginpas.Value.Password;
                string path = $"Authorization/LoginWith?username={login}";
                var authorization =  await new ELMA().PostRequest<Auth>(path,pasw);

                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы успешно авторизованы", cancellationToken);
            }
            catch (Exception ex)
            {

                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Неверный логин или пароль", cancellationToken);
            }
        }
        public async Task Menu(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы вышли в меню", cancellationToken);

            }
            catch (Exception ex)
            {
                //TODO Вывод exception
            }
        }
    }
}
