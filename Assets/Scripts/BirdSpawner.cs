using UnityEngine;

/// <summary>
/// Спавнит птиц во время фазы стрельбы.
/// Вешается на пустой GameObject в сцене.
/// </summary>
public class BirdSpawner : MonoBehaviour
{
    [Header("Префабы птиц")]
    public GameObject[] birdPrefabs;

    [Header("Зона спавна (world space)")]
    public Vector3 spawnAreaMin = new Vector3(-40f, 10f, -40f);
    public Vector3 spawnAreaMax = new Vector3(40f, 25f, 40f);

    [Header("Зона полёта (world space)")]
    public Vector3 flightAreaMin = new Vector3(-50f, 5f, -50f);
    public Vector3 flightAreaMax = new Vector3(50f, 30f, 50f);

    [Header("Настройки спавна")]
    public float spawnInterval = 2f;
    public int maxBirdsAlive = 15;
    public int startBirdsCount = 5;     // Сколько сразу заспавнить при старте фазы

    // Приватные поля
    private float nextSpawnTime;
    private int currentBirdCount = 0;
    private bool isSpawning = false;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onShootingPhaseStart.AddListener(StartSpawning);
            GameManager.Instance.onGameOver.AddListener(StopSpawning);
        }
    }

    void Update()
    {
        if (!isSpawning) return;

        // Спавн новых птиц, если есть место
        if (currentBirdCount < maxBirdsAlive && Time.time >= nextSpawnTime)
        {
            SpawnBird();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    /// <summary>
    /// Создаёт одну птицу в случайной точке зоны спавна.
    /// </summary>
    void SpawnBird()
    {
        if (birdPrefabs == null || birdPrefabs.Length == 0)
        {
            Debug.LogWarning("BirdSpawner: нет префабов птиц!");
            return;
        }

        Vector3 spawnPos = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );

        GameObject prefab = birdPrefabs[Random.Range(0, birdPrefabs.Length)];
        GameObject birdObj = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Передаём зону полёта
        BirdController bc = birdObj.GetComponent<BirdController>();
        if (bc != null)
        {
            bc.flightAreaMin = flightAreaMin;
            bc.flightAreaMax = flightAreaMax;
        }

        currentBirdCount++;
    }

    /// <summary>
    /// Вызывается из BirdController при смерти птицы.
    /// </summary>
    public void OnBirdDestroyed()
    {
        currentBirdCount = Mathf.Max(0, currentBirdCount - 1);
    }

    /// <summary>
    /// Начинает спавн (вызывается событием GameManager).
    /// </summary>
    void StartSpawning()
    {
        isSpawning = true;
        nextSpawnTime = Time.time;

        // Заспавнить стартовых птиц сразу
        for (int i = 0; i < startBirdsCount; i++)
        {
            if (currentBirdCount >= maxBirdsAlive) break;
            SpawnBird();
        }
    }

    /// <summary>
    /// Останавливает спавн (вызывается событием GameManager).
    /// </summary>
    void StopSpawning()
    {
        isSpawning = false;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onShootingPhaseStart.RemoveListener(StartSpawning);
            GameManager.Instance.onGameOver.RemoveListener(StopSpawning);
        }
    }
}