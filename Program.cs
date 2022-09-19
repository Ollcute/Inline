using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Microsoft.Data.Sqlite;
using NLog;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBGY;

namespace TelegramBotExperiments
{

    class Program
    {
        static string kod = System.IO.File.ReadAllText(@"token.txt");
        static ITelegramBotClient bot = new TelegramBotClient(kod.ToString());
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static ReplyKeyboardMarkup lastkeyboard = new ReplyKeyboardMarkup(new KeyboardButton(""));
        private static ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(new KeyboardButton(""));
        


        static void Main(string[] args)

        {   

            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            logger.Debug("log {1}", "EventHandler"); //лог

            //Подключение SQLite
            CreateTable();

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            //подключение событий
            {
                AllowedUpdates = { }, //получать все типы обновлений
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();

            //подключение БД
            using (var connection = new SqliteConnection("Data Source=users.db"))
            {
                connection.Open();
            }
            Console.Read();
            string sqlExpression = "SELECT * FROM BGY";
            using (var connection = new SqliteConnection("Data Source=BGY.db"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read())   // построчно считываем данные
                        {
                            var id = reader.GetValue(0);
                            var Category = reader.GetValue(1);
                            var Product = reader.GetValue(2);
                            var B = reader.GetValue(3);
                            var F = reader.GetValue(4);
                            var u = reader.GetValue(5);
                            var K = reader.GetValue(6);

                            BGU bgu = new BGU(id, Category, Product, K, B, F, u);
                        }


                    }
                }
            }
            Console.Read();
        }


        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //изменени
            // Некоторые действия
            logger.Debug("log {0}", "Start/Info/Help.Debug"); //лог
            Console.WriteLine(JsonConvert.SerializeObject(update));
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                InsertData(message);

                logger.Debug("log {0}", "Кнопка Start"); //лог
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать в  Telegram-бот по подсчету калорий!");
                    return;
                }
                logger.Debug("log {0}", "Кнопка Info"); //лог
                if (message.Text.ToLower() == "/info")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Моя задача помочь пользователю посчитать количество КБЖУ в продуктах.");
                    return;
                }
                logger.Debug("log {0}", "Кнопка Help"); //лог
                if (message.Text.ToLower() == "/help")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "\"Для работы с ботом необходимо воспользоваться кнопками: выбрать категорию продукта, затем сам продукт из предложенных. Также использовать  /start, /help, /info, /menu.");
                    return;
                }
                logger.Debug("log {0}", "Кнопка Menu"); //лог
                //создание кнопок
                if (message.Text.ToLower() == "/menu") //запуск кнопок
                {
                    //изменения
                    replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Напитки"),
                        new KeyboardButton("Фрукты и Овощи"),
                        new KeyboardButton("Молочные продукты"),
                        new KeyboardButton("Мясо")
                    }
                    );
                    await bot.SendTextMessageAsync(message.From.Id, "Для работы с ботом необходимо воспользоваться кнопками: выбрать категорию продукта, затем сам продукт из предложенных", replyMarkup: replyKeyboard);
                    return;
                }
                //создание подкнопок
                if (message.Text.ToLower() == "напитки")
                {
                    lastkeyboard = replyKeyboard;
                    replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Чай"),
                        new KeyboardButton("Кофе"),
                        new KeyboardButton("Газировка"),
                        new KeyboardButton("Назад")
                    }
                    );
                    await bot.SendTextMessageAsync(message.From.Id, "Выберите что-то из списка.", replyMarkup: replyKeyboard);
                    return;
                }
                if (message.Text.ToLower() == "фрукты и овощи")
                {
                    lastkeyboard = replyKeyboard;
                    replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Фрукты"),
                        new KeyboardButton("Овощи"),
                        new KeyboardButton("Назад")
                    }
                    );
                    await bot.SendTextMessageAsync(message.From.Id, "Выберите что-то из списка", replyMarkup: replyKeyboard);
                    return;
                }
                if (message.Text.ToLower() == "молочные продукты")
                {
                    lastkeyboard = replyKeyboard;
                    replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Молоко"),
                        new KeyboardButton("Творог"),
                        new KeyboardButton("Назад")
                    }
                    );
                    await bot.SendTextMessageAsync(message.From.Id, "Выберите что-то из списка.", replyMarkup: replyKeyboard);
                    return;
                }
                if (message.Text.ToLower() == "мясо")
                {
                    lastkeyboard = replyKeyboard;
                    replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Курица"),
                        new KeyboardButton("Баранина"),
                        new KeyboardButton("Свинина"),
                        new KeyboardButton("Говядина"),
                        new KeyboardButton("Назад")
                    }
                    );
                    await bot.SendTextMessageAsync(message.From.Id, "Выберите что-то из списка.", replyMarkup: replyKeyboard);
                    return;
                }
                if (message.Text.ToLower() == "назад")
                {
                    replyKeyboard = lastkeyboard;
                    await bot.SendTextMessageAsync(message.From.Id, "Выберите что-то из списка.", replyMarkup: replyKeyboard);
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat, "Извините, я не могу Вас понять.");
                //Те самые inline кнопки, я переустановлю среду и смогу их реализовать
                //if (message.Text == "/inline")
                //{
                //    InlineKeyboardMarkup keyboard = new(new[]
                //    {
                //    new[]
                //    {
                //    InlineKeyboardButton.WithCallbackData("Buy 50c", "buy_50c"),
                //    InlineKeyboardButton.WithCallbackData("Buy 100c", "buy_100c"),
                //    },
                //     new[]
                //    {
                //    InlineKeyboardButton.WithCallbackData("Sell 50c", "sell_50c"),
                //    InlineKeyboardButton.WithCallbackData("Sell 100c", "sell_100c"),
                //     },
                //     });
                //    await botClient.SendTextMessageAsync(message.Chat.Id, "Choose inline:", replyMarkup: keyboard);
                //    return;
                //}
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        //Работа с БД для аналитики пользователей
        //Соединение с БД
        static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            //Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source= users.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {

            }
            return sqlite_conn;
        }

        //Создание таблиц
        static void CreateTable()
        {
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand sqlite_cmd;
            string Createsql = "CREATE TABLE IF NOT EXISTS Users (Text text, ID INT, FromID INT, Bot boolean, Date string(40), Username string(30), Firstname string(25), Lastname string(25))";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
        }

        //Вставка данных в таблицу
        static void InsertData(Message message)
        {
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"INSERT INTO Users (Text, ID, FromID, Bot, Date, Username, Firstname, Lastname) " +
                $"VALUES( '{message.Text}', {message.MessageId}, {message.From.Id}, {message.From.IsBot}, '{message.Date}', '{message.From.Username}', '{message.From.FirstName}', '{message.From.LastName}' ); ";
            sqlite_cmd.ExecuteNonQuery();
        }
       
        
    }
}
