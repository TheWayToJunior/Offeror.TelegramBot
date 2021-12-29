﻿using Offeror.TelegramBot.Extensions;
using System.Collections.Concurrent;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Offeror.TelegramBot.Commands
{
    public interface ICommandExecutor
    {
        Task ExecuteAsync(Update update);
    }

    public sealed class CommandExecutor : ICommandExecutor
    {
        private readonly IDictionary<string, Func<IBotCommand>> _commands;

        private readonly ConcurrentDictionary<long, IBotCommand> _usersCommands;

        public CommandExecutor(IServiceProvider provider)
        {
            _commands = new Dictionary<string, Func<IBotCommand>>()
            {
                { Commands.StartCommand, () => new StartCommand(provider) },
            };

            _usersCommands = new();
        }

        private IBotCommand? GetUserCommand(long chatId) => _usersCommands.GetValueOrDefault(chatId);

        public async Task ExecuteAsync(Update update)
        {
            var command = update.Message?.Entities?.SingleOrDefault(e => e.Type == MessageEntityType.BotCommand);

            if (command is not null)
            {
                SetBotCommand(update);
            }

            var botCommand = GetUserCommand(update.GetChatId());

            await (botCommand?.InvokeAsync(update)
                ?? throw new InvalidOperationException($"The command execution time is up. Please enter the command again"));
        }

        private void SetBotCommand(Update update)
        {
            string? commandKey = update.Message?.Text;

            if (string.IsNullOrWhiteSpace(commandKey) || !_commands.ContainsKey(commandKey))
            {
                throw new InvalidOperationException("The specified command does not exist");
            }

            long chatId = update.GetChatId();
            var currentCommand = GetUserCommand(chatId);

            if (currentCommand?.CommandName == commandKey)
            {
                currentCommand.Restart(); 
                return;
            }

            var newCommand = _commands[commandKey].Invoke();
            _usersCommands.AddOrUpdate(chatId, newCommand, (id, oldCommand) => newCommand);
        }
    }
}
