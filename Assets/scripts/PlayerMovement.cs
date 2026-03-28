using UnityEngine;
using Seagull.Interior_I1.SceneProps;

public class PlayerMovement : MonoBehaviour
{
    // ... Eski Hız ve Fare ayarların aynı kalıyor ...
    [Header("Duvar Önleme Ayarları")]
    public float wallCheckDistance = 0.5f; // Duvardan ne kadar uzakta durmalı?
    public float wallCheckRadius = 0.3f;   // Kafanın genişliği gibi düşün

    
    public bool isHidden = false;

    // ... Diğer Headerlar ve Değişkenler (Aynı kalıyor) ...
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
    }

    void Update()
    {
        // 1. Fare ve Etkileşim kontrolleri aynı
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

        HandleMovement();
    }

    // ... HandleInteraction ve HandleCollection aynı kalıyor ...
    void HandleInteraction()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, interactionDistance))
        {
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

        // --- YENİ: DUVAR ÖNLEME SİSTEMİ ---
        // Işın fırlatmak (Raycast) yerine küre fırlatıyoruz (SphereCast)
        // Çünkü kafa bir nokta değil, bir hacimdir.
        if (moveDir != Vector3.zero)
        {
            RaycastHit wallHit;
            // Kameranın olduğu yükseklikten bakıyoruz
            Vector3 checkOrigin = transform.position + Vector3.up * (isCrouching ? crouchCameraY : standCameraY);

            if (Physics.SphereCast(checkOrigin, wallCheckRadius, moveDir, out wallHit, wallCheckDistance))
            {
                // Eğer çarptığımız şey kapı veya duvar gibi bir engel ise hızı sıfırla
                // Bu sayede karakter duvara 'yapışmadan' bir tık önce durur
                if (!wallHit.collider.isTrigger)
                {
                    moveDir = Vector3.zero;
                }
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