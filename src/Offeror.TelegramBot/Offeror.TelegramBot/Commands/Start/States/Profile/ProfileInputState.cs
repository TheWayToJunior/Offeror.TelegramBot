﻿using Offeror.TelegramBot.Constants;
using Offeror.TelegramBot.Contracts;
using Offeror.TelegramBot.Exceptions;
using Offeror.TelegramBot.Models;
using Telegram.Bot.Types;

namespace Offeror.TelegramBot.Commands.Start.States
{
    public class ProfileInputState : IState
    {
        private readonly ISearchFilterWriter _filter;

        public ProfileInputState(ISearchFilterWriter filter)
        {
            _filter = filter;
        }

        public async Task ExecuteAsync(IBotStateMachine command, Update update)
        {
            var states = command as IStateContainer ?? throw new InvalidCastException();

            var status = update?.Message?.Text switch
            {
                Buttons.Applicant => Requests.Vacancy,
                Buttons.Сompany => Requests.Resume,

                _ => throw new CommandNotFoundException("There is no such answer option"),
            };

            _filter.SetProperty(nameof(SearchFilter.Status), status);
            IState nextState = states.GetState<RegionDisplayState>();

            await command.UpdateState(nextState).ExecuteAsync(command, update);
        }
    }
}
