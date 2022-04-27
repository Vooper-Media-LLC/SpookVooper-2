using SV2.Database;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using SV2.Web;

namespace SV2.Workers
{
    public class EconomyWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public readonly ILogger<EconomyWorker> _logger;

        public EconomyWorker(ILogger<EconomyWorker> logger,
                            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Task task = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            List<GroupRole>? roles = DBCache.GetAll<GroupRole>().ToList();

                            foreach(GroupRole role in roles) {
                                if (role.Salary > 0.1m) {
                                    TaxCreditPolicy taxcredit = DBCache.GetAll<TaxCreditPolicy>().FirstOrDefault(x => x.DistrictId == role.Group.DistrictId && x.taxCreditType == TaxCreditType.Employee);
                                    foreach(String Id in role.Members) {
                                        Transaction tran = new Transaction(role.GroupId, Id, role.Salary, TransactionType.Paycheck, $"{role.Name} Salary");
                                        TaskResult result = await tran.Execute();
                                        if (!result.Succeeded) {
                                            // no sense to keep paying these members since the group has ran out of credits
                                            break;
                                        }
                                        if (taxcredit is not null) {
                                            Transaction TaxCreditTran = new Transaction(taxcredit.DistrictId!, role.GroupId, role.Salary*taxcredit.Rate, TransactionType.TaxCreditPayment, $"Employee Tax Credit Payment");
                                            TaxCreditTran.NonAsyncExecute();
                                        }
                                    }
                                }  
                            }

                            List<UBIPolicy>? UBIPolicies = DBCache.GetAll<UBIPolicy>().ToList();

                            foreach(UBIPolicy policy in UBIPolicies) {
                                if (policy.Rate <= 0.0m)
                                {
                                    continue;
                                }
                                List<User> effected = DBCache.GetAll<User>().ToList();
                                string fromId = "";
                                if (policy.DistrictId != null) {
                                    effected = effected.Where(x => x.DistrictId == policy.DistrictId).ToList();
                                    fromId = "g-"+policy.DistrictId;
                                }
                                else {
                                    fromId = "g-vooperia";
                                }
                                if (policy.ApplicableRank != null) {
                                    effected = effected.Where(x => x.Rank == policy.ApplicableRank).ToList();
                                }
                                foreach(User user in effected) {
                                    Transaction tran = new Transaction(fromId, user.Id, policy.Rate/24.0m, TransactionType.Paycheck, $"UBI for rank {policy.ApplicableRank.ToString()}");
                                    tran.NonAsyncExecute();
                                }
                            }

                            if (DateTime.UtcNow.Hour == 1) {
                                // every day, update credit snapchats
                                List<IEntity> entities = DBCache.GetAll<User>().ToList<IEntity>();
                                entities.AddRange(DBCache.GetAll<Group>());
                                foreach(IEntity entity in entities)
                                {
                                    if (entity.CreditSnapshots is null) {
                                        entity.CreditSnapshots = new();
                                    }
                                    entity.CreditSnapshots.Add(entity.TaxAbleCredits);
                                    await entity.DoIncomeTax();
                                }
                            }

                            await Task.Delay(1000 * 60 * 60);
                        }
                        catch(System.Exception e)
                        {
                            Console.WriteLine("FATAL ECONOMY WORKER ERROR:");
                            Console.WriteLine(e.Message);
                        }
                    }
                });

                while (!task.IsCompleted)
                {
                    _logger.LogInformation("Economy Worker running at: {time}", DateTimeOffset.Now);
                    // for right now, just save cache to database every 2 minutes
                    await DBCache.SaveAsync();
                    await Task.Delay(120_000, stoppingToken);
                }

                _logger.LogInformation("Economy Worker task stopped at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Restarting.", DateTimeOffset.Now);
            }
        }
    }
}