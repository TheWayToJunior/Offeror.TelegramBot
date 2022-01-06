﻿using Offeror.TelegramBot.Contracts;
using Offeror.TelegramBot.Extensions;
using Telegram.Bot.Types;

namespace Offeror.TelegramBot.Commands.Start.States
{
    public class SetKeywordsState : IState
    {
        private readonly ISearchFilterWriter _filter;

        public SetKeywordsState(ISearchFilterWriter filter)
        {
            _filter = filter;
        }

        public async Task ExecuteAsync(IBotCommand command, Update update)
        {
            var states = command as IStateContainer ?? throw new InvalidCastException();

            IState nextState = update?.Message?.Text switch
            {
                Buttons.Back => states.GetState<DisplayRegionsState>(),
                Buttons.Search => ButtonSearchHandler(states),
                Buttons.Clear => ButtonClearKeywordsHandler(states),

                _ => KeywordsHandler(states, update.GetTextMessage()),
            };

            await command.UpdateState(nextState).ExecuteAsync(command, update);
        }

        private IState ButtonSearchHandler(IStateContainer states)
        {
            /// TODO: Check if the search filter is full
            return states.GetState<DisplaySearchState>();
        }

        private IState ButtonClearKeywordsHandler(IStateContainer states)
        {
            _filter.ClearKeyword();
            return states.GetState<DisplayKeywordsState>();
        }

        private IState KeywordsHandler(IStateContainer states, string keywords)
        {
            _filter.AppendKeyword(keywords);
            return states.GetState<DisplayKeywordsState>();
        }
    }
}
