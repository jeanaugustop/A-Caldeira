using System.Collections;
using System.IO;
using System.Linq;
using ACaldeira.Combat;
using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using ACaldeira.UI;
using ACaldeira.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ACaldeira.Tests
{
    public sealed class DieselpunkArtTests
    {
        [UnityTest]
        public IEnumerator ArtRunSupportsLargeMapChoicesAndRestart()
        {
            yield return SceneManager.LoadSceneAsync("00_Bootstrap");
            var game=Object.FindObjectOfType<GameManager>();
            Assert.That(game,Is.Not.Null);
            float timeout=Time.realtimeSinceStartup+40;
            while(game.State!=GameState.MainMenu || SceneManager.GetActiveScene().name!="01_MainMenu")
            { Assert.That(Time.realtimeSinceStartup,Is.LessThan(timeout));yield return null; }
            var theme=Object.FindObjectOfType<DieselpunkInterface>();
            Assert.That(theme.Art.Actors.Length,Is.EqualTo(4));
            Assert.That(theme.Art.Weapons.Length,Is.EqualTo(6));
            Assert.That(theme.Art.Accessories.Length,Is.EqualTo(8));
            Assert.That(theme.Art.Props.Length,Is.EqualTo(6));
            foreach(var icon in theme.Art.Weapons)Assert.That(icon,Is.Not.Null);
            var playerVisual=theme.Player.GetComponent<DirectionalActor>();
            Assert.That(playerVisual,Is.Not.Null);
            Assert.That(playerVisual.Sprites.Frames.Length,Is.EqualTo(8));
            Assert.That(playerVisual.Sprites.MoveFrames[8].name,Does.StartWith("Engineer-W-"));
            Assert.That(playerVisual.Sprites.MoveFrames[24].name,Does.StartWith("Engineer-E-"));
            var projectileVisuals=game.transform.root.GetComponentsInChildren<ProjectileActor>(true)
                .GroupBy(projectile=>projectile.transform.parent.name)
                .ToDictionary(group=>group.Key,group=>group.First().GetComponent<SpriteRenderer>());
            Assert.That(projectileVisuals["RivetBank"].sprite.name,Is.EqualTo("SimpleProjectile"));
            Assert.That(projectileVisuals["FlameBank"].sprite.name,Is.EqualTo("SimpleProjectile"));
            Assert.That(projectileVisuals["StakeBank"].sprite.name,Is.EqualTo("SimpleProjectile"));
            Assert.That(projectileVisuals["SawBank"].sprite.texture.name,Is.EqualTo("saw-silver-v2"));
            Capture("Menu");
            StageSO yard=null;
            foreach(var stage in Resources.FindObjectsOfTypeAll<StageSO>())if(stage.Id=="yard")yard=stage;
            Assert.That(yard,Is.Not.Null);
            Assert.That(yard.SpawnArea.width,Is.EqualTo(180));
            Assert.That(yard.SpawnArea.height,Is.EqualTo(120));
            game.StartRun(yard);
            timeout=Time.realtimeSinceStartup+40;
            while(game.State!=GameState.Playing){Assert.That(Time.realtimeSinceStartup,Is.LessThan(timeout));yield return null;}
            for(int i=0;i<90;i++)yield return null;
            Capture("Patio");
            var simulation=Object.FindObjectOfType<GameplaySimulation>();
            Assert.That(simulation.Alive,Is.GreaterThan(0));
            Assert.That(YardObstacles.IsBlocked(new Vector2(-30,20)),Is.True);
            Assert.That(YardObstacles.IsBlocked(Vector2.zero),Is.False);
            theme.Player.position=new Vector3(84,53,0);
            for(int i=0;i<20;i++)yield return null;
            Assert.That(theme.Player.position.x,Is.GreaterThan(80));
            var pools=Object.FindObjectOfType<PoolManager>();
            var drone=yard.Waves[0].Enemies[0].Enemy;
            Assert.That(pools.TrySpawn(drone.ActorPool,theme.Player.position+Vector3.right*3,Quaternion.identity,out ACaldeira.Enemies.EnemyActor target),Is.True);
            target.Configure(drone,-1,null);
            yield return null;
            Assert.That(simulation.TryAim(12,out Vector2 direction),Is.True,"A mira deve funcionar no extremo do mapa ampliado.");
            Capture("ExtremidadeDoMapa");
            theme.Progression.Gain(theme.Progression.Required);
            yield return null;
            Assert.That(game.State,Is.EqualTo(GameState.LevelUp));
            foreach(var image in theme.ChoiceIcons)Assert.That(image.sprite,Is.Not.Null);
            var instruction=theme.Reroll.transform.parent.Find("Instrucao").GetComponent<TMPro.TMP_Text>();
            Assert.That(instruction.text,Does.Contain("ESCOLHA UM CARD"));
            Assert.That(instruction.rectTransform.anchoredPosition.y,Is.LessThan(-280));
            Capture("Aprimoramentos");
            theme.Reroll.onClick.Invoke();
            Assert.That(theme.Progression.Rerolls,Is.EqualTo(2));
            theme.Progression.Choose(0);
            yield return null;
            Assert.That(game.State,Is.EqualTo(GameState.Playing));
            game.Pause();float elapsed=simulation.Elapsed;yield return null;
            Assert.That(simulation.Elapsed,Is.EqualTo(elapsed));
            game.Resume();game.EndRun();game.Retry();
            timeout=Time.realtimeSinceStartup+40;
            while(game.State!=GameState.Playing){Assert.That(Time.realtimeSinceStartup,Is.LessThan(timeout));yield return null;}
            yield return null;
            Assert.That(theme.Player.position.sqrMagnitude,Is.LessThan(1));
            ValidateAndCaptureDirections(game,theme,pools);
            game.ReturnToMainMenu();
            while(game.State!=GameState.MainMenu){Assert.That(Time.realtimeSinceStartup,Is.LessThan(timeout));yield return null;}
            LogAssert.NoUnexpectedReceived();
        }
        private static void ValidateAndCaptureDirections(GameManager game,DieselpunkInterface theme,PoolManager pools)
        {
            Time.timeScale=0;
            foreach(var enemy in Object.FindObjectsOfType<ACaldeira.Enemies.EnemyActor>())if(enemy.IsSpawned)pools.Despawn(enemy);
            var visuals=new DirectionalActor[4];visuals[0]=theme.Player.GetComponent<DirectionalActor>();
            theme.Player.position=new Vector3(-6,0,0);
            string[] ids={"Drone","Tractor","Heavy"};
            var definitions=Resources.FindObjectsOfTypeAll<EnemySO>();
            for(int i=0;i<3;i++)
            {
                EnemySO definition=null;
                foreach(var item in definitions)if(item.Id==ids[i])definition=item;
                Assert.That(definition,Is.Not.Null,ids[i]);
                Assert.That(pools.TrySpawn(definition.ActorPool,new Vector3(-2+i*5,0,0),Quaternion.identity,out ACaldeira.Enemies.EnemyActor actor),Is.True);
                actor.Configure(definition,-1,null);visuals[i+1]=actor.GetComponent<DirectionalActor>();
                Assert.That(actor.transform.localScale,Is.EqualTo(Vector3.one));
            }
            Assert.That(visuals[2].Sprites.WorldSize,Is.GreaterThan(visuals[1].Sprites.WorldSize*3));
            string[] names={"S","SW","W","NW","N","NE","E","SE"};
            for(int direction=0;direction<8;direction++)
            {
                float angle=direction*45*Mathf.Deg2Rad;
                var delta=new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle));
                foreach(var visual in visuals)
                {
                    Assert.That(visual.Sprites.Frames.Length,Is.EqualTo(8));
                    Assert.That(visual.Sprites.MoveFrames.Length,Is.EqualTo(32));
                    foreach(var frame in visual.Sprites.MoveFrames)Assert.That(frame,Is.Not.Null);
                    visual.ResetFacing();
                    visual.TickVisual(delta*.01f,.1f);
                    Assert.That(visual.Facing,Is.EqualTo(direction));
                    Assert.That(visual.MovementFrame,Is.EqualTo(0));
                    Assert.That(visual.Visual.sprite,Is.EqualTo(visual.Sprites.MoveFrames[direction*4]));
                    Assert.That(Mathf.Max(visual.Visual.bounds.size.x,visual.Visual.bounds.size.y),Is.EqualTo(visual.Sprites.WorldSize).Within(.01));
                }
                Capture("Movimento-"+names[direction]+"-1");
                for(int frame=1;frame<4;frame++)
                {
                    foreach(var visual in visuals)
                    {
                        visual.TickVisual(delta*(1.01f/visual.Sprites.AnimationFramesPerUnit),.1f);
                        Assert.That(visual.MovementFrame,Is.EqualTo(frame));
                        Assert.That(visual.Visual.sprite,Is.EqualTo(visual.Sprites.MoveFrames[direction*4+frame]));
                    }
                    if(direction==0 || direction==2 || direction==6)Capture("Movimento-"+names[direction]+"-"+(frame+1));
                }
                foreach(var visual in visuals)
                {
                    visual.TickVisual(Vector2.zero,.1f);
                    Assert.That(visual.MovementFrame,Is.EqualTo(-1));
                    Assert.That(visual.Visual.sprite,Is.EqualTo(visual.Sprites.Frames[direction]));
                }
            }
            Time.timeScale=1;
        }
        private static void Capture(string name)
        {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
            var canvas=Object.FindObjectOfType<Canvas>();
            var camera=Object.FindObjectOfType<Camera>();
            var originalMode=canvas.renderMode;var originalCamera=canvas.worldCamera;
            int originalOrder=canvas.sortingOrder;float originalPlane=canvas.planeDistance;
            var texture=new RenderTexture(1280,720,24);
            camera.targetTexture=texture;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            canvas.sortingOrder=1000;
            Canvas.ForceUpdateCanvases();camera.Render();
            var old=RenderTexture.active;RenderTexture.active=texture;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
            Directory.CreateDirectory("ArtPreview");File.WriteAllBytes("ArtPreview/"+name+".png",image.EncodeToPNG());
            RenderTexture.active=old;camera.targetTexture=null;canvas.renderMode=originalMode;canvas.worldCamera=originalCamera;
            canvas.sortingOrder=originalOrder;canvas.planeDistance=originalPlane;
            Object.Destroy(image);texture.Release();Object.Destroy(texture);
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            var game=Object.FindObjectOfType<GameManager>();
            if(game!=null)Object.Destroy(game.transform.root.gameObject);
            Time.timeScale=1;yield return null;
        }
    }
}
