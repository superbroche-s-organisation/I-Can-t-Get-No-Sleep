using StardewValley;
using StardewValley.Inventories;
using HarmonyLib;
using Netcode;

namespace ICGNS;

/// <summary>The normal shipping inventories aren't serialized. Global inventories are, including full item data.</summary>
internal static class ShippingStorage
{
    internal const string Prefix = SaveDataMigration.CurrentId + "/Shipping/";
    internal const string SharedKey = Prefix + "Shared";

    internal static void StoreBeforeSave()
    {
        FarmerTeam team = Game1.player.team;
        // Store both kinds of bin, so switching wallet modes overnight doesn't discard anything.
        NetRef<Inventory> shared = (NetRef<Inventory>)(AccessTools.Field(typeof(Farm), "sharedShippingBin").GetValue(Game1.getFarm())
            ?? throw new InvalidOperationException("The game's shared shipping inventory is unavailable."));
        Move(shared.Value, team.GetOrCreateGlobalInventory(SharedKey));

        foreach (Farmer farmer in Game1.getAllFarmers())
            Move(farmer.personalShippingBin.Value, team.GetOrCreateGlobalInventory(PersonalKey(farmer.UniqueMultiplayerID)));
    }

    internal static void RestoreAfterSaveOrLoad()
    {
        FarmerTeam team = Game1.player.team;
        SaveDataMigration.MigrateInventories(team);
        bool separate = team.useSeparateWallets.Value;
        // If the wallet mode changed, return retained items to the corresponding usable bin.
        if (team.globalInventories.TryGetValue(SharedKey, out Inventory? shared))
            Move(shared, Game1.getFarm().getShippingBin(Game1.MasterPlayer));

        foreach (Farmer farmer in Game1.getAllFarmers())
        {
            if (team.globalInventories.TryGetValue(PersonalKey(farmer.UniqueMultiplayerID), out Inventory? stored))
                Move(stored, separate ? farmer.personalShippingBin.Value : Game1.getFarm().getShippingBin(Game1.MasterPlayer));
        }
    }

    internal static void Move(IList<Item> source, IList<Item> destination)
    {
        if (ReferenceEquals(source, destination))
            return;

        // Detach before attaching; never clone, reprice, or recreate a modded item.
        while (source.Count > 0)
        {
            Item item = source[0];
            source.RemoveAt(0);
            if (item is null)
                continue;
            item.onDetachedFromParent();
            destination.Add(item);
        }
    }

    private static string PersonalKey(long id) => Prefix + id;
}
