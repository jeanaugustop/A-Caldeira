using UnityEngine;

namespace ACaldeira.UI
{
    [CreateAssetMenu(menuName="A Caldeira/Art/Directional sprites")]
    public sealed class DirectionalSprites : ScriptableObject
    {
        // Clockwise on screen starting down: S, SW, W, NW, N, NE, E, SE.
        public Sprite[] Frames = new Sprite[8];
        // Four consecutive movement frames for each direction: direction * 4 + phase.
        public Sprite[] MoveFrames = new Sprite[32];
        public bool[] Mirror = new bool[8];
        public float WorldSize = 1.8f;
        public float BobAmplitude = .035f;
        public float BobFrequency = 12f;
        [Min(.1f)] public float AnimationFramesPerUnit = 3f;
    }
}
