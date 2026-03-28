using UnityEngine;
using TMPro;

public class LootSelector : MonoBehaviour
{
    public float range = 3f;
    
    [Header("Filtreleme")]
    public LayerMask lookableLayers; // Inspector'dan 'Everything' seçilecek!

    [Header("UI Referansları")]
    public GameObject infoPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI valueText;

    private LootItem lastHighlighted;

    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        // 'lookableLayers' sayesinde ışın artık her şeye (dolap, duvar, eşya) çarpıyor
        if (Physics.Raycast(ray, out hit, range, lookableLayers))
        {
            LootItem loot = hit.collider.GetComponent<LootItem>();

            // Eğer çarptığı İLK ŞEY bir LootItem ise UI'ı göster
            if (loot != null)
            {
                if (lastHighlighted != loot)
                {
                    if (lastHighlighted != null) lastHighlighted.SetHighlight(false);
                    loot.SetHighlight(true);
                    ShowUI(loot);
                    lastHighlighted = loot;
                }
            }
            else
            {
                // Eğer ışın bir dolap kapağına veya duvara çarptıysa (LootItem değilse)
                // Hemen her şeyi temizle ki arkadaki eşya görünmesin
                ClearSelection();
            }
        }
        else
        {
            // Hiçbir şeye çarpmıyorsa (boşluğa bakıyorsa) yine temizle
            ClearSelection();
        }
    }

    void ShowUI(LootItem item)
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
            nameText.text = item.itemName;
            valueText.text = "Değer: $" + item.value;
        }
    }

    void ClearSelection()
    {
        if (lastHighlighted != null)
        {
            lastHighlighted.SetHighlight(false);
            lastHighlighted = null;
        }
        if (infoPanel != null) infoPanel.SetActive(false);
    }
}