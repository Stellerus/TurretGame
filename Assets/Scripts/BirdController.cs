using UnityEngine;

/// <summary>
/// Управляет одной птицей: случайный полёт, получение урона, смерть.
/// Вешается на префаб птицы.
/// </summary>
public class BirdController : MonoBehaviour
{
[Header("Характеристики")]
public float health = 100f;
public float speed = 5f;
public float minSpeed = 3f;
public float maxSpeed = 10f;
public float waypointReachDistance = 2f;
public int scoreValue = 10;

[Header("Зона полёта (world space)")]
public Vector3 flightAreaMin = new Vector3(-50f, 5f, -50f);
public Vector3 flightAreaMax = new Vector3(50f, 30f, 50f);

[Header("Плавность")]
public float rotationSmoothTime = 0.2f;
public float noiseStrength = 0.5f;
public float noiseFrequency = 0.5f;

// Приватные поля
private Vector3 currentWaypoint;
private float noiseOffsetX;
private float noiseOffsetY;
private float noiseOffsetZ;
private Vector3 currentVelocity;
private float currentSpeed;

void Start()
{
noiseOffsetX = Random.Range(0f, 100f);
noiseOffsetY = Random.Range(0f, 100f);
noiseOffsetZ = Random.Range(0f, 100f);
currentSpeed = Random.Range(minSpeed, maxSpeed);
PickNewWaypoint();
}

void Update()
{
// Направление к точке
Vector3 toWaypoint = (currentWaypoint - transform.position).normalized;

// Шум Перлина для естественности
float t = Time.time * noiseFrequency;
float noiseX = (Mathf.PerlinNoise(t, noiseOffsetX) * 2f - 1f) * noiseStrength;
float noiseY = (Mathf.PerlinNoise(t, noiseOffsetY) * 2f - 1f) * noiseStrength;
float noiseZ = (Mathf.PerlinNoise(t, noiseOffsetZ) * 2f - 1f) * noiseStrength;
Vector3 noise = new Vector3(noiseX, noiseY, noiseZ);

Vector3 desiredDirection = (toWaypoint + noise).normalized;

// Перемещение
transform.position += desiredDirection * currentSpeed * Time.deltaTime;

// Плавный поворот
if (desiredDirection.sqrMagnitude > 0.001f)
{
Quaternion targetRot = Quaternion.LookRotation(desiredDirection);
transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
}

// Смена точки при достижении
if (Vector3.Distance(transform.position, currentWaypoint) < waypointReachDistance)
{
PickNewWaypoint();
}

// Если улетела далеко за границы — вернуть в зону
ClampToFlightArea();
}

/// <summary>
/// Выбирает новую случайную точку в зоне полёта.
/// </summary>
void PickNewWaypoint()
{
currentWaypoint = new Vector3(
Random.Range(flightAreaMin.x, flightAreaMax.x),
Random.Range(flightAreaMin.y, flightAreaMax.y),
Random.Range(flightAreaMin.z, flightAreaMax.z)
);
currentSpeed = Random.Range(minSpeed, maxSpeed);
}

/// <summary>
/// Мягко возвращает птицу в разрешённую зону.
/// </summary>
void ClampToFlightArea()
{
Vector3 pos = transform.position;
bool clamped = false;

if (pos.x < flightAreaMin.x) { pos.x = flightAreaMin.x; clamped = true; }
if (pos.x > flightAreaMax.x) { pos.x = flightAreaMax.x; clamped = true; }
if (pos.y < flightAreaMin.y) { pos.y = flightAreaMin.y; clamped = true; }
if (pos.y > flightAreaMax.y) { pos.y = flightAreaMax.y; clamped = true; }
if (pos.z < flightAreaMin.z) { pos.z = flightAreaMin.z; clamped = true; }
if (pos.z > flightAreaMax.z) { pos.z = flightAreaMax.z; clamped = true; }

if (clamped)
{
transform.position = pos;
PickNewWaypoint(); // Даём новое направление внутрь зоны
}
}

/// <summary>
/// Получить урон от турели.
/// </summary>
public void TakeDamage(float amount)
{
health -= amount;
if (health <= 0f)
{
Die();
}
}

/// <summary>
/// Смерть птицы: очки, эффекты, уведомление спавнера.
/// </summary>
void Die()
{
// Очки
if (GameManager.Instance != null)
GameManager.Instance.AddScore(scoreValue);

// Уведомить спавнер
BirdSpawner spawner = FindObjectOfType<BirdSpawner>();
if (spawner != null)
spawner.OnBirdDestroyed();

// TODO: добавить эффект смерти (перья, звук)
Destroy(gameObject);
}
}