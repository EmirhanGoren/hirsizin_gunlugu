using UnityEngine;
using UnityEngine.UI; // Slider için şart
using TMPro; // TextMeshPro için şart

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Hedef Ayarları")]
    public int targetMoney = 5000;
    public int currentMoney = 0;

    [Header("UI Referansları")]
    public Slider lootProgressBar; // Oluşturduğun Slider'ı buraya sürükle
    public TextMeshProUGUI goalText; // GoalText yazısını buraya sürükle

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    // Bu fonksiyon her para toplandığında çağrılacak
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();

        // Hedefe ulaşıldı mı kontrolü
        if (currentMoney >= targetMoney)
        {
            Debug.Log("Tebrikler! Hedefe ulaştın, şimdi kaçış noktasına git!");
        }
    }

    void UpdateUI()
    {
        // 1. Barı Güncelle (Yüzde hesapla: 0 ile 1 arası)
        if (lootProgressBar != null)
        {
            float progress = (float)currentMoney / targetMoney;
            lootProgressBar.value = progress;
        }

        // 2. Yazıyı Güncelle
        if (goalText != null)
        {
            goalText.text = $"Ganimet: ${currentMoney} / ${targetMoney}";
        }
    }
}