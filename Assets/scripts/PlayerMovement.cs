using UnityEngine;
using Seagull.Interior_I1.SceneProps;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    // ... Mevcut değişkenlerin aynen kalıyor ...
    [Header("Duvar Önleme Ayarları")]
    public float wallCheckDistance = 0.5f;
    public float wallCheckRadius = 0.3f;
    public bool isHidden = false;

    [Header("Hız Ayarları")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;

    [Header("Fare Ayarları")]
    public float mouseSensitivity = 2f; 
    public Transform playerCamera; 

    [Header("Eğilme & Kamera")]
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float standCameraY = 1.6f;
    public float standCameraZ = -0.2f; 
    public float crouchCameraY = 0.9f;
    public float crouchCameraZ = -0.4f; 
    [HideInInspector] public float attackZOffset = 0f; 

    [Header("Etkileşim & Silah")]
    public float interactionDistance = 3f;
    public GameObject batObject;

    [Header("Günü Bitirme (Karavan)")]
    public GameObject finishDayPrompt;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator anim;
    private float xRotation = 0f;
    private bool isCrouching = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();
        rb.freezeRotation = true; 
        Cursor.lockState = CursorLockMode.Locked; 

        // Başlangıçta karavan yazısını kapatalım
        if (finishDayPrompt != null) finishDayPrompt.SetActive(false);
    }

    void Update()
    {
        // --- KRİTİK DÜZELTME BURADA KANKA ---
        // GameManager'ın kullandığı anahtar kelime ile BİREBİR aynı yaptık ("FareHassasiyeti")
        mouseSensitivity = PlayerPrefs.GetFloat("FareHassasiyeti", 2.0f);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (Input.GetKeyDown(KeyCode.C)) isCrouching = !isCrouching;
        if (Input.GetKeyDown(KeyCode.E)) HandleInteraction();

        if (Input.GetMouseButtonDown(0))
        {
            if (batObject != null && batObject.activeInHierarchy) Debug.Log("Sopa ile vuruldu!");
            else HandleCollection();
        }

        CheckForCaravanUI();
        HandleMovement();
    }
    
    void CheckForCaravanUI()
    {
        if (finishDayPrompt == null) return;
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Caravan") || hit.collider.name.Contains("Caravan"))
            {
                if (GameManager.instance.currentMoney >= GameManager.instance.targetMoney)
                    finishDayPrompt.SetActive(true);
                else
                    finishDayPrompt.SetActive(false);
            }
            else finishDayPrompt.SetActive(false);
        }
        else finishDayPrompt.SetActive(false);
    }

    void HandleInteraction()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Caravan") || hit.collider.name.Contains("Caravan"))
            {
                if (GameManager.instance.currentMoney >= GameManager.instance.targetMoney)
                {
                    GameManager.instance.FinishDay();
                    return;
                }
            }
            DoorController door = hit.collider.GetComponentInParent<DoorController>();
            if (door != null) { door.Interact(); return; } 
            Shiftable shiftable = hit.collider.GetComponentInParent<Shiftable>();
            if (shiftable != null) { shiftable.Toggle(); return; }
            Rotatable rotatable = hit.collider.GetComponentInParent<Rotatable>();
            if (rotatable != null) { rotatable.Toggle(); return; }
        }
    }

    void HandleCollection()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, interactionDistance))
        {
            LootItem loot = hit.collider.GetComponentInParent<LootItem>();
            if (loot != null) { loot.Steal(); }
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = (transform.forward * z + transform.right * x).normalized;
        if (moveDir != Vector3.zero)
        {
            RaycastHit wallHit;
            Vector3 checkOrigin = transform.position + Vector3.up * (isCrouching ? crouchCameraY : standCameraY);
            if (Physics.SphereCast(checkOrigin, wallCheckRadius, moveDir, out wallHit, wallCheckDistance))
            {
                if (!wallHit.collider.isTrigger) moveDir = Vector3.zero;
            }
        }
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching && z > 0;
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        float targetCameraY = isCrouching ? crouchCameraY : standCameraY;
        float targetCameraZ = (isCrouching ? crouchCameraZ : standCameraZ) + attackZOffset;
        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        col.height = Mathf.Lerp(col.height, targetHeight, Time.deltaTime * 10f);
        col.center = Vector3.Lerp(col.center, new Vector3(0, targetHeight / 2f, 0), Time.deltaTime * 10f);
        Vector3 targetCamPos = new Vector3(0, targetCameraY, targetCameraZ);
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetCamPos, Time.deltaTime * 10f);
        if (anim != null)
        {
            float animMultiplier = isSprinting ? 2f : 1f;
            anim.SetFloat("VelX", x * animMultiplier, 0.1f, Time.deltaTime);
            anim.SetFloat("VelZ", z * animMultiplier, 0.1f, Time.deltaTime);
            anim.SetBool("IsCrouching", isCrouching);
        }
        rb.linearVelocity = new Vector3(moveDir.x * currentSpeed, rb.linearVelocity.y, moveDir.z * currentSpeed);
    }
}