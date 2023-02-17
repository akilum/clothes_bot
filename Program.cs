using clothes_bot.Models;
using clothes_bot.Models.ModelsDB;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    public class BotRunner
    {
        private const string token = ""; // put your bot token here
        private static List<CategoryProducts> chatMemory = new();
        private static ITelegramBotClient bot = new TelegramBotClient(token);
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    var message = update.Message;

                    if (message == null)
                        return;

                    #region /start
                    if (message.Text?.ToLower() == "/start")
                    {
                        if (!chatMemory.Any(cm => cm.ChatId == message.Chat.Id))
                        {
                            chatMemory.Add(new CategoryProducts
                            {
                                CurrentProductListIndex = 0,
                                ProductsList = new List<Product>(),
                                ProductCategoryId = null,
                                ChatId = message.Chat.Id
                            });
                        }

                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("View Products", "openCatalog"),
                                InlineKeyboardButton.WithCallbackData("How to contact you?", "sendContacts"),
                            }
                        });

                        await botClient.SendTextMessageAsync(message.Chat.Id, "Hello.\n\rHow can I help?", replyMarkup: inlineKeyboard);

                        return;
                    }
                    #endregion
                }

                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    if (update.CallbackQuery == null || update.CallbackQuery.Data == null || update.CallbackQuery.Message == null)
                        return;

                    var callbackQuery = update.CallbackQuery;

                    #region sendContacts
                    if (callbackQuery.Data == "sendContacts")
                    {
                        var urls = new List<FeedbackUrl>();

                        using (var db = new BotDbContext())
                        {
                            urls = db.FeedbackUrls.ToList();
                        }

                        InlineKeyboardButton[] buttonsArray = new InlineKeyboardButton[urls.Count];

                        for (int i = 0; i < urls.Count; i++)
                        {
                            buttonsArray[i] = InlineKeyboardButton.WithUrl(urls[i].Name, urls[i].Url);
                        }

                        var inlineKeyboard = new InlineKeyboardMarkup(buttonsArray);

                        var text = "You can contact us through our social networks: ";

                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, text, replyMarkup: inlineKeyboard);

                    }
                    #endregion
                    #region openCatalog
                    else if (callbackQuery.Data == "openCatalog")
                    {
                        chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id)?.Clear();

                        var categories = new List<ProductCategory>();

                        using (var db = new BotDbContext())
                        {
                            categories = db.ProductCategories.ToList();
                        }

                        InlineKeyboardButton[] buttonsArray = new InlineKeyboardButton[categories.Count];

                        for (int i = 0; i < categories.Count; i++)
                        {
                            buttonsArray[i] = InlineKeyboardButton.WithCallbackData(categories[i].Name, $"show.{categories[i].Description}");
                        }

                        var inlineKeyboard = new InlineKeyboardMarkup(buttonsArray);

                        var text = "What category of clothing are you interested in?";

                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, text, replyMarkup: inlineKeyboard);
                        await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
                    }
                    #endregion
                    #region back
                    else if (callbackQuery.Data == "back")
                    {
                        chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id)?.DecreaseIndex();

                        var entity = chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id);
                        if (entity == null)
                            return;
                        if (entity.CurrentProductListIndex == 0)
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Next", "next")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Back to categories", "openCatalog")
                                }
                            });
                            await SendPhoto(botClient, callbackQuery.Message, chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id)?.GetCurrentIndexImage(), inlineKeyboard);
                            await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Back", "back"),
                                    InlineKeyboardButton.WithCallbackData("Next", "next")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Back to categories", "openCatalog")
                                }
                            });
                            await SendPhoto(botClient, callbackQuery.Message, chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id)?.GetCurrentIndexImage(), inlineKeyboard);
                            await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
                        }
                    }
                    #endregion
                    #region next
                    else if (callbackQuery.Data == "next")
                    {
                        chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id)?.IncreaseIndex();

                        var entity = chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id);
                        if (entity == null)
                            return;
                        if (entity.CurrentProductListIndex + 1 == entity.ProductsAmount)
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Back", "back")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Back to categories", "openCatalog")
                                }
                            });
                            await SendPhoto(botClient, callbackQuery.Message, chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id)?.GetCurrentIndexImage(), inlineKeyboard);
                            await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Back", "back"),
                                    InlineKeyboardButton.WithCallbackData("Next", "next")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Back to categories", "openCatalog")
                                }
                            });
                            await SendPhoto(botClient, callbackQuery.Message, chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id)?.GetCurrentIndexImage(), inlineKeyboard);
                            await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
                        }
                    }
                    #endregion
                    #region show

                    else if (callbackQuery.Data.Contains("show"))
                    {
                        using (var db = new BotDbContext())
                        {
                            string selectedCategory = callbackQuery.Data.Split('.')[1];
                            int? categoryId = db.ProductCategories.SingleOrDefault(pc => pc.Description == selectedCategory)?.Id;

                            if (categoryId == null)
                                return;

                            List<Product> products = db.Products.Where(p => p.CategoryId == categoryId).ToList();

                            if (products == null || products.Count == 0)
                                return;

                            CategoryProducts? categoryProducts = chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id);
                            if (categoryProducts == null)
                                return;
                            categoryProducts.ProductsAmount = products.Count;
                            categoryProducts.ProductCategoryId = categoryId;
                            categoryProducts.CurrentProductListIndex = 0;
                            categoryProducts.AddRange(products);
                        }

                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Back", "back"),
                                InlineKeyboardButton.WithCallbackData("Next", "next")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Back to categories", "openCatalog")
                            }
                        });

                        await SendPhoto(botClient, callbackQuery.Message, chatMemory.SingleOrDefault(cm => cm.ChatId == callbackQuery.Message.Chat.Id)?.GetCurrentIndexImage(), inlineKeyboard);
                        await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
                    }

                    #endregion
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static async Task<Message> SendPhoto(ITelegramBotClient botClient, Message message, string? path, InlineKeyboardMarkup markup = null)
        {
            FileStream stream = new FileInfo(path).OpenRead();
            var photo = new InputOnlineFile(stream);
            var msg = await botClient.SendPhotoAsync(message.Chat.Id, photo, replyMarkup: markup);
            stream.Close();
            return msg;
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public static void Main()
        {
            Console.WriteLine($"Bot «{bot.GetMeAsync().Result.FirstName}» launched");
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}
