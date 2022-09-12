using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WpfElmaBot.Models;

namespace WpfElmaBot.Service.Commands
{
    public class Common
    {
        private CommandRoute route;
        private ELMA elma = new ELMA();
       
        public Common(CommandRoute route)
        {
            
            this.route = route;
            
        }
        public async Task GetMyId(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                //var entity = await new ELMA().GetEntity<Entity>("c5c12f67-3f57-45c2-aa70-02dfded87f77");
                //var message = await new ELMA().GetUnreadMessage<MessegesOtvet>();
                OptionTelegramMessage message = new OptionTelegramMessage();
               
                message.MenuInlineKeyboardMarkup = new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Авторизация"),
                            InlineKeyboardButton.WithCallbackData("Не авторизация"),

                        };
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Ваш id {update.Message.Chat.Id}", cancellationToken, message);
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
                string path = $"Authorization/LoginWith?username={loginpas.Value.Login}";
                var authorization =  await elma.PostRequest<Auth>(path, loginpas.Value.Password);
                var message = await elma.GetUnreadMessage<MessegesOtvet>(authorization.AuthToken, authorization.SessionToken);

                //TODO проверка наличия пользователя в справочнике


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
