﻿using MediatR;
using Moq;
using Offeror.TelegramBot.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Offeror.TelegramBot.Tests.Mocks
{
    internal class MockMediator
    {
        public static Mock<IMediator> CreateAnnouncementQueryHandler()
        {
            var response = new Mock<IAnnouncement>();
            response
                .Setup(x => x.AcceptAsync(It.IsAny<IDisplayVisitor>()))
                .Verifiable();

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(x => x.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((object?)response.Object));

            return mediator;
        }
    }
}
