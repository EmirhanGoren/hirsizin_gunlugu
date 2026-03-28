using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EvSahibi : MonoBehaviour
{
    [Header("Algılama Ayarları")]
    private Transform hirsiz; // Artık private yaptık, Start'ta bulacağız.
    public float gorüşMesafesi = 10f;
    public float gorüşAcisi = 110f;

    [Header("Polis Ayarları")]
    public string polisTag = "Polis"; 
    public float ihbarGecikmesi = 7f; 

    private Animator anim;
    private bool isKnockedOut = false;
    private bool ihbarVerildi = false;

    void Start()
    {
        anim = GetComponent<Animator>();

        // YENİ: Sahnede "Player" tag'ine sahip objeyi otomatik bulur.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            hirsiz = playerObj.transform;
        }
        else
        {
            Debug.LogError("Sahnede 'Player' tag'ine sahip bir obje bulunamadı kanka!");
        }
    }

    void Update()
    {
        if (isKnockedOut || ihbarVerildi) return;

        if (HirsiziGorduMu())
        {
            ihbarVerildi = true;
            StartCoroutine(IhbarSureci());
        }
    }

    bool HirsiziGorduMu()
    {
        if (hirsiz == null) return false;

        // --- YENİ: ÇALIDA SAKLANMA KONTROLÜ ---
        // Hırsız çalıdaysa ev sahibi onu asla göremez!
        PlayerMovement pm = hirsiz.GetComponent<PlayerMovement>();
        if (pm != null && pm.isHidden) return false;

        float mesafe = Vector3.Distance(transform.position, hirsiz.position);

        if (mesafe < gorüşMesafesi)
        {
            Vector3 yon = (hirsiz.position - transform.position).normalized;
            float aci = Vector3.Angle(transform.forward, yon);

            if (aci < gorüşAcisi / 2f)
            {
                RaycastHit hit;
                // Polisteki gibi göz hizasından (1.5m) hırsızın gövdesine (0.8m) bakıyoruz
                Vector3 startPos = transform.position + Vector3.up * 1.5f;
                Vector3 targetPos = hirsiz.position + Vector3.up * 0.8f;
                Vector3 lookYon = (targetPos - startPos).normalized;

                if (Physics.Raycast(startPos, lookYon, out hit, gorüşMesafesi))
                {
                    if (hit.collider.transform == hirsiz) 
                    {
                        return true; 
                    }
                }
            }
        }
        return false;
    }

    IEnumerator IhbarSureci()
    {
        Debug.Log("Hırsız tespit edildi! Telefon animasyonu başlıyor...");
        
        if (anim != null) anim.SetTrigger("TelefonArama");

        yield return new WaitForSeconds(ihbarGecikmesi);

        Debug.Log("İhbar süresi doldu, en yakın polis aranıyor!");
        PolisiCagir();
    }

    public void PolisiCagir()
    {
        GameObject[] polisler = GameObject.FindGameObjectsWithTag(polisTag);
        
        if (polisler.Length == 0)
        {
            Debug.LogWarning("Sahnede 'Polis' tag'li obje bulunamadı!");
            return;
        }

        GameObject enYakinPolis = null;
        float enKisaMesafe = Mathf.Infinity;

        foreach (GameObject polis in polisler)
        {
            float mesafe = Vector3.Distance(transform.position, polis.transform.position);
            if (mesafe < enKisaMesafe)
            {
                enKisaMesafe = mesafe;
                enYakinPolis = polis;
            }
        }

        if (enYakinPolis != null)
        {
            PoliceAI pAI = enYakinPolis.GetComponent<PoliceAI>();
            if (pAI != null)
            {
                pAI.IhbaraGit(transform.position); 
                Debug.Log(enYakinPolis.name + " doğrudan ev sahibinin yanına geliyor!");
            }
        }
    }

    public void Bayilt()
    {
        if (isKnockedOut) return;
        isKnockedOut = true;

        StopAllCoroutines(); 

        if (anim != null) anim.SetTrigger("Bayilma");

        if (GetComponent<NavMeshAgent>() != null) GetComponent<NavMeshAgent>().enabled = false;
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false; 
        
        var rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
    }
}