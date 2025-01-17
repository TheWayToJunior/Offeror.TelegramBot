﻿using Moq;
using Offeror.TelegramBot.Commands;
using Offeror.TelegramBot.Commands.Start.States;
using Offeror.TelegramBot.Constants;
using Offeror.TelegramBot.Contracts;
using Offeror.TelegramBot.Models;
using Offeror.TelegramBot.Tests.Mocks;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Xunit;

namespace Offeror.TelegramBot.Tests
{
    public class StartCommandTests
    {
        [Fact]
        public async Task DisplayProfilesState_SwitchState()
        {
            var client = new MockTelegramBotClient().CreateMockSendMessageRequest();

            var serviceProviderBuilder = new MockServiceProvider();
            serviceProviderBuilder
                .AddService(new ProfileDisplayState(client.Object))
                .AddService(new ProfileInputState(null))
                .Build();

            var command = new StartCommand(serviceProviderBuilder.GetResult());
            Update update = UpdateFactory.CreateUpdate(1, "Test");

            /// Act
            await command.InvokeAsync(update);

            Assert.NotNull(command.CommandState);
            Assert.IsType<ProfileInputState>(command.CommandState);
        }

        [Fact]
        public async Task SetProfileState_ButtonNext_StateLoop()
        {
            var mediator = MockMediator.CreateAnnouncementQueryHandler();
            var searchFilter = SearchFilterFactory.CreateSearchFilter(nameof(SearchFilter.Status), Requests.Vacancy);
            var currentState = new SearchInputState();

            var reader = new Mock<ISearchFilterReader>();
            reader.Setup(x => x.GetFilter())
                .Returns(searchFilter);

            var serviceProviderBuilder = new MockServiceProvider();
            serviceProviderBuilder
                .AddService(new ProfileDisplayState(null))
                .AddService(new SearchDisplayState(null, reader.Object, mediator.Object))
                .AddService(currentState)
                .Build();

            var command = new StartCommand(serviceProviderBuilder.GetResult());
            command.UpdateState(currentState);

            Update update = UpdateFactory.CreateUpdate(1, Buttons.Search);

            /// Act
            await command.InvokeAsync(update);

            Assert.NotNull(command.CommandState);
            Assert.IsType<SearchInputState>(command.CommandState);
            Assert.Equal(currentState, command.CommandState);
        }

        [Fact]
        public async Task SetProfileState_ButtonRestart_StateRestart()
        {
            var client = new MockTelegramBotClient().CreateMockSendMessageRequest();
            var currentState = new SearchInputState();

            var serviceProviderBuilder = new MockServiceProvider();
            serviceProviderBuilder
                .AddService(new ProfileDisplayState(client.Object))
                .AddService(new ProfileInputState(null))
                .AddService(currentState)
                .Build();

            var command = new StartCommand(serviceProviderBuilder.GetResult());
            command.UpdateState(currentState);

            Update update = UpdateFactory.CreateUpdate(1, Buttons.Restart);

            /// Act
            await command.InvokeAsync(update);

            Assert.NotNull(command.CommandState);
            Assert.IsType<ProfileInputState>(command.CommandState);
        }
    }
}
