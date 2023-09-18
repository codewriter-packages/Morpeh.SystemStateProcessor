# Morpeh.SystemStateProcessor [![Github license](https://img.shields.io/github/license/codewriter-packages/Morpeh.SystemStateProcessor.svg?style=flat-square)](#) [![Unity 2021.3](https://img.shields.io/badge/Unity-2021.3+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/Morpeh.SystemStateProcessor?style=flat-square)
_Reactivity for Morpeh ECS_

## How to use?

```csharp
using System;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Systems;
using UnityEngine;

[Serializable] public struct HeroComponent : IComponent { }
[Serializable] public struct HeroDamagedMarker : IComponent { }
[Serializable] public struct HeroDeadMarker : IComponent { }

public class HealthBarSystem : UpdateSystem {
    [SerializeField] private GameObject healthBarPrefab;

    private SystemStateProcessor<HealthBarSystemStateComponent> heroProcessor;

    public override void OnAwake() {
        heroProcessor = World.Filter
            .With<HeroComponent>()
            .With<HeroDamagedMarker>()
            .Without<HeroDeadMarker>()
            .ToSystemStateProcessor(CreateHealthBarForHero, RemoveHealthBarForHero);
    }

    public override void Dispose() {
        heroProcessor.Dispose();
    }

    public override void OnUpdate(float deltaTime) {
        heroProcessor.Process();
    }

    // Called when an entity has been added to the Filter
    private HealthBarSystemStateComponent CreateHealthBarForHero(Entity heroEntity) {
        var healthBar = Instantiate(healthBarPrefab);

        return new HealthBarSystemStateComponent {
            healthBar = healthBar,
        };
    }

    // Called when an entity has been removed from filter or has been destroyed
    private void RemoveHealthBarForHero(ref HealthBarSystemStateComponent state) {
        Destroy(state.healthBar);
    }

    [Serializable]
    private struct HealthBarSystemStateComponent : ISystemStateComponent {
        public GameObject healthBar;
    }
}
```

## License

Morpeh.SystemStateProcessor is [MIT licensed](./LICENSE.md).
