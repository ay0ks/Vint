using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.BackHit;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(-1962420821)]
public class BackhitDefenceModule : PassiveBattleModule, IAlwaysActiveModule {
    public override string ConfigPath => "garage/module/upgrade/properties/backhitdefence";

    public override BackhitDefenceEffect GetEffect() => new(Tank, Level, Multiplier);

    float Multiplier { get; set; }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        BackhitDefenceEffect? effect = Tank.Effects.OfType<BackhitDefenceEffect>().SingleOrDefault();

        if (effect != null) return;

        await GetEffect().Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Multiplier = GetStat<ModuleBackhitModificatorEffectPropertyComponent>();
    }
}
