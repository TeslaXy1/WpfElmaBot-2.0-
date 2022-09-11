using Telegram.Bot.Types.ReplyMarkups;

namespace WpfElmaBot.Service.Commands
{
    public class OptionTelegramMessage
    {
        public ReplyKeyboardMarkup MenuReplyKeyboardMarkup { get; set; }
        public InlineKeyboardMarkup MenuInlineKeyboardMarkup { get; set; }
        public bool ClearMenu = false;

        public OptionTelegramMessage()
        {

                    InlineKeyboardMarkup auth = new InlineKeyboardMarkup(new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Авторизация"),
                         
                        }
                    });


        }   
    }
}