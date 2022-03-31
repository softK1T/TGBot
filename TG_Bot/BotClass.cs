using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;

namespace TG_Bot
{
    public class BotClass
    {
        public TelegramBotClient Bot;
        public async void SendMessage(long userid, string message)
        {
            await Bot.SendTextMessageAsync(userid, message);
        }
    }
}
