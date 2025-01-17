﻿using Offeror.TelegramBot.Contracts;
using Offeror.TelegramBot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Offeror.TelegramBot.Commands
{
    public class StopCommand : IBotCommand
    {
        private readonly ITelegramBotClient _client;

        public StopCommand(IServiceProvider serviceProvider)
        {
            _client = serviceProvider.CreateScope()
                .ServiceProvider.GetRequiredService<ITelegramBotClient>();
        }

        public string CommandName => BotCommands.StopCommand;

        public DateTime CommandStartTime { get; private set; }

        public bool IsCompleted { get; private set; }

        public event EventHandler<long>? CommandCompleted;

        public async Task InvokeAsync(Update update)
        {
            long chatId = update.GetChatId();
            await _client.SendTextMessageAsync(chatId, "Search terminated", replyMarkup: new ReplyKeyboardRemove());

            IsCompleted = true;
            CommandCompleted?.Invoke(this, chatId);
        }
    }
}
