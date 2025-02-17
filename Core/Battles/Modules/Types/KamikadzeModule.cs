using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(-1603423529)]
public class KamikadzeModule : TriggerBattleModule, IDeathModule {
    public override string ConfigPath => "garage/module/upgrade/properties/kamikadze";

    public override KamikadzeEffect GetEffect() => new(Cooldown, MarketEntity, Radius, MinPercent, MaxDamage, MinDamage, Impact, Tank, Level);

    float Impact { get; set; }
    float Radius { get; set; }
    float MinPercent { get; set; }
    float MinDamage { get; set; }
    float MaxDamage { get; set; }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Impact = GetStat<ModuleEffectImpactPropertyComponent>();
        Radius = GetStat<ModuleEffectSplashRadiusPropertyComponent>();
        MinPercent = GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>() * 100;
        MinDamage = GetStat<ModuleEffectMinDamagePropertyComponent>();
        MaxDamage = GetStat<ModuleEffectMaxDamagePropertyComponent>();
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        KamikadzeEffect? effect = Tank.Effects.OfType<KamikadzeEffect>().SingleOrDefault();

        if (effect != null) return;

        await base.Activate();
        await GetEffect().Activate();
    }

    public Task OnDeath() => Activate();
}
