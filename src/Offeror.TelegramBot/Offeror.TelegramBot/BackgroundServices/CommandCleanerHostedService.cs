﻿using Offeror.TelegramBot.Contracts;

namespace Offeror.TelegramBot.BackgroundServices
{
    public class CommandCleanerHostedService : IHostedService, IDisposable
    {
        private readonly ICommandExecutor _commandExecutor;
        private readonly ILogger<CommandCleanerHostedService> _logger;
        private readonly CancellationTokenSource _tokenSource;

        private Timer? _timer = null;
        private Task? _executingTask = null;

        public CommandCleanerHostedService(ICommandExecutor commandExecutor, ILogger<CommandCleanerHostedService> logger)
        {
            _commandExecutor = commandExecutor;
            _logger = logger;

            _tokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Command Cleaner Hosted Service running.");

            /// TODO: Set the launch interval in the project configuration
#if DEBUG
            TimeSpan interval = TimeSpan.FromMinutes(2);
#else
            TimeSpan interval = TimeSpan.FromMinutes(10);
#endif
            _timer = new Timer((state) => _executingTask = ExecuteTaskAsync(_tokenSource.Token),
                null, dueTime: interval, period: interval);

            return Task.CompletedTask;
        }

        private async Task ExecuteTaskAsync(CancellationToken cancellationToken)
        {
            var deletedCommands = await _commandExecutor.ClearOutdatedCommandsAsync();
            _logger.LogInformation($"[{DateTime.Now}] The commands have been removed: {deletedCommands.Count()}");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Command Cleaner Hosted Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);

            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _tokenSource.Cancel();
            }
            finally
            {
                await Task.WhenAny(
                    _executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _timer?.Dispose();
        }
    }
}
