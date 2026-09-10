using System;
using System.IO;
using ACaldeira.Core;
using ACaldeira.Data;
using Unity.Profiling;
using UnityEngine;

namespace ACaldeira.Simulation
{
    public sealed class PerformanceProbe : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameplaySimulation simulation;
        [SerializeField] private PoolManager pools;
        private ProfilerRecorder gc;
        private readonly float[] frameTimes = new float[7200];
        private int count;
        private long allocated;
        private int minEnemies;
        private float started;
        private bool measuring;
        public void Begin()
        {
            count = 0; allocated = 0; minEnemies = int.MaxValue; started = Time.realtimeSinceStartup;
            gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1); measuring = true;
        }
        private void LateUpdate()
        {
            if (!measuring || gameManager.State != GameState.Playing || Time.realtimeSinceStartup - started < 5f) return;
            if (count >= frameTimes.Length) return;
            frameTimes[count++] = Time.unscaledDeltaTime * 1000f;
            if (gc.Valid) allocated += gc.LastValue;
            minEnemies = Mathf.Min(minEnemies, simulation.Alive);
        }
        public void Finish()
        {
            if (!measuring) return;
            measuring = false; bool gcAvailable = gc.Valid; gc.Dispose();
            if (count == 0) return;
            Array.Sort(frameTimes, 0, count);
            float sum = 0; for (int i = 0; i < count; i++) sum += frameTimes[i];
            string report = "device=" + SystemInfo.deviceModel + "\ngpu=" + SystemInfo.graphicsDeviceName +
                "\nunity=" + Application.unityVersion + "\neditor=" + Application.isEditor +
                "\nframes=" + count + "\nmean_ms=" + sum / count + "\np95_ms=" + frameTimes[(count - 1) * 95 / 100] +
                "\ngc_counter_available=" + gcAvailable + "\ngc_bytes=" + allocated + "\nminimum_enemies=" + minEnemies +
                "\npool_exhaustions=" + pools.ExhaustedRequests;
            try { File.WriteAllText(Path.Combine(Application.persistentDataPath, "stress-report.txt"), report); }
            catch (IOException) { Debug.LogWarning("Stress report could not be saved."); }
            Debug.Log(report);
        }
        private void OnDisable() { Finish(); }
    }
}
