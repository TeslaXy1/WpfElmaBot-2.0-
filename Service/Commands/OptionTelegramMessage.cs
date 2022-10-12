using Telegram.Bot.Types.ReplyMarkups;

namespace WpfElmaBot.Service.Commands
{
    public class OptionTelegramMessage
    {
        private static OptionTelegramMessage instance;
        public static OptionTelegramMessage getInstance()
        {

            if (instance == null)
                instance = new OptionTelegramMessage();
            return instance;
        }
        public ReplyKeyboardMarkup MenuReplyKeyboardMarkup { get; set; }
        public InlineKeyboardMarkup MenuInlineKeyboardMarkup { get; set; }

        public bool ClearMenu = false;

        public OptionTelegramMessage()
        {
           
        }   
    }
}