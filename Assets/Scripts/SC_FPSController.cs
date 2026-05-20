using UnityEngine;

/// <summary>
/// FPS-контроллер от первого лица с поддержкой режима турели.
/// Когда игрок садится за турель (SetTurretMode(true)):
///   — движение отключается
///   — обзор камерой сохраняется
///   — гравитация продолжает действовать
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SC_FPSController : MonoBehaviour
{
    [Header("Скорости")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    [Header("Камера")]
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    // Приватные поля
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;

    [HideInInspector]
    public bool canMove = true;

    private bool isOnTurret = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ── Режим турели ──
        if (isOnTurret)
        {
            // Только обзор, без движения
            HandleCameraRotation();

            // Гравитация (чтобы игрок не висел в воздухе)
            if (!characterController.isGrounded)
            {
                moveDirection.y -= gravity * Time.deltaTime;
                characterController.Move(moveDirection * Time.deltaTime);
            }
            else
            {
                moveDirection.y = -2f; // Прижимаем к земле
            }
            return;
        }

        // ── Обычный FPS-режим ──

        // Направления
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0f;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0f;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Прыжок
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Гравитация
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Обзор
        if (canMove)
        {
            HandleCameraRotation();
        }
    }

    /// <summary>
    /// Вращение камеры мышью.
    /// </summary>
    void HandleCameraRotation()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        transform.rotation *= Quaternion.Euler(0f, Input.GetAxis("Mouse X") * lookSpeed, 0f);
    }

    /// <summary>
    /// Переключает режим турели. Вызывается из TurretController.
    /// </summary>
    public void SetTurretMode(bool onTurret)
    {
        isOnTurret = onTurret;

        if (onTurret)
        {
            // Сбрасываем скорость при посадке
            moveDirection = Vector3.zero;
        }
    }

    /// <summary>
    /// Находится ли игрок сейчас за турелью?
    /// </summary>
    public bool IsOnTurret()
    {
        return isOnTurret;
    }
}