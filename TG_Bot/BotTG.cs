using System;
using System.Text;
using System.Data.SQLite;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;
using System.Collections.Generic;
using System.Linq;

namespace TG_Bot
{
    public class BotTG
    {
        public static TelegramBotClient Bot;
        //public static BotClass Bot;
        //static string path = "TelegramDB.db";
        //public static SQLiteConnection con;
        //public static SQLiteCommand cmd;
        //public static SQLiteDataReader dr;
        //public static int result;
        //public static string connectionstring = "Data Source = TelegramDB.db; Version=3;";

        public static InlineKeyboardMarkup keyboardfornote;
        static Database db = new Database();
        public static DateTime BotStartupTime;
        static void Main(string[] args)
        {
            BotInitialize();
            db.DatabaseIni();
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Console.ReadLine();
        }
        private static void BotInitialize()
        {
            Bot = new TelegramBotClient("5021002246:AAE9846AS5QEJ-8XK5IwGJ16Qm7eWmXXXXX");
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            /*----------Bot.OnMessage += BotOnMessageReceivedAsync;-----------------------------------------------------------------------------------*/
            var me = Bot.GetMeAsync().Result;
            Console.WriteLine(me.FirstName);
            Bot.StartReceiving();
            BotStartupTime = DateTime.UtcNow;
            keyboardfornote = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Delete note", "deleteCallback")
                }
            }
            );
        }

        private static async void BotOnCallbackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";
            Console.WriteLine($"____________________________________________________________" +
                $"\n{name} pressed button {buttonText}");
            switch (e.CallbackQuery.Data)
            {
                case "deleteCallback":
                    try
                    {
                        await Bot.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString() + "+");
                    }
                    string[] lines = e.CallbackQuery.Message.Text.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                    db.DeleteNote(e.CallbackQuery.From.Id, lines[1], lines[2], lines[0].Split(' ', ':')[2]);
                    break;
                case "editCallback":

                    break;
            }
        }
        private async static void BotOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Date >= BotStartupTime)
            {
                var uid = message.From.Id;
                var ustate = db.GetState(uid);
                var name = message.From.FirstName;
                Console.WriteLine("____________________________________________________________" +
                    "\nMessage received." + DateTime.UtcNow + " Sender: " + message.From.FirstName + " " + message.From.LastName + " UID: " + uid);
                Log(message);
                switch (ustate)
                {
                    case CurrentState.Add:
                        await Bot.SendTextMessageAsync(uid, "Enter your note name: ");
                        db.UpdateState(uid, CurrentState.AddName);
                        break;
                    case CurrentState.AddName:
                        db.UpdateName(uid, message.Text);
                        await Bot.SendTextMessageAsync(uid, "Enter your thoughts: ");
                        db.UpdateState(uid, CurrentState.AddContent);
                        break;
                    case CurrentState.AddContent:
                        db.AddNote(uid, db.GetTempName(uid), message.Text);
                        db.UpdateState(uid, CurrentState.Menu);
                        break;
                    case CurrentState.Menu:
                        if (message.Text == "/start")
                        {
                            db.NewUserQuery(message.From.Id, message.From.FirstName);
                            await Bot.SendTextMessageAsync(uid, "/start - menu\n/add - add new note\n/get - list your notes");
                            db.UpdateState(message.From.Id, CurrentState.Menu);                            
                        }
                        if (message.Text == "/add")
                        {
                            db.UpdateState(message.From.Id, CurrentState.AddName);
                            await Bot.SendTextMessageAsync(uid, "Enter your note name: ");
                        }
                        if (message.Text == "/get")
                        {
                            db.GetNotes(message);
                        }
                        if (message.Text.Split()[0] == "/say")
                        {
                            SendMessageToEveryone(message.Text);
                        }
                        break;
                }
            }
        }
            public static bool isInRange(Telegram.Bot.Types.Message message, int delta)
            {
                int secnow = DateTime.UtcNow.Second;
                int secnowmessage = message.Date.Second;
                if (secnow - secnowmessage <= delta || secnow - secnowmessage == 60)
                {
                    return true;
                }
                else return false;
            }

            public async static void SendMessage(long userid, string message)
            {
                try
                {
                    await Bot.SendTextMessageAsync(userid, message);
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            public async static void SendMessageWithReply(long userid, string message)
            {
                try
                {
                    await Bot.SendTextMessageAsync(userid, message, replyMarkup: keyboardfornote);
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            public async static void SendMessageToEveryone(string message)
            {

            }
            public static void Log(Telegram.Bot.Types.Message message)
            {
                string name = $"{message.From.FirstName} {message.From.LastName}";
                Console.WriteLine($"{name} отправил сообщение: {message.Text}");
                System.IO.StreamWriter writer = new System.IO.StreamWriter("logs.txt", true);
                writer.Write(System.DateTime.Now + "     ");
                writer.WriteLine($"{name} отправил сообщение: {message.Text}");
                writer.Close();
            }
            public static void LogEx(Exception e, string when)
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter("exlogs.txt", true);
                writer.WriteLine(System.DateTime.Now + $"     {when}" + e.Message);
            Console.WriteLine("ERROR: " + System.DateTime.Now + $"     {when}" + e.Message);
                writer.Close();
            }

            static void CurrentDomain_ProcessExit(object sender, EventArgs e)
            {
                Console.WriteLine("App is closed().");
            }
        }
    }