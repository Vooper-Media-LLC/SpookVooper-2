using SV2.Database.Models.Entities;
using SV2.Database.Models.Users;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Items;
using SV2.Database.Models.Districts;
using SV2.Database.Models.Factories;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SV2.Database;

public static class DBCache
{
    /// <summary>
    /// The high level cache object which contains the lower level caches
    /// </summary>
    public static Dictionary<Type, ConcurrentDictionary<string, object>> HCache = new();

    public static IEnumerable<T> GetAll<T>() where T : class
    {
        var type = typeof(T);

        if (!HCache.ContainsKey(type))
            yield break;

        foreach (T item in HCache[type].Values)
            yield return item;
    }

    /// <summary>
    /// Returns true if the cache contains the item
    /// </summary>
    public static bool Contains<T>(string Id) where T : class
    {
        var type = typeof(T);

        if (!HCache.ContainsKey(typeof(T)))
            return false;

        return HCache[type].ContainsKey(Id);
    }

    /// <summary>
    /// Places an item into the cache
    /// </summary>
    public static async Task Put<T>(string Id, T? obj) where T : class
    {
        // Empty object is ignored
        if (obj == null)
            return;

        // Get the type of the item
        var type = typeof(T);

        // If there isn't a cache for this type, create one
        if (!HCache.ContainsKey(type))
            HCache.Add(type, new ConcurrentDictionary<string, object>());

        if (!HCache[type].ContainsKey(Id)) {
            HCache[type][Id] = obj;
        }
    }

    /// <summary>
    /// Returns the item for the given id, or null if it does not exist
    /// </summary>
    public static T? Get<T>(string Id) where T : class
    {
        var type = typeof(T);

        if (HCache.ContainsKey(type))
            if (HCache[type].ContainsKey(Id)) 
                return HCache[type][Id] as T;

        return null;
    }

    public static IEntity? FindEntity(string Id)
    {
        if (Id is null) {
            return null;
        }
        switch (Id.Substring(0, 1))
        {
            case "g":
                return Get<Group>(Id);
            case "u":
                return Get<User>(Id);
            default:
                return null;
        }
    }

    public static async Task LoadAsync()
    {
        //#if !DEBUG

        List<Task> tasks = new();
        foreach(Group group in VooperDB.Instance.Groups) {
            tasks.Add(DBCache.Put<Group>(group.Id, group));
        }
        foreach(User user in VooperDB.Instance.Users) {
            tasks.Add(DBCache.Put<User>(user.Id, user));
        }
        foreach(TaxPolicy policy in VooperDB.Instance.TaxPolicies) {
            tasks.Add(DBCache.Put<TaxPolicy>(policy.Id, policy));
        }
        foreach(TradeItem item in VooperDB.Instance.TradeItems) {
            tasks.Add(DBCache.Put<TradeItem>(item.Id, item));
        }
        foreach(TradeItemDefinition definition in VooperDB.Instance.TradeItemDefinitions) {
            tasks.Add(DBCache.Put<TradeItemDefinition>(definition.Id, definition));
        }
        foreach(Factory factory in VooperDB.Instance.Factories) {
            tasks.Add(DBCache.Put<Factory>(factory.Id, factory));
        }
        foreach(UBIPolicy policy in VooperDB.Instance.UBIPolicies) {
            tasks.Add(DBCache.Put<UBIPolicy>(policy.Id, policy));
        }
        foreach(District district in VooperDB.Instance.Districts) {
            tasks.Add(DBCache.Put<District>(district.Id, district));
        }
        foreach(GroupRole role in VooperDB.Instance.GroupRoles) {
            tasks.Add(DBCache.Put<GroupRole>(role.Id, role));
        }
        await Task.WhenAll(tasks);

        //#endif
    }

    public static async Task SaveAsync()
    {
        VooperDB.Instance.Groups.UpdateRange(GetAll<Group>());
        VooperDB.Instance.Users.UpdateRange(GetAll<User>());
        VooperDB.Instance.TaxPolicies.UpdateRange(GetAll<TaxPolicy>());
        VooperDB.Instance.TradeItems.UpdateRange(GetAll<TradeItem>());
        VooperDB.Instance.TradeItemDefinitions.UpdateRange(GetAll<TradeItemDefinition>());
        VooperDB.Instance.Factories.UpdateRange(GetAll<Factory>());
        VooperDB.Instance.TaxPolicies.UpdateRange(GetAll<TaxPolicy>());
        VooperDB.Instance.Districts.UpdateRange(GetAll<District>());
        await VooperDB.Instance.SaveChangesAsync();
    }
}