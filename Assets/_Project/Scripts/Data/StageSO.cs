using System;
using UnityEngine;

namespace ACaldeira.Data
{
    [Serializable]
    public struct EnemySpawnEntry
    {
        [SerializeField] private EnemySO enemy;
        [SerializeField, Min(1)] private int weight;

        public EnemySO Enemy => enemy;
        public int Weight => weight;
    }

    [Serializable]
    public struct WaveDefinition
    {
        [SerializeField, Min(0f)] private float startTime;
        [SerializeField, Min(0f)] private float endTime;
        [SerializeField, Min(0.01f)] private float spawnInterval;
        [SerializeField, Min(1)] private int maxAlive;
        [SerializeField] private EnemySpawnEntry[] enemies;

        public float StartTime => startTime;
        public float EndTime => endTime;
        public float SpawnInterval => spawnInterval;
        public int MaxAlive => maxAlive;
        public EnemySpawnEntry[] Enemies => enemies;
    }

    [Serializable]
    public struct HazardDefinition
    {
        [SerializeField] private PoolKeySO hazardPool;
        [SerializeField, Min(0f)] private float startTime;
        [SerializeField, Min(0.01f)] private float interval;
        [SerializeField, Min(0f)] private float telegraphDuration;
        [SerializeField, Min(0f)] private float activeDuration;
        [SerializeField, Min(0f)] private float damage;

        public PoolKeySO HazardPool => hazardPool;
        public float StartTime => startTime;
        public float Interval => interval;
        public float TelegraphDuration => telegraphDuration;
        public float ActiveDuration => activeDuration;
        public float Damage => damage;
    }

    [CreateAssetMenu(menuName = "A Caldeira/Data/Stage", fileName = "Stage_")]
    public sealed class StageSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private string sceneName;
        [SerializeField] private Sprite preview;
        [SerializeField] private AudioClip music;

        [Header("Run")]
        [SerializeField, Min(1f)] private float durationSeconds = 1200f;
        [SerializeField] private Rect spawnArea = new Rect(-30f, -20f, 60f, 40f);
        [SerializeField] private WaveDefinition[] waves;
        [SerializeField] private HazardDefinition[] hazards;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public string SceneName => sceneName;
        public Sprite Preview => preview;
        public AudioClip Music => music;
        public float DurationSeconds => durationSeconds;
        public Rect SpawnArea => spawnArea;
        public WaveDefinition[] Waves => waves;
        public HazardDefinition[] Hazards => hazards;
    }
}
