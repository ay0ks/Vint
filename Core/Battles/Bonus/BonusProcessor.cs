using System.Collections.Frozen;
using Vint.Core.Battles.Bonus.Type;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;
using BonusInfo = Vint.Core.Config.MapInformation.Bonus;

namespace Vint.Core.Battles.Bonus;

public interface IBonusProcessor {
    public int GoldsDropped { get; }

    public Task Start();

    public Task Tick();

    public Task Take(BonusBox bonus, BattleTank tank);

    public Task ShareEntities(IPlayerConnection connection);

    public Task UnshareEntities(IPlayerConnection connection);

    public BonusBox? FindByEntity(IEntity bonusEntity);

    public Task<bool> ForceDropBonus(BonusType type, BattlePlayer? battlePlayer);
}

public class BonusProcessor : IBonusProcessor {
    public BonusProcessor(Battle battle, Dictionary<BonusType, IEnumerable<BonusInfo>> bonusesInfos) {
        List<BonusBox> bonuses = [];

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach ((BonusType type, IEnumerable<BonusInfo> bonusesInfo) in bonusesInfos) {
            IEnumerable<BonusBox> bonusBoxes = type switch {
                BonusType.Repair => bonusesInfo.Select(bonusInfo => new RepairBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                BonusType.Armor => bonusesInfo.Select(bonusInfo => new ArmorBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                BonusType.Damage => bonusesInfo.Select(bonusInfo => new DamageBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                BonusType.Speed => bonusesInfo.Select(bonusInfo => new SpeedBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                BonusType.Gold => bonusesInfo.Select(bonusInfo => new GoldBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                _ => throw new ArgumentException("Unexpected bonus type", nameof(bonusesInfos))
            };

            bonuses.AddRange(bonusBoxes);
        }

        Bonuses = bonuses.Shuffle().ToFrozenSet();
    }

    FrozenSet<BonusBox> Bonuses { get; }

    public int GoldsDropped { get; private set; }

    public async Task Start() {
        foreach (SupplyBox supply in Bonuses.OfType<SupplyBox>()) {
            await supply.ShareRegion();
            await supply.StateManager.SetState(
                new Cooldown(supply.StateManager, TimeSpan.FromSeconds(Random.Shared.Next(60))));
        }
    }

    public async Task Tick() {
        foreach (BonusBox bonus in Bonuses)
            await bonus.Tick();
    }

    public async Task Take(BonusBox bonus, BattleTank tank) =>
        await bonus.Take(tank);

    public async Task ShareEntities(IPlayerConnection connection) {
        List<IEntity> entities = new(Bonuses.Count * 2);

        foreach (BonusBox bonus in Bonuses) {
            if (bonus.RegionEntity == null || bonus is GoldBox && bonus.StateManager.CurrentState is None) continue;

            entities.Add(bonus.RegionEntity);

            if (bonus.Entity != null)
                entities.Add(bonus.Entity);
        }

        await connection.ShareIfUnshared(entities);
    }

    public async Task UnshareEntities(IPlayerConnection connection) {
        List<IEntity> entities = new(Bonuses.Count * 2);

        foreach (BonusBox bonus in Bonuses) {
            if (bonus.RegionEntity != null)
                entities.Add(bonus.RegionEntity);

            if (bonus.Entity != null)
                entities.Add(bonus.Entity);
        }

        await connection.UnshareIfShared(entities);
    }

    public BonusBox? FindByEntity(IEntity bonusEntity) =>
        Bonuses.FirstOrDefault(bonus => bonus.Entity == bonusEntity);

    public async Task<bool> ForceDropBonus(BonusType type, BattlePlayer? player) {
        BonusBox? bonus = Bonuses.ToArray().Shuffle().FirstOrDefault(bonus => bonus.Type == type && bonus.CanBeDropped(true));

        switch (bonus) {
            case null:
                return false;

            case GoldBox gold:
                return await ForceDropGold(gold, player);

            default:
                await bonus.Drop();
                return true;
        }
    }

    async Task<bool> ForceDropGold(GoldBox gold, BattlePlayer? player) {
        if (!gold.CanBeDropped(true))
            return false;

        await gold.Drop(player);
        GoldsDropped++;
        return true;
    }
}
