using System.Collections;
using System.Linq;
using ACaldeira.Combat;
using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Enemies;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ACaldeira.Tests
{
    public sealed class OilWeaponTests
    {
        [UnityTest]
        public IEnumerator TwoOilGlobsLandApartDamageAndSlow()
        {
            yield return SceneManager.LoadSceneAsync("00_Bootstrap");
            var game=Object.FindObjectOfType<GameManager>();
            float timeout=Time.realtimeSinceStartup+40f;
            while(game.State!=GameState.MainMenu || SceneManager.GetActiveScene().name!="01_MainMenu")
            {Assert.That(Time.realtimeSinceStartup,Is.LessThan(timeout));yield return null;}

            StageSO yard=Resources.FindObjectsOfTypeAll<StageSO>().First(stage=>stage.Id=="yard");
            game.StartRun(yard);
            while(game.State!=GameState.Playing)
            {Assert.That(Time.realtimeSinceStartup,Is.LessThan(timeout));yield return null;}

            var weapons=Object.FindObjectOfType<WeaponManager>();
            WeaponSO oil=Resources.FindObjectsOfTypeAll<WeaponSO>().First(weapon=>weapon.Id=="Oleo Cru");
            Assert.That(weapons.TryEquip(oil),Is.True);
            weapons.ApplyCandidate(oil.GetInstanceID(),false);
            weapons.ApplyCandidate(oil.GetInstanceID(),false);
            int slot=Enumerable.Range(0,8).First(index=>weapons.EquippedAt(index)==oil);
            Assert.That(weapons.TryFire(slot,Vector2.right),Is.True);

            var globs=Object.FindObjectsOfType<ProjectileActor>().Where(projectile=>projectile.IsOilInFlight).ToArray();
            Assert.That(globs.Length,Is.EqualTo(2));
            Assert.That(Vector2.Distance(globs[0].OilLanding,globs[1].OilLanding),Is.GreaterThan(1.5f));

            EnemySO tractor=yard.Waves.SelectMany(wave=>wave.Enemies).Select(entry=>entry.Enemy)
                .First(enemy=>enemy.Id=="Tractor");
            var pools=Object.FindObjectOfType<PoolManager>();
            Assert.That(pools.TrySpawn(tractor.ActorPool,globs[0].OilLanding,Quaternion.identity,out EnemyActor target),Is.True);
            target.Configure(tractor,-1,null);
            float initialHealth=target.Health;
            timeout=Time.realtimeSinceStartup+3f;
            while(target.IsSpawned && (target.Health>=initialHealth || target.EffectiveMoveSpeed>=tractor.MoveSpeed))
            {Assert.That(Time.realtimeSinceStartup,Is.LessThan(timeout));yield return null;}

            Assert.That(target.IsSpawned,Is.True);
            Assert.That(target.Health,Is.LessThan(initialHealth));
            Assert.That(target.EffectiveMoveSpeed,Is.LessThan(tractor.MoveSpeed));
            game.EndRun();
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            var game=Object.FindObjectOfType<GameManager>();
            if(game!=null)Object.Destroy(game.transform.root.gameObject);
            Time.timeScale=1f;
            yield return null;
        }
    }
}
