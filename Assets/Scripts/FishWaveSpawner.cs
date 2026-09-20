using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FishWaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FishEntry
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float weight = 1f;   // relative probability, not required to sum to 1
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public FishEntry[] fishEntries;  // weighted list of fish
        public int count = 5;
        public float spawnDelay = 0.2f;
    }

    [Header("Waves")]
    public Wave[] waves;
    public bool loopWaves = true;      // loop back to wave 0 after the last

    [Header("Procedural Scaling")]
    public bool scaleWithWaveNumber = true;
    public int countIncreasePerWave = 2;    // add this many fish per wave
    public float delayDecreasePerWave = 0f; // decrease spawn delay per wave
    public float minSpawnDelay = 0.05f;

    [Header("Spawn Area")]
    public Collider2D spawnBounds;
    public float edgeMargin = 1f;
    public float marginFromCenter = 3f;

    [Header("Input")]
    public Key spawnKey = Key.Tab;

    private int _currentWave = 0;
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        if (Keyboard.current[spawnKey].wasPressedThisFrame)
            SpawnNextWave();
    }

    public void SpawnNextWave()
    {
        if (waves == null || waves.Length == 0) return;

        int index = loopWaves
            ? _currentWave % waves.Length
            : Mathf.Min(_currentWave, waves.Length - 1);

        Wave wave = waves[index];

        // Scale count and delay based on how many waves have passed
        int scaledCount = scaleWithWaveNumber
            ? wave.count + _currentWave * countIncreasePerWave
            : wave.count;

        float scaledDelay = scaleWithWaveNumber
            ? Mathf.Max(wave.spawnDelay - _currentWave * delayDecreasePerWave, minSpawnDelay)
            : wave.spawnDelay;

        StartCoroutine(SpawnWave(wave, scaledCount, scaledDelay));

        _currentWave++;
    }

    IEnumerator SpawnWave(Wave wave, int count, float delay)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = PickFish(wave.fishEntries);

            if (prefab != null)
            {
                Vector2 pos = GetEdgeSpawnPoint();
                Instantiate(prefab, pos, Quaternion.identity);
            }

            yield return new WaitForSeconds(delay);
        }
    }

    // Weighted random pick from fish entries
    GameObject PickFish(FishEntry[] entries)
    {
        if (entries == null || entries.Length == 0) return null;

        float total = 0f;
        foreach (var e in entries)
            total += Mathf.Max(e.weight, 0f);

        if (total <= 0f)
            return entries[Random.Range(0, entries.Length)].prefab;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var e in entries)
        {
            cumulative += e.weight;
            if (roll <= cumulative)
                return e.prefab;
        }

        return entries[entries.Length - 1].prefab;
    }

    Vector2 GetEdgeSpawnPoint()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2 candidate = RandomEdgePoint();

            if (spawnBounds != null && !spawnBounds.OverlapPoint(candidate))
                continue;

            if (Vector2.Distance(candidate, spawnBounds.bounds.center) < marginFromCenter)
                continue;

            return candidate;
        }

        Bounds b = spawnBounds.bounds;
        return new Vector2(b.min.x + edgeMargin, b.center.y);
    }

    Vector2 RandomEdgePoint()
    {
        if (spawnBounds == null)
        {
            Vector2 screenMin = _cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector2 screenMax = _cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
            return PointOnRect(screenMin, screenMax);
        }

        Bounds b = spawnBounds.bounds;
        Vector2 min = new Vector2(b.min.x + edgeMargin, b.min.y + edgeMargin);
        Vector2 max = new Vector2(b.max.x - edgeMargin, b.max.y - edgeMargin);

        return PointOnRect(min, max);
    }

    Vector2 PointOnRect(Vector2 min, Vector2 max)
    {
        float w = max.x - min.x;
        float h = max.y - min.y;
        float perimeter = 2f * (w + h);
        float t = Random.Range(0f, perimeter);

        if (t < w) return new Vector2(min.x + t, min.y);
        t -= w;
        if (t < h) return new Vector2(max.x, min.y + t);
        t -= h;
        if (t < w) return new Vector2(max.x - t, max.y);
        t -= w;
        return new Vector2(min.x, max.y - t);
    }

    public void SpawnWave(int index)
    {
        if (waves == null || index >= waves.Length) return;
        Wave wave = waves[index];
        StartCoroutine(SpawnWave(wave, wave.count, wave.spawnDelay));
    }
}