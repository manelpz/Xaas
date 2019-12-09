using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Xaas
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        public IConfiguration _config;

        
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        #region Worker service
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                //call the function that connects with twitter API
                twitterConnection twitterConnectionClass = new twitterConnection();
                twitterConnectionClass.twitterFunction(_config);

                //delay the time to recall the worker service, multiply by 30*1000 to get a call each 30 seconds
                await Task.Delay(1000, stoppingToken);
            }
        }
        #endregion

    }

}

