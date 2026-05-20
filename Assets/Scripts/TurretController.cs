using System.Collections;
using UnityEngine;

/// <summary>
/// Управляет турелью: вход/выход (клавиша E), поворот ствола за камерой,
/// автоматическая стрельба при зажатии левой кнопки мыши.
/// Вешается на корневой GameObject турели.
/// </summary>
public class TurretController : MonoBehaviour
{
    [Header("Детали турели")]
    [Tooltip("Вращается горизонтально (Y)")]
    public Transform basePivot;
    [Tooltip("Вращается вертикально (X) — ствол")]
    public Transform barrelPivot;
    [Tooltip("Точка, откуда вылетают пули/луч")]
    public Transform firePoint;
    [Tooltip("Куда телепортируется игрок при посадке")]
    public Transform seatPosition;

    [Header("Параметры стрельбы")]
    public float fireRate = 0.1f;
    public float damage = 100f;
    public float range = 300f;
    public LayerMask hitLayers = ~0;

    [Header("Ограничения поворота")]
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;
    public float turretRotationSpeed = 20f;

    [Header("Взаимодействие")]
    public float interactionRange = 3f;

    [Header("Эффекты")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletTrailPrefab;
    public float trailDuration = 0.04f;

    // Приватные поля
    private bool isMounted = false;
    private bool playerInRange = false;
    private SC_FPSController playerFPS;
    private Camera playerCamera;
    private float nextFireTime;
    private int currentAmmo;
    private int maxAmmo = 300;

    void Start()
    {
        playerFPS = FindObjectOfType<SC_FPSController>();
        if (playerFPS != null)
            playerCamera = playerFPS.playerCamera;

        if (GameManager.Instance != null)
            GameManager.Instance.onGameOver.AddListener(OnGameOver);
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        if (playerFPS == null) return;

        // Проверка дистанции до игрока
        float dist = Vector3.Distance(transform.position, playerFPS.transform.position);
        bool wasInRange = playerInRange;
        playerInRange = dist <= interactionRange;

        if (playerInRange && !wasInRange)
            UIManager.Instance?.ShowInteractPrompt(true);
        else if (!playerInRange && wasInRange)
            UIManager.Instance?.ShowInteractPrompt(false);

        // Посадка / высадка по E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isMounted)
                Mount();
            else
                Dismount();
        }

        // Логика на турели
        if (isMounted && playerCamera != null)
        {
            UpdateTurretRotation();

            // Автоматический огонь при зажатии ЛКМ
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime && currentAmmo > 0)
            {
                Fire();
                currentAmmo--;
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    /// <summary>
    /// Поворачивает базу и ствол турели за камерой игрока.
    /// </summary>
    void UpdateTurretRotation()
    {
        Vector3 camForward = playerCamera.transform.forward;

        // Горизонтальный поворот базы
        Vector3 flatForward = Vector3.ProjectOnPlane(camForward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude > 0.001f && basePivot != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatForward);
            basePivot.rotation = Quaternion.Slerp(basePivot.rotation, targetRot, Time.deltaTime * turretRotationSpeed);
        }

        // Вертикальный поворот ствола
        if (barrelPivot != null && basePivot != null)
        {
            Vector3 effectiveFlat = Vector3.ProjectOnPlane(camForward, basePivot.up).normalized;
            if (effectiveFlat.sqrMagnitude > 0.001f)
            {
                float pitch = Vector3.SignedAngle(effectiveFlat, camForward, basePivot.right);
                pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
                barrelPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }
    }

    /// <summary>
    /// Стреляет из турели (рейкаст + эффекты).
    /// </summary>
    void Fire()
    {
        if (firePoint == null) return;

        Vector3 origin = firePoint.position;
        Vector3 direction = firePoint.forward;

        // Рейкаст
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitLayers))
        {
            BirdController bird = hit.collider.GetComponentInParent<BirdController>();
            if (bird != null)
            {
                bird.TakeDamage(damage);
            }

            SpawnTrailEffect(origin, hit.point);
        }
        else
        {
            SpawnTrailEffect(origin, origin + direction * range);
        }

        // Дульная вспышка
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, origin, firePoint.rotation);
            Destroy(flash, 0.1f);
        }
    }

    /// <summary>
    /// Спавнит визуальный трейсер пули.
    /// </summary>
    void SpawnTrailEffect(Vector3 from, Vector3 to)
    {
        if (bulletTrailPrefab == null)
        {
            // Создаём простой LineRenderer на лету
            StartCoroutine(CreateSimpleTrail(from, to));
        }
        else
        {
            StartCoroutine(PlayPrefabTrail(from, to));
        }
    }

    IEnumerator CreateSimpleTrail(Vector3 from, Vector3 to)
    {
        GameObject trailObj = new GameObject("BulletTrail");
        LineRenderer lr = trailObj.AddComponent<LineRenderer>();
        lr.startWidth = 0.04f;
        lr.endWidth = 0.04f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.yellow;
        lr.endColor = new Color(1f, 0.8f, 0f, 0f);
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        yield return new WaitForSeconds(trailDuration);
        Destroy(trailObj);
    }

    IEnumerator PlayPrefabTrail(Vector3 from, Vector3 to)
    {
        GameObject trail = Instantiate(bulletTrailPrefab);
        LineRenderer lr = trail.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);
        }
        yield return new WaitForSeconds(trailDuration);
        Destroy(trail);
    }

    /// <summary>
    /// Посадить игрока за турель.
    /// </summary>
    void Mount()
    {
        if (playerFPS == null) return;

        isMounted = true;
        playerFPS.SetTurretMode(true);

        // Перемещаем игрока на место посадки
        CharacterController cc = playerFPS.GetComponent<CharacterController>();
        if (cc != null && seatPosition != null)
        {
            cc.enabled = false;
            playerFPS.transform.position = seatPosition.position;
            playerFPS.transform.rotation = seatPosition.rotation;
            cc.enabled = true;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerOnTurret = true;

        UIManager.Instance?.ShowInteractPrompt(false);
    }

    /// <summary>
    /// Высадить игрока из турели.
    /// </summary>
    void Dismount()
    {
        isMounted = false;
        if (playerFPS != null)
            playerFPS.SetTurretMode(false);

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerOnTurret = false;

        UIManager.Instance?.ShowInteractPrompt(true);
    }

    void OnGameOver()
    {
        if (isMounted)
            Dismount();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.onGameOver.RemoveListener(OnGameOver);
    }
}