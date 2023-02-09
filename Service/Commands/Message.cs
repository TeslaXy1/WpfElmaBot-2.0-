using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WpfElmaBot_2._0_.ViewModels;

namespace WpfElmaBot.Service.Commands
{

    public class Message
    {
        private CommandRoute route;
        public Message(CommandRoute route)
        {
            this.route = route;
        }
        /// <summary>
        /// Команда для отправки сообщения пользователю
        /// </summary>
        public async Task Send(ITelegramBotClient botClient, long chatId, string msg, CancellationToken cancellationToken, OptionTelegramMessage option = null)
        {
            try
            {
                if (option == null)
                {
                    var sentMessage = await botClient.SendTextMessageAsync(
                         chatId: chatId,
                         text: msg,
                         parseMode: ParseMode.Html,
                         cancellationToken: cancellationToken);
                }
                else
                {
                    if (option.ClearMenu)
                    {
                        var sentMessage = await botClient.SendTextMessageAsync(
                             chatId: chatId,
                             text: msg,
                             parseMode: ParseMode.Html,
                             replyMarkup: new ReplyKeyboardRemove(),
                             cancellationToken: cancellationToken);
                    }
                    else if (option.MenuReplyKeyboardMarkup != null)
                    {
                        var sentMessage = await botClient.SendTextMessageAsync(
                             chatId: chatId,
                             text: msg,
                             parseMode: ParseMode.Html,
                             replyMarkup: option.MenuReplyKeyboardMarkup,
                             cancellationToken: cancellationToken);
                    }
                    else if (option.MenuInlineKeyboardMarkup != null)
                    {
                        var sentMessage = await botClient.SendTextMessageAsync(
                             chatId: chatId,
                             text: msg,
                             parseMode: ParseMode.Html,
                             replyMarkup: option.MenuInlineKeyboardMarkup,
                             cancellationToken: cancellationToken);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка отправки сообщений в телеграм | " + ex);
                throw new Exception(ex.Message);
            }

        }
    }
}
