using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Управляет всем UI: таймер, счёт, подсказка "Нажми E", экран конца игры, прицел.
/// Вешается на Canvas в сцене.
/// ПРИМЕЧАНИЕ: использует TextMeshPro (TMP) вместо старого Legacy Text.
/// </summary>
public class UIManager : MonoBehaviour
{
public static UIManager Instance { get; private set; }

[Header("Таймер")]
public TextMeshProUGUI timerText;
public GameObject timerPanel;

[Header("Счёт")]
public TextMeshProUGUI scoreText;

[Header("Подсказка взаимодействия")]
public GameObject interactPrompt;
public TextMeshProUGUI interactLabel;

[Header("Экран конца игры")]
public GameObject gameOverPanel;
public TextMeshProUGUI gameOverTitle;
public TextMeshProUGUI gameOverScore;
public Button restartButton;

[Header("Прицел")]
public GameObject crosshair;

[Header("Информация о фазе")]
public TextMeshProUGUI phaseLabel;

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
if (GameManager.Instance != null)
{
GameManager.Instance.onRunPhaseStart.AddListener(OnRunPhaseStart);
GameManager.Instance.onShootingPhaseStart.AddListener(OnShootingPhaseStart);
GameManager.Instance.onGameOver.AddListener(OnGameOver);
}

if (restartButton != null)
restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());

// Начальное состояние
if (interactPrompt != null) interactPrompt.SetActive(false);
if (gameOverPanel != null) gameOverPanel.SetActive(false);
if (crosshair != null) crosshair.SetActive(false);
}

void Update()
{
if (GameManager.Instance == null) return;

// Таймер
float timer = GameManager.Instance.CurrentTimer;
if (timerText != null)
timerText.text = Mathf.CeilToInt(timer).ToString();

// Счёт
if (scoreText != null)
scoreText.text = $"Очки: {GameManager.Instance.Score}";

// Прицел показываем только когда игрок на турели в фазе стрельбы
if (crosshair != null)
crosshair.SetActive(GameManager.Instance.IsShootingPhase && GameManager.Instance.PlayerOnTurret);
}

public void ShowInteractPrompt(bool show)
{
if (interactPrompt != null)
interactPrompt.SetActive(show);

if (show && interactLabel != null)
{
interactLabel.text = "Press E to seat";
}
}

void OnRunPhaseStart()
{
if (gameOverPanel != null) gameOverPanel.SetActive(false);
if (timerPanel != null) timerPanel.SetActive(true);
if (phaseLabel != null) phaseLabel.text = "Run to the turrel!!!";
if (crosshair != null) crosshair.SetActive(false);
}

void OnShootingPhaseStart()
{
if (phaseLabel != null) phaseLabel.text = "Shoot'em";
}

void OnGameOver()
{
if (gameOverPanel == null) return;

gameOverPanel.SetActive(true);

bool success = GameManager.Instance != null && GameManager.Instance.WasSuccessful;
if (gameOverTitle != null)
gameOverTitle.text = success ? "Time's up" : "AHAHAHAHAHHAH";
if (gameOverScore != null)
gameOverScore.text = $"Your score: {GameManager.Instance?.Score ?? 0}";

if (crosshair != null) crosshair.SetActive(false);
if (interactPrompt != null) interactPrompt.SetActive(false);
if (timerPanel != null) timerPanel.SetActive(false);
}

void OnDestroy()
{
if (GameManager.Instance != null)
{
GameManager.Instance.onRunPhaseStart.RemoveListener(OnRunPhaseStart);
GameManager.Instance.onShootingPhaseStart.RemoveListener(OnShootingPhaseStart);
GameManager.Instance.onGameOver.RemoveListener(OnGameOver);
}
}
}