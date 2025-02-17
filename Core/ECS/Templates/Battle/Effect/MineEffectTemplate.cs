using System.Numerics;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Battle.Effect.Type.Mine;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Splash;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1486709084156)]
public class MineEffectTemplate : EffectBaseTemplate {
    protected virtual string ConfigPath => "battle/effect/mine";

    public IEntity Create(
        BattlePlayer battlePlayer,
        TimeSpan duration,
        Vector3 position,
        bool canTargetTeammates,
        float beginHideDistance,
        float hideRange,
        float triggeringArea,
        float impact,
        float minSplashDamagePercent,
        float radiusOfMaxSplashDamage,
        float radiusOfMinSplashDamage) {
        IEntity entity = Create(ConfigPath, battlePlayer, duration, true, true);

        entity.AddComponent(new MineConfigComponent(beginHideDistance, hideRange));
        entity.AddComponent(new MineEffectTriggeringAreaComponent(triggeringArea));
        entity.AddComponent(new MinePositionComponent(position));

        entity.AddComponent(new SplashImpactComponent(impact));
        entity.AddComponent(new SplashEffectComponent(canTargetTeammates));
        entity.AddComponent(new SplashWeaponComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));

        entity.AddComponent(new DamageWeakeningByDistanceComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        entity.AddComponent<DiscreteWeaponComponent>();
        entity.AddComponentFrom<UserGroupComponent>(battlePlayer.BattleUser);
        return entity;
    }
}
