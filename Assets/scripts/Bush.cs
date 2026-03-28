using UnityEngine;

public class Bush : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Eğer giren objenin Tag'i "Player" ise saklanmayı aç
        if (other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.isHidden = true;
                Debug.Log("Hırsız çalıya girdi, güvende!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Çalıdan çıkınca saklanmayı kapat
        if (other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.isHidden = false;
                Debug.Log("Hırsız çalıdan çıktı, açık hedef!");
            }
        }
    }
}