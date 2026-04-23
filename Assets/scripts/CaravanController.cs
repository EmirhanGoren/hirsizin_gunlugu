using UnityEngine;

public class CaravanController : MonoBehaviour
{
    public GameObject interactionUI; // "E bas Günü Bitir" yazısı

    void Update()
    {
        // Eğer hedefe ulaşıldıysa ve oyuncu yakınsa yazıyı göster
        // (Bu kısmı PlayerMovement içindeki Raycast ile de yapabiliriz)
    }

    public void Interact()
    {
        if (GameManager.instance.currentMoney >= GameManager.instance.targetMoney)
        {
            GameManager.instance.FinishDay();
        }
        else
        {
            Debug.Log("Henüz yeterli paran yok kanka!");
        }
    }
}