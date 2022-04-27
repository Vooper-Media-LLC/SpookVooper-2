using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SV2.Database.Models.Entities;
using SV2.Workers;
using SV2.Web;

namespace SV2.Database.Models.Economy;

public enum TransactionType
{
    Loan = 1,
    // also includes trading resources
    ItemTrade = 2,
    Paycheck = 3,
    StockTrade = 4,
    // use this when the transaction does not fit the other types
    Payment = 5,
    // only issued by governmental bodies
    TaxCreditPayment = 6,
    TaxPayment = 7
}

public class Transaction
{
    [Key]
    [GuidID]
    public string Id { get; set; }
    public decimal Credits { get; set; }

    public DateTime Time { get; set; }

    [EntityId]
    public string FromId { get; set; }

    [EntityId]
    public string ToId { get; set; }
    public TransactionType transactionType { get; set; }

    [VarChar(1024)]
    public string Details { get; set; }

    [NotMapped]

    public bool IsCompleted = false;

    [NotMapped]

    public TaskResult? Result = null;

    [NotMapped]

    public bool Force = false;

    public Transaction()
    {
        
    }

    public Transaction(string fromId, string toId, decimal credits, TransactionType TransactionType, string details)
    {
        Id = Guid.NewGuid().ToString();
        Credits = credits;
        FromId = fromId;
        ToId = toId;
        Time = DateTime.UtcNow;
        transactionType = TransactionType;
        Details = details;
    }

    public async Task<TaskResult> Execute(bool force = false)
    {
        Force = force;
        TransactionManager.transactionQueue.Enqueue(this);

        while (!IsCompleted) await Task.Delay(1);

        return Result!;
    }

    public void NonAsyncExecute(bool force = false)
    {
        Force = true;
        TransactionManager.transactionQueue.Enqueue(this);
    }

    public async Task<TaskResult> ExecuteFromManager(bool Force = false)
    {

        while (TransactionManager.ActiveSvids.Contains(FromId) || TransactionManager.ActiveSvids.Contains(ToId))
        {
            await Task.Delay(1);
        }

        if (!Force && Credits < 0)
        {
            return new TaskResult(false, "Transaction must be positive.");
        }
        if (Credits == 0)
        {
            return new TaskResult(false, "Transaction must have a value.");
        }
        if (FromId == ToId)
        {
            return new TaskResult(false, $"An entity cannot send credits to itself.");
        }

        IEntity? fromEntity = IEntity.Find(FromId);
        IEntity? toEntity = IEntity.Find(ToId);

        if (fromEntity == null) { return new TaskResult(false, $"Failed to find sender {FromId}."); }
        if (toEntity == null) { return new TaskResult(false, $"Failed to find reciever {ToId}."); }

        TransactionManager.ActiveSvids.Add(FromId);
        TransactionManager.ActiveSvids.Add(ToId);

        if (!Force && fromEntity.Credits < Credits)
        {
            TransactionManager.ActiveSvids.Remove(FromId);
            TransactionManager.ActiveSvids.Remove(ToId);
            return new TaskResult(false, $"{fromEntity.Name} cannot afford to send ¢{Credits}");
        }

        decimal totaltaxpaid = 0.0m;

        if (transactionType != TransactionType.TaxPayment && transactionType != TransactionType.Loan) {

            List<TaxPolicy> policies = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == null || x.DistrictId == fromEntity.DistrictId || x.DistrictId == toEntity.DistrictId).ToList();

            // must do TAXES (don't let Etho see this)

            foreach (TaxPolicy policy in policies)
            {
                if (policy.Rate <= 0.01m)
                {
                    continue;
                }
                decimal amount = 0.0m;
                switch (policy.taxType) 
                {
                    case TaxType.Transactional:
                        amount = policy.GetTaxAmount(Credits);
                        break;
                    case TaxType.Sales:
                        if (transactionType == TransactionType.ItemTrade) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.StockBought:
                        if (transactionType == TransactionType.StockTrade) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.StockSale:
                        if (transactionType == TransactionType.StockTrade) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                    case TaxType.Payroll:
                        if (transactionType == TransactionType.Paycheck) {
                            amount = policy.GetTaxAmount(Credits);
                        }
                        break;
                }
                // continue if transaction did not match the tax policy
                if (amount == 0.0m) {
                    continue;
                }
                if (policy.DistrictId is null) {
                    string _FromId = FromId;
                    if (policy.taxType == TaxType.Sales || policy.taxType == TaxType.Transactional || policy.taxType == TaxType.Payroll) {
                        _FromId = ToId;
                    }
                    Transaction taxtrans = new Transaction(_FromId, "g-vooperia", amount, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}. Tax Id: {policy.Id}");
                    policy.Collected += amount;
                    totaltaxpaid += amount;
                    taxtrans.NonAsyncExecute(true);
                }
                else {
                    if (policy.DistrictId == fromEntity.DistrictId && policy.taxType != TaxType.Sales && policy.taxType != TaxType.Payroll && policy.taxType != TaxType.Transactional) {
                        Transaction taxtrans = new Transaction(FromId, "g-"+policy.DistrictId, amount, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}. Tax Id: {policy.Id}");
                        policy.Collected += amount;
                        totaltaxpaid += amount;
                        taxtrans.NonAsyncExecute(true);
                    }
                    else if (policy.DistrictId == toEntity.DistrictId){
                        Transaction taxtrans = new Transaction(toEntity.Id, "g-"+policy.DistrictId, amount, TransactionType.TaxPayment, $"Tax payment for transaction id: {Id}. Tax Id: {policy.Id}");
                        policy.Collected += amount;
                        totaltaxpaid += amount;
                        taxtrans.NonAsyncExecute(true);
                    }
                }
            }
        }

        fromEntity.Credits -= Credits;
        toEntity.Credits += Credits;

        if (transactionType == TransactionType.ItemTrade || transactionType == TransactionType.Paycheck || transactionType == TransactionType.Payment || transactionType == TransactionType.StockTrade)
        {
            fromEntity.TaxAbleCredits -= Credits;
            toEntity.TaxAbleCredits += Credits;
        }

        VooperDB.Instance.Transactions.AddAsync(this);

        TransactionManager.ActiveSvids.Remove(FromId);
        TransactionManager.ActiveSvids.Remove(ToId);

        return new TaskResult(true, $"Successfully sent ¢{Credits} to {toEntity.Name} with ¢{totaltaxpaid} tax.");

    }
}