using System;
using ACaldeira.Data;
using ACaldeira.Enemies;
using ACaldeira.Pooling;
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
    }
}
