using Vint.Core.Battles.Player;
using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class KamikadzeEffect(
    TimeSpan cooldown,
    IEntity marketEntity,
    float radius,
    float minPercent,
    float maxDamage,
    float minDamage,
    float impact,
    BattleTank tank,
    int level
) : WeaponEffect(tank, level) {
    public override ModuleWeaponHandler WeaponHandler { get; protected set; } = null!;

    public override async Task Activate() {
        if (IsActive) return;

        CanBeDeactivated = false;
        Tank.Effects.Add(this);

        WeaponEntity = Entity = new KamikadzeEffectTemplate().Create(Tank.BattlePlayer,
            Duration, Battle.Properties.FriendlyFire, impact, minPercent, 0, radius);

        WeaponHandler = new KamikadzeWeaponHandler(Tank,
            cooldown,
            marketEntity,
            Entity,
            true,
            0,
            radius,
            minPercent,
            maxDamage,
            minDamage,
            int.MaxValue);

        await Share(Tank.BattlePlayer);
        Schedule(TimeSpan.FromSeconds(10), DeactivateInternal);
    }

    public override async Task Deactivate() {
        if (!IsActive || !CanBeDeactivated) return;

        Tank.Effects.TryRemove(this);
        await Unshare(Tank.BattlePlayer);

        Entity = null;
    }

    async Task DeactivateInternal() {
        CanBeDeactivated = true;
        await Deactivate();
    }

    public override async Task DeactivateByEMP() =>
        await DeactivateInternal();

    public override async Task Share(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank != Tank) return;

        await battlePlayer.PlayerConnection.Share(Entity!);
    }

    public override async Task Unshare(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank != Tank) return;

        await battlePlayer.PlayerConnection.Unshare(Entity!);
    }
}
