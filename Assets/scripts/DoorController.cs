using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Rotasyon Ayarları")]
    public bool isOpen = false;
    public Vector3 openRotation = new Vector3(0, 90f, 0); // Açık hali (Genelde Y ekseni 90)
    public Vector3 closedRotation = new Vector3(0, 0, 0); // Kapalı hali
    public float smooth = 5f; // Açılma hızı

    [Header("Polis Ayarları")]
    public float policeDetectionRange = 4f; 
    public string policeTag = "Polis";

    private float checkTimer = 0f;

    void Update()
    {
        // --- PÜRÜZSÜZ FİZİKSEL ROTASYON ---
        // Animator yerine her karede hedef rotasyona doğru yumuşakça dönüyoruz
        Quaternion targetRot = Quaternion.Euler(isOpen ? openRotation : closedRotation);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * smooth);

        // --- POLİS KONTROLÜ ---
        if (!isOpen)
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= 0.5f) 
            {
                DetectPolice();
                checkTimer = 0f;
            }
        }
    }

    void DetectPolice()
    {
        GameObject[] polisler = GameObject.FindGameObjectsWithTag(policeTag);
        foreach (GameObject polis in polisler)
        {
            if (Vector3.Distance(transform.position, polis.transform.position) <= policeDetectionRange)
            {
                isOpen = true; // Polisi görünce kapıyı aç
                Debug.Log(gameObject.name + ": Polis baskını! Kapı açılıyor.");
                break;
            }
        }
    }

    // Oyuncu 'E'ye bastığında burası çalışacak
    public void Interact()
    {
        isOpen = !isOpen;
        Debug.Log(gameObject.name + (isOpen ? " açıldı." : " kapandı."));
    }
}