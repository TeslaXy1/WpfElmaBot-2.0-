using Telegram.Bot.Types.ReplyMarkups;

namespace WpfElmaBot.Service.Commands
{
    public class OptionTelegramMessage
    {
        public ReplyKeyboardMarkup MenuReplyKeyboardMarkup { get; set; }
        public InlineKeyboardMarkup MenuInlineKeyboardMarkup { get; set; }
        public bool ClearMenu = false;
    }
}