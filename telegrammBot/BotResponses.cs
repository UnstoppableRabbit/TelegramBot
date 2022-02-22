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
using Emgu.CV.Structure;
using telegrammBot.PhotoConfig;
using System.Text;

namespace telegrammBot
{
    public static class BotResponses
    {
        public static async Task SendHelpInfo(TelegramBotClient Bot, Message message)
        {
            await Bot.SendTextMessageAsync(message.Chat.Id, "Доступные команды:\n" +
                                                               "/weather - узнать погоду;\n" +
                                                               "Скоро появится фонк!\n"+
                                                               "А вот и он! - /phonk\n" +
                                                               "Также можете отправить фото\n" +
                                                               "По фото бот может сканировать qr-коды и определять количество людей на фото\n",
                                
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
            try
            {
                if (message.Photo != null)
                {
                    var file = await Bot.GetFileAsync(message.Photo[^1].FileId);
                    var fileName = $"C:\\Users\\Minaev_G\\Desktop\\photos\\{message.Chat.Id}.jpg";
                    using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                        await Bot.DownloadFileAsync(file.FilePath!, fs);
                    
                    Mat image = new Mat(fileName);
                    //image.SetTo(new Bgr(255, 255, 255).MCvScalar);
                    //CvInvoke.PutText(image, "JOpa", new System.Drawing.Point(10, 50), Emgu.CV.CvEnum.FontFace.HersheyPlain, 3.0, new Bgr(255.0, 0.0, 0.0).MCvScalar);
                    var alalyzedPhoto = image.ToImage<Bgr, byte>();
                   
                    var e = new CascadeFaceDetector();
                    await e.Init();

                    var _renderMat = new Mat();
                    using (InputArray iaImage = alalyzedPhoto.GetInputArray())
                    {
                        iaImage.CopyTo(_renderMat);
                    }
                    var count = e.GetFacesCount(alalyzedPhoto, _renderMat);

                    var sended = _renderMat.ToImage<Bgr, byte>();

                    using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                    {
                        await fs.WriteAsync(sended.ToJpegData());
                    }

                    using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (count > 0)
                            await Bot.SendPhotoAsync(
                                message.Chat.Id, new InputOnlineFile(fs), caption: $"На фото изображены людишки в количестве ({count})", replyToMessageId: message.MessageId);
                        else
                        {
                            var codes = await GetQrCodes(_renderMat);
                            
                            var resultText = new StringBuilder();
                            foreach(var str in codes){
                                resultText.Append($"{str}\n");
                            }
                            await Bot.SendTextMessageAsync(
                                message.Chat.Id, resultText.ToString(), replyToMessageId: message.MessageId);
                        }
                    }
                }
            }
            catch 
            {
                // ignored
            }
        }


        public static async Task<string[]> GetQrCodes(Mat qrPhoto){

            var qr = new CascadeQRCodeDetector();
            await qr.Init(null);

            return qr.ProcessAndRender(qrPhoto).Select(_=>_.Code).ToArray();
        }
    }
}