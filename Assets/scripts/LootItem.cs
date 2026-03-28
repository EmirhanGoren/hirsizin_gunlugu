using UnityEngine;
// YENİ: Eğer QuickOutline kullandıysan buraya o namespace'i eklemelisin
// Örneğin: using QuickOutline; (scriptin adına göre değişebilir)

public class LootItem : MonoBehaviour
{
    [Header("Eşya Bilgileri")]
    public string itemName;
    public int value;

    [Header("Şans Ayarları")]
    [Range(0, 100)]
    public float spawnChance = 80f;

    // YENİ: Outline component'i referansımız
    private Outline outlineComponent;
    private bool isHighlighted = false;

    // YENİ: Eski Emission kodu deaktif edildi
    // private Renderer itemRenderer;
    // private Color originalEmission;
    // private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        // Şans kontrolü
        if (Random.Range(0f, 100f) > spawnChance)
        {
            gameObject.SetActive(false);
            return;
        }

        // YENİ: Objeye ait Outline component'ini alıyoruz
        outlineComponent = GetComponent<Outline>();

        // YENİ: Eski Emission kodu deaktif edildi
        /*itemRenderer = GetComponent<Renderer>();
        if (itemRenderer != null)
        {
            itemRenderer.material.EnableKeyword("_EMISSION");
            originalEmission = itemRenderer.material.GetColor(EmissionColor);
        }*/
    }

    // YENİ: Bu fonksiyon artık Outline component'ini açıp kapatır
    public void SetHighlight(bool state)
    {
        // Eğer outline component'i yoksa hata vermesin diye kontrol et
        if (outlineComponent == null || isHighlighted == state) return;

        isHighlighted = state;

        // YENİ: Çizgiyi açmak veya kapatmak için component'i etkinleştiriyoruz
        outlineComponent.enabled = state;

        // YENİ: Eski Emission kodu deaktif edildi
        // Color targetColor = state ? Color.white * 0.4f : originalEmission;
        // itemRenderer.material.SetColor(EmissionColor, targetColor);
    }

    public void Steal()
    {
        if (GameManager.instance != null) GameManager.instance.AddMoney(value);
        Destroy(gameObject); 
    }
}