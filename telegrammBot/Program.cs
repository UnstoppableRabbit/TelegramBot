using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Telegram.Bot;
using telegrammBot.Data;

namespace telegrammBot
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static TelegramBotClient Bot;

        static async Task Main(string[] args)
        {
            Bot = new Telegram.Bot.TelegramBotClient(BotData.BotKey);
            var handle = GetConsoleWindow(); //скрываем окно
            ShowWindow(handle, SW_HIDE);
            await BotWork();
        }
        
        private static async Task BotWork()
        {
            await Bot.SetWebhookAsync("");
            try
            {
                int offset = 0;
                while (true)
                {
                    var updates = await Bot.GetUpdatesAsync(offset);
                    foreach (var update in updates) // Перебираем все обновления
                    {
                        var message = update.Message;
                        
                        if (message?.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                        {
                            if (message.Text.ToLower().StartsWith("/help"))
                            {
                                await BotResponses.SendHelpInfo(Bot, message);
                            }

                            else if (message.Text.ToLower().StartsWith("/weather"))
                            {
                                await BotResponses.GetWeatherLocation(Bot, message);
                            }

                            else if (message.Text.ToLower().StartsWith("/phonk"))
                            {
                                await BotResponses.GetPhonkPhoto(Bot, message);
                            }

                            else if (message.Text.ToLower().StartsWith("пасхалка"))
                            {
                                await BotResponses.SendCriminalInfo(Bot, message);
                            }

                            // else if (message.Text.ToLower().StartsWith("/get photo"))
                            // {
                            //     await BotResponses.SendTextToPhoto(Bot, message);
                            // }

                        }

                        else if (message?.Type == Telegram.Bot.Types.Enums.MessageType.Location)
                        {
                            await BotResponses.SendWeatherLocation(Bot, message);
                        }

                        else if (message?.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
                        {
                            await BotResponses.SendInfoByPhoto(Bot, message);
                        }

                        offset = update.Id + 1;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}