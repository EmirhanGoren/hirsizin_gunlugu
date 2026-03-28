using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject batObject;
    public Animator playerAnimator;
    public PlayerMovement movementScript;
    public Transform playerCamera;

    [Header("Ayarlar")]
    public float attackDistance = 3f;
    public float cameraForwardPush = 0.5f; // Kamera ne kadar ileri gitsin?
    public float returnTime = 0.8f;        // Ne kadar süre sonra geri gelsin?

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) batObject.SetActive(!batObject.activeSelf);

        if (batObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        if (playerAnimator != null) playerAnimator.SetTrigger("Hit");

        // Kamerayı ileri iten Coroutine
        StartCoroutine(PushCameraForward());

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, attackDistance))
        {
            EvSahibi evSahibi = hit.collider.GetComponent<EvSahibi>();
            if (evSahibi != null) evSahibi.Bayilt();
        }
    }

    IEnumerator PushCameraForward()
    {
        movementScript.attackZOffset = cameraForwardPush; // Kamerayı ileri it
        yield return new WaitForSeconds(returnTime);
        movementScript.attackZOffset = 0f; // Kamerayı eski yerine çek
    }
}