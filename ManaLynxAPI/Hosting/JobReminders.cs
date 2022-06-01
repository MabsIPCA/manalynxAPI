using ManaLynxAPI.Data;
using Quartz;
using ManaLynxAPI.Utils;

namespace ManaLynxAPI.Hosting
{
    public class JobReminders : IJob
    {
        private readonly IServiceProvider _provider;
        public JobReminders(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Task Execute(IJobExecutionContext context)
        {
            using (var scope = _provider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                PagamentoUtils.DailyPagamentoVerification(dbContext);
            }

            return Task.CompletedTask;
        }
    }
}
