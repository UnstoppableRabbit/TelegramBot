using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PexelsNet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using telegrammBot.Data;
using telegrammBot.WeatherAccess;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace telegrammBot
{
    public static class BotResponses
    {
        public static async Task SendHelpInfo(TelegramBotClient Bot, Message message)
        {
            await Bot.SendTextMessageAsync(message.Chat.Id, "Доступные команды:\n " +
                                                               "/weather - узнать погоду;\n" +
                                                               "Скоро появится фонк! \n" +
                                                               "А вот и он! - /phonk",
                                
                replyToMessageId: message.MessageId);
        }

        public static async Task SendCriminalInfo(TelegramBotClient Bot, Message message)
        {
            int days = (DateTime.Now - new DateTime(2022, 1, 18)).Days;

            await Bot.SendTextMessageAsync(message.Chat.Id, $"Я незаметно существую на рабочем сервере Контур-НИИРС уже {days} дней",
                replyToMessageId: message.MessageId);
        }

        public static async Task GetWeatherLocation(TelegramBotClient Bot, Message message)
        {
            var keyboard = new ReplyKeyboardMarkup(new KeyboardButton("Отправить локацию") { RequestLocation = true });
            await Bot.SendTextMessageAsync(message.Chat.Id, "Необходимо указать местоположение",
                replyToMessageId: message.MessageId, replyMarkup: keyboard);
        }

        public static async Task SendWeatherLocation(TelegramBotClient Bot, Message message)
        {
            var wether = await WeatherAction.GetCurrentWeather(message.Location.Latitude, message.Location.Longitude);

            if (string.IsNullOrEmpty(wether.Photo))
                await Bot.SendTextMessageAsync(message.Chat.Id, $"Температура: {wether.Temperature};\n " +
                                                                  $"Населенный пукнт: {wether.City};\n " +
                                                                  $"Время: {wether.Date:t}", replyToMessageId: message.MessageId);
            else
                await Bot.SendPhotoAsync(
                    message.Chat.Id, new InputOnlineFile(new Uri(wether.Photo)), replyToMessageId: message.MessageId, caption: $"Температура: {wether.Temperature};\n" + $"Населенный пукнт: {wether.City};\n " + $"Время: {wether.Date:t}", replyMarkup: new ReplyKeyboardRemove());
        }

        public static async Task GetPhonkPhoto(TelegramBotClient Bot, Message message)
        {
            var client = new PexelsClient(BotData.PexelesKey);
            var results = await client.SearchAsync("jdm");
            var rnd = new Random();
            var k = rnd.Next(0, results.Photos.Count);

            await Bot.SendPhotoAsync(
                    message.Chat.Id, new InputOnlineFile(new Uri(results.Photos[k].Src.Medium)), replyToMessageId: message.MessageId, replyMarkup: new ReplyKeyboardRemove());

            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "/../../../audio");
            var k1 = rnd.Next(0, dir.EnumerateFiles().Count());

            await Bot.SendAudioAsync(
                message.Chat.Id, audio: $"https://github.com/UnstoppableRabbit/TelegramBot/raw/master/telegrammBot/audio/{dir.EnumerateFiles().ToList()[k1].Name}", replyToMessageId: message.MessageId + 1, replyMarkup: new ReplyKeyboardRemove());
        }

        public static async Task SendInfoByPhoto(TelegramBotClient Bot, Message message)
        {
            Image<Bgr, Byte> My_Image = new Image<Bgr, byte>(message.Document.FileId);
            var data = My_Image.ToJpegData();
            using(FileStream fs = new FileStream("C:/Users/Minaev_G/Desktop/some.jpg", FileMode.Create)){
                await fs.WriteAsync(data);
            } 
        }
    }
}
