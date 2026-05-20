using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central game state manager. Controls the flow:
/// 1. RunningToTurret — игрок должен добежать до турели за N секунд
/// 2. ShootingPhase — стрельба по птицам в течение N секунд
/// 3. GameOver — финал, показ результатов
/// </summary>
public enum GameState
{
WaitingToStart,
RunningToTurret,
ShootingPhase,
GameOver
}

public class GameManager : MonoBehaviour
{
public static GameManager Instance { get; private set; }

[Header("Таймеры")]
[Tooltip("Секунд, чтобы добежать до турели")]
public float runToTurretTime = 15f;
[Tooltip("Секунд на фазу стрельбы")]
public float shootingPhaseTime = 60f;

[Header("Состояние")]
public GameState currentState = GameState.WaitingToStart;

[Header("События")]
public UnityEvent onRunPhaseStart;
public UnityEvent onShootingPhaseStart;
public UnityEvent onGameOver;

// Приватные поля
private float currentTimer;
private int score;
private bool playerOnTurret = false;
private bool wasSuccessful = false;

// Публичные свойства
public float CurrentTimer => currentTimer;
public int Score => score;
public bool IsShootingPhase => currentState == GameState.ShootingPhase;
public bool PlayerOnTurret
{
get => playerOnTurret;
set => playerOnTurret = value;
}
public bool WasSuccessful => wasSuccessful;

void Awake()
{
if (Instance == null)
Instance = this;
else
{
Destroy(gameObject);
return;
}
}

void Start()
{
StartRunPhase();
}

void Update()
{
switch (currentState)
{
case GameState.RunningToTurret:
currentTimer -= Time.deltaTime;
if (currentTimer <= 0f)
{
currentTimer = 0f;
GameOver(success: false);
}
// Как только игрок сел за турель — сразу фаза стрельбы
if (playerOnTurret)
{
StartShootingPhase();
}
break;

case GameState.ShootingPhase:
currentTimer -= Time.deltaTime;
if (currentTimer <= 0f)
{
currentTimer = 0f;
GameOver(success: true);
}
break;

case GameState.GameOver:
case GameState.WaitingToStart:
default:
break;
}
}

/// <summary>
/// Запускает фазу «беги к турели».
/// </summary>
public void StartRunPhase()
{
currentState = GameState.RunningToTurret;
currentTimer = runToTurretTime;
score = 0;
playerOnTurret = false;
wasSuccessful = false;
onRunPhaseStart?.Invoke();
}

/// <summary>
/// Запускает фазу стрельбы.
/// </summary>
public void StartShootingPhase()
{
currentState = GameState.ShootingPhase;
currentTimer = shootingPhaseTime;
onShootingPhaseStart?.Invoke();
}

/// <summary>
/// Завершает игру.
/// </summary>
/// <param name="success">true — игрок успел к турели и стрелял; false — не успел добежать.</param>
public void GameOver(bool success)
{
currentState = GameState.GameOver;
wasSuccessful = success || playerOnTurret;
onGameOver?.Invoke();

// Разблокируем курсор для меню
Cursor.lockState = CursorLockMode.None;
Cursor.visible = true;
}

/// <summary>
/// Добавляет очки за убийство птицы.
/// </summary>
public void AddScore(int points)
{
score += points;
}

/// <summary>
/// Перезапускает игру (можно повесить на кнопку).
/// </summary>
public void RestartGame()
{
UnityEngine.SceneManagement.SceneManager.LoadScene(
UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
);
}
}