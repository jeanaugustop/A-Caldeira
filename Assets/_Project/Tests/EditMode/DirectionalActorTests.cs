using ACaldeira.UI;
using NUnit.Framework;
using UnityEngine;

namespace ACaldeira.Tests
{
    public sealed class DirectionalActorTests
    {
        [TestCase(0,-1,0)] [TestCase(-1,-1,1)] [TestCase(-1,0,2)] [TestCase(-1,1,3)]
        [TestCase(0,1,4)] [TestCase(1,1,5)] [TestCase(1,0,6)] [TestCase(1,-1,7)]
        public void ResolvesEightDirections(float x,float y,int expected)
        {Assert.That(DirectionalActor.DirectionIndex(new Vector2(x,y)),Is.EqualTo(expected));}

        [Test] public void IdlePauseAndPoolResetPreserveExpectedFacing()
        {
            var actor=new GameObject("Test actor");var child=new GameObject("Visual");child.transform.SetParent(actor.transform);
            var visual=child.AddComponent<SpriteRenderer>();var config=ScriptableObject.CreateInstance<DirectionalSprites>();
            var directional=actor.AddComponent<DirectionalActor>();directional.Visual=visual;directional.Sprites=config;
            directional.TickVisual(Vector2.right,.1f);Assert.That(directional.Facing,Is.EqualTo(6));
            directional.TickVisual(Vector2.zero,.1f);Assert.That(directional.Facing,Is.EqualTo(6));
            directional.TickVisual(Vector2.up,0);Assert.That(directional.Facing,Is.EqualTo(6));
            directional.ResetFacing();Assert.That(directional.Facing,Is.EqualTo(0));
            Object.DestroyImmediate(actor);Object.DestroyImmediate(config);
        }

        [Test] public void UsesMovementFramesAndReturnsToIdle()
        {
            var actor=new GameObject("Animated actor");var child=new GameObject("Visual");child.transform.SetParent(actor.transform);
            var visual=child.AddComponent<SpriteRenderer>();var config=ScriptableObject.CreateInstance<DirectionalSprites>();
            var texture=new Texture2D(2,2);var idle=Sprite.Create(texture,new Rect(0,0,2,2),Vector2.one*.5f);
            config.Frames=new Sprite[8];config.MoveFrames=new Sprite[32];
            for(int i=0;i<8;i++)config.Frames[i]=idle;
            for(int i=0;i<32;i++)config.MoveFrames[i]=Sprite.Create(texture,new Rect(0,0,2,2),Vector2.one*.5f);
            config.AnimationFramesPerUnit=4;
            var directional=actor.AddComponent<DirectionalActor>();directional.Visual=visual;directional.Sprites=config;directional.ResetFacing();
            directional.TickVisual(Vector2.right*.3f,.1f);
            Assert.That(directional.Facing,Is.EqualTo(6));Assert.That(directional.MovementFrame,Is.EqualTo(1));
            Assert.That(visual.sprite,Is.EqualTo(config.MoveFrames[25]));
            directional.TickVisual(Vector2.zero,.1f);Assert.That(directional.MovementFrame,Is.EqualTo(-1));Assert.That(visual.sprite,Is.EqualTo(idle));
            Object.DestroyImmediate(actor);Object.DestroyImmediate(config);Object.DestroyImmediate(texture);
        }
    }
}
