using System;
using System.Collections.Generic;
using System.Reflection;
using ACaldeira.Combat;
using ACaldeira.Data;
using ACaldeira.Enemies;
using ACaldeira.Pooling;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ACaldeira.Tests
{
    public sealed class PoolAndGridTests
    {
        private PoolKeySO key;
        private EnemyActor a, b;
        [SetUp] public void Setup()
        {
            key = ScriptableObject.CreateInstance<PoolKeySO>();
            a = new GameObject("A").AddComponent<EnemyActor>(); b = new GameObject("B").AddComponent<EnemyActor>();
        }
        [TearDown] public void Teardown()
        { Object.DestroyImmediate(a.gameObject); Object.DestroyImmediate(b.gameObject); Object.DestroyImmediate(key); }
        [Test] public void ExhaustionDoubleReturnAndResetPreserveCapacity()
        {
            var pool = new GenericObjectPool<EnemyActor>(key, new[] { a, b });
            Assert.That(pool.TryRent(Vector3.one, Quaternion.identity, out var first), Is.True);
            Assert.That(first.transform.position, Is.EqualTo(Vector3.one));
            Assert.That(pool.TryRent(Vector3.zero, Quaternion.identity, out var second), Is.True);
            Assert.That(pool.TryRent(Vector3.zero, Quaternion.identity, out _), Is.False);
            Assert.That(pool.Return(first), Is.True); Assert.That(pool.Return(first), Is.False);
            pool.ReturnAll(); Assert.That(pool.Available, Is.EqualTo(2));
            Assert.That(second.IsSpawned, Is.False); Assert.That(second.gameObject.activeSelf, Is.False);
        }
        [Test] public void ForeignInstanceCannotBeReturnedEvenWithSameKey()
        {
            var one = new GenericObjectPool<EnemyActor>(key, new[] { a });
            var two = new GenericObjectPool<EnemyActor>(key, new[] { b });
            two.TryRent(Vector3.zero, Quaternion.identity, out var foreign);
            Assert.That(one.Return(foreign), Is.False); Assert.That(foreign.IsSpawned, Is.True);
        }
        [Test] public void DuplicateRegistrationFailsBeforeClaimingObjects()
        {
            Assert.Throws<ArgumentException>(() => new GenericObjectPool<EnemyActor>(key, new[] { a, a }));
            Assert.DoesNotThrow(() => new GenericObjectPool<EnemyActor>(key, new[] { a, b }));
        }
        [Test] public void CallerCannotMutateAvailableStorage()
        {
            var bank = new[] { a, b }; var pool = new GenericObjectPool<EnemyActor>(key, bank); bank[1] = null;
            Assert.That(pool.TryRent(Vector3.zero, Quaternion.identity, out var rented), Is.True);
            Assert.That(rented, Is.SameAs(b));
        }
        [Test] public void AlreadyOwnedObjectsCannotJoinAnotherPool()
        {
            new GenericObjectPool<EnemyActor>(key, new[] { a });
            Assert.Throws<ArgumentException>(() => new GenericObjectPool<EnemyActor>(key, new[] { b, a }));
            Assert.DoesNotThrow(() => new GenericObjectPool<EnemyActor>(key, new[] { b }));
        }
        [Test] public void DenseCellRetainsAll1200EntitiesAndClearRemovesThem()
        {
            var grid = new SpatialGrid(1200, 4, 4, -4, -4, 2);
            for (int i = 0; i < 1200; i++) grid.Insert(i, -0.1f, -0.1f);
            int count = 0;
            for (int i = grid.Head(grid.Column(-0.1f), grid.Row(-0.1f)); i >= 0; i = grid.Next(i)) count++;
            Assert.That(count, Is.EqualTo(1200)); grid.Clear();
            Assert.That(grid.Head(1,1), Is.EqualTo(-1));
        }
        [Test] public void GridClampsWorldEdgesAndUsesFloorForNegativeCoordinates()
        {
            var grid = new SpatialGrid(4, 4, 4, -4, -4, 2);
            Assert.That(grid.Column(-0.1f), Is.EqualTo(1)); Assert.That(grid.Column(0f), Is.EqualTo(2));
            Assert.That(grid.Column(-100f), Is.Zero); Assert.That(grid.Row(100f), Is.EqualTo(3));
        }
        [Test] public void AttributeFallbacksNeverRepeatOnSameScreen()
        {
            GameObject root = new GameObject("ProgressionFallbackTest");
            UnityEngine.Random.State previousRandomState = UnityEngine.Random.state;
            try
            {
                WeaponManager weapons = root.AddComponent<WeaponManager>();
                RunProgression progression = root.AddComponent<RunProgression>();
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                typeof(WeaponManager).GetField("definitions", flags).SetValue(weapons, new WeaponSO[0]);
                typeof(RunProgression).GetField("weapons", flags).SetValue(progression, weapons);
                typeof(RunProgression).GetField("maxEquipmentSlots", flags).SetValue(progression, 0);
                MethodInfo rollOffers = typeof(RunProgression).GetMethod("RollOffers", flags);
                Assert.That(rollOffers, Is.Not.Null);
                UnityEngine.Random.InitState(20260917);

                for (int roll = 0; roll < 100; roll++)
                {
                    rollOffers.Invoke(progression, null);
                    var attributeNames = new HashSet<string>();
                    for (int slot = 0; slot < 3; slot++)
                    {
                        RunOffer offer = progression.Offer(slot);
                        Assert.That(offer, Is.Not.Null);
                        Assert.That(offer.Kind, Is.EqualTo(RunOfferKind.Attribute));
                        attributeNames.Add(offer.Title.Split(new[] { " — " }, StringSplitOptions.None)[0]);
                    }
                    Assert.That(attributeNames.Count, Is.EqualTo(3), "A mesma oferta de atributo apareceu mais de uma vez.");
                }
            }
            finally
            {
                UnityEngine.Random.state = previousRandomState;
                Object.DestroyImmediate(root);
            }
        }
    }
}
