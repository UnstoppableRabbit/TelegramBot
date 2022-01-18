using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using telegrammBot.WeatherAccess;

namespace telegrammBot
{
    public static class BotResponses
    {
        public static async Task SendHelpInfo(TelegramBotClient Bot, Message message)
        {
            await Bot.SendTextMessageAsync(message.Chat.Id, "Доступные команды:\n " +
                                                              "/weather - узнать погоду;\n" +
                                                              "Скоро появится фонк!",
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
                                                                $"Время: {wether.Date:t}", replyToMessageId: message.MessageId);
            else
                await Bot.SendPhotoAsync(
                    message.Chat.Id, new InputOnlineFile(new Uri(wether.Photo)), replyToMessageId: message.MessageId, caption: $"Температура: {wether.Temperature};\n " + $"Время: {wether.Date:t}", replyMarkup: new ReplyKeyboardRemove());
        }
    }
}
