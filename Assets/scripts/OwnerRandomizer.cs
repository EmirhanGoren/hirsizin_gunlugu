using UnityEngine;
using System.Collections.Generic;

public class OwnerRandomizer : MonoBehaviour
{
    [Header("Ev Sahipleri Listesi")]
    public List<GameObject> owners; // Hiyerarşideki 3 ev sahibini buraya sürükleyeceğiz

    [Header("Ayarlar")]
    [Range(0, 100)]
    public float spawnChance = 75f; // Evde birinin olma ihtimali (%75 dolu, %25 boş)

    void Awake()
    {
        // Oyun başında hepsini kapatalım
        foreach (GameObject owner in owners)
        {
            owner.SetActive(false);
        }

        // Rastgele seçim yapalım
        RandomizeOwner();
    }

    void RandomizeOwner()
    {
        // 1. Şans kontrolü: Ev boş mu kalsın?
        float randomRoll = Random.Range(0f, 100f);
        
        if (randomRoll > spawnChance)
        {
            Debug.Log("Ev bu sefer boş çıktı kanka, rahat takıl!");
            return; // Fonksiyondan çık, kimseyi açma
        }

        // 2. Eğer ev doluysa, listeden rastgele birini seç
        if (owners.Count > 0)
        {
            int randomIndex = Random.Range(0, owners.Count);
            owners[randomIndex].SetActive(true);
            Debug.Log(owners[randomIndex].name + " şu an içeride, dikkatli ol!");
        }
    }
}