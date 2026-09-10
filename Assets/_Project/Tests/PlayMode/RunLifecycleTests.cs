using System.Collections;
using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ACaldeira.Tests
{
    public sealed class RunLifecycleTests
    {
        [UnityTest] public IEnumerator StressRunMenuAndRestartReturnAllEnemies()
        {
            Assert.That(Application.CanStreamedLevelBeLoaded("00_Bootstrap"), Is.True, "Run Tools > A Caldeira > Generate Prototype first.");
            yield return SceneManager.LoadSceneAsync("00_Bootstrap");
            var game = Object.FindObjectOfType<GameManager>();
            Assert.That(game, Is.Not.Null);
            float timeout = Time.realtimeSinceStartup + 20;
            while (SceneManager.GetActiveScene().name != "01_MainMenu" || game.State != GameState.MainMenu)
            { Assert.That(Time.realtimeSinceStartup, Is.LessThan(timeout)); yield return null; }
            StageSO stress = null;
            foreach (var stage in Resources.FindObjectsOfTypeAll<StageSO>()) if (stage.Id == "stress") stress = stage;
            Assert.That(stress, Is.Not.Null);
            game.StartRun(stress);
            timeout = Time.realtimeSinceStartup + 20;
            while (game.State != GameState.Playing)
            { Assert.That(Time.realtimeSinceStartup, Is.LessThan(timeout)); yield return null; }
            yield return null;
            var simulation = Object.FindObjectOfType<GameplaySimulation>();
            Assert.That(simulation.Alive, Is.GreaterThanOrEqualTo(1200));
            game.Pause(); float elapsed = simulation.Elapsed; yield return null; yield return null;
            Assert.That(simulation.Elapsed, Is.EqualTo(elapsed));
            game.Resume(); game.EndRun(); game.Retry();
            timeout = Time.realtimeSinceStartup + 20;
            while (game.State != GameState.Playing)
            { Assert.That(Time.realtimeSinceStartup, Is.LessThan(timeout)); yield return null; }
            yield return null; Assert.That(simulation.Alive, Is.GreaterThanOrEqualTo(1200));
            game.ReturnToMainMenu();
            timeout = Time.realtimeSinceStartup + 20;
            while (game.State != GameState.MainMenu)
            { Assert.That(Time.realtimeSinceStartup, Is.LessThan(timeout)); yield return null; }
            var pools = Object.FindObjectOfType<PoolManager>();
            Assert.That(pools.TryGetAvailability(stress.Waves[0].Enemies[0].Enemy.ActorPool, out int available, out int capacity), Is.True);
            Assert.That(available, Is.EqualTo(capacity));
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            var game = Object.FindObjectOfType<GameManager>();
            if (game != null) Object.Destroy(game.transform.root.gameObject);
            Time.timeScale = 1; yield return null;
        }
    }
}
