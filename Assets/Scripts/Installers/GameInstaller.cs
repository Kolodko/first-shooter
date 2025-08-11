using System;
using System.Linq;
using System.Reflection;
using Game.Enemy;
using Game.Input;
using Game.Player;
using Game.Save;
using Game.UI;
using Game.Upgrades;
using Game.Weapons;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [Header("Settings")]
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private UpgradeSettings _upgradeSettings;
        [SerializeField] private EnemySettings _enemySettings;
        [SerializeField] private WeaponSettings _weaponSettings;

        [Header("Prefabs")] 
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private GameObject _projectilePrefab;

        [Header("Containers")]
        [SerializeField] private Transform _projectileContainer;

        public override void InstallBindings()
        {
            InstallSettings();
            InstallSignals();
            InstallServices();
            InstallFactories();
            InstallPools();
            InstallUI();
            
            InjectSceneComponents();
        }

        private void InstallSettings()
        {
            Container.BindInstance(_gameSettings).AsSingle();
            Container.BindInstance(_upgradeSettings).AsSingle();
            Container.BindInstance(_enemySettings).AsSingle();
            Container.BindInstance(_weaponSettings).AsSingle();
        }

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);
            
            Container.DeclareSignal<PlayerShootSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerDamagedSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerDeathSignal>().OptionalSubscriber();
            
            Container.DeclareSignal<EnemyDeathSignal>().OptionalSubscriber();
            Container.DeclareSignal<EnemySpawnedSignal>().OptionalSubscriber();
            Container.DeclareSignal<EnemyAttackSignal>().OptionalSubscriber();
            
            Container.DeclareSignal<UpgradePointEarnedSignal>().OptionalSubscriber();
            Container.DeclareSignal<PendingUpgradeChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<PendingUpgradesResetSignal>().OptionalSubscriber();
            Container.DeclareSignal<UpgradesAppliedSignal>().OptionalSubscriber();
            
            Container.DeclareSignal<MenuOpenedSignal>().OptionalSubscriber();
            Container.DeclareSignal<MenuClosedSignal>().OptionalSubscriber();
        }

        private void InstallServices()
        {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SaveService>().AsSingle().NonLazy();
            
            var playerController = FindObjectOfType<PlayerController>();
            
            if (playerController != null)
            {
                Container.Bind<PlayerController>().FromInstance(playerController).AsSingle();
                Container.Bind<IPlayerController>().To<PlayerController>().FromResolve();
            }
            else
            {
                Container.Bind<IPlayerController>().To<PlayerController>().FromComponentInHierarchy().AsSingle();
            }

            Container.BindInterfacesAndSelfTo<PlayerStatsService>().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<UpgradeService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WeaponService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<EnemySpawnService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameStateService>().AsSingle().NonLazy();
        }

        private void InstallFactories()
        {
            if (_projectilePrefab != null)
            {
                var projectileTransform = _projectileContainer ?? transform;

                Container.BindFactory<Vector3, Quaternion, float, Projectile, Projectile.Factory>()
                    .FromMonoPoolableMemoryPool(x => x
                        .WithInitialSize(20)
                        .FromComponentInNewPrefab(_projectilePrefab)
                        .UnderTransform(projectileTransform));
            }
        }

        private void InstallPools()
        {
            if (_enemyPrefab != null)
            {
                Container.BindMemoryPool<EnemyAI, EnemyPool>()
                    .WithInitialSize(10)
                    .FromComponentInNewPrefab(_enemyPrefab)
                    .UnderTransformGroup("Enemies");
            }
        }

        private void InstallUI()
        {
            var playerHealthUI = FindObjectOfType<PlayerHealthUI>();
            
            if (playerHealthUI != null)
            {
                Container.Bind<PlayerHealthUI>().FromInstance(playerHealthUI).AsSingle();
            }

            var upgradeMenuUI = FindObjectOfType<UpgradeMenuUI>();
            
            if (upgradeMenuUI != null)
            {
                Container.Bind<UpgradeMenuUI>().FromInstance(upgradeMenuUI).AsSingle();
            }

            var upgradeButtonUI = FindObjectOfType<UpgradeButtonUI>();
            
            if (upgradeButtonUI != null)
            {
                Container.Bind<UpgradeButtonUI>().FromInstance(upgradeButtonUI).AsSingle();
            }
        }

        private void InjectSceneComponents()
        {
            var playerController = FindObjectOfType<PlayerController>();
            
            if (playerController != null)
            {
                Container.Inject(playerController);
                Debug.Log("Injected PlayerController");
            }
            
            var upgradeMenuUI = FindObjectOfType<UpgradeMenuUI>();
            
            if (upgradeMenuUI != null)
            {
                Container.Inject(upgradeMenuUI);
                Debug.Log("Injected UpgradeMenuUI");
            }

            var playerHealthUI = FindObjectOfType<PlayerHealthUI>();
            
            if (playerHealthUI != null)
            {
                Container.Inject(playerHealthUI);
                Debug.Log("Injected PlayerHealthUI");
            }

            var upgradeButtonUI = FindObjectOfType<UpgradeButtonUI>();
            
            if (upgradeButtonUI != null)
            {
                Container.Inject(upgradeButtonUI);
                Debug.Log("Injected UpgradeButtonUI");
            }
            
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            
            foreach (var component in allMonoBehaviours)
            {
                var type = component.GetType();
                var injectFields = type.GetFields(BindingFlags.Instance |
                                                  BindingFlags.Public |
                                                  BindingFlags.NonPublic)
                    .Where(f => f.GetCustomAttributes(typeof(InjectAttribute), true).Any());

                if (injectFields.Any() &&
                    component != playerController &&
                    component != upgradeMenuUI &&
                    component != playerHealthUI &&
                    component != upgradeButtonUI)
                {
                    try
                    {
                        Container.Inject(component);
                        Debug.Log($"Injected {type.Name}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to inject {type.Name}: {e.Message}");
                    }
                }
            }
        }

        public struct PlayerShootSignal { }

        public struct PlayerDamagedSignal { public float Damage; }

        public struct PlayerDeathSignal { }

        public struct EnemyDeathSignal { public EnemyAI Enemy; }

        public struct EnemySpawnedSignal { public EnemyAI Enemy; }

        public struct EnemyAttackSignal { public float Damage; }

        public struct UpgradePointEarnedSignal { }

        public struct PendingUpgradeChangedSignal { public StatType StatType; }

        public struct PendingUpgradesResetSignal { }

        public struct UpgradesAppliedSignal { }

        public struct MenuOpenedSignal { public MenuType MenuType; }

        public struct MenuClosedSignal { public MenuType MenuType; }
    }
}