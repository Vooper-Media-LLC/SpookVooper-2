using System.Threading.Tasks;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Factories;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Users;
using System.Collections.Concurrent;
using SV2.Database.Models.Items;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using SV2.Web;

namespace SV2.Managers;

public static class TransactionManager
{
    static public HashSet<string> ActiveSvids = new();

    static public ConcurrentQueue<Transaction> transactionQueue = new();

    static public async Task<bool> Run()
    {
        if (transactionQueue.IsEmpty) return false;

        Transaction tran;
        bool dequeued = transactionQueue.TryDequeue(out tran);

        if (!dequeued) return false;

        TaskResult result = await tran.ExecuteFromManager();

        tran.Result = result;

        tran.IsCompleted = true;

        string success = "SUCC";
        if (!result.Succeeded) success = "FAIL";

        Console.WriteLine($"[{success}] Processed {tran.Details} for {tran.Credits}.");

        return true;

        // Notify SignalR
        //string json = JsonConvert.SerializeObject(request);

        //await TransactionHub.Current.Clients.All.SendAsync("NotifyTransaction", json);
    }
}