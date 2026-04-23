using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor; // Editörde oyunu durdurabilmek için bu kütüphane şart kanka
#endif

public class MenuManager : MonoBehaviour
{
    [Header("Paneller ve Gruplar")]
    public GameObject anaMenuGrubu;  // Butonlarımızın olduğu dikey grup
    public GameObject ayarlarPaneli; // Yeni yaptığımız ayarlar paneli

    [Header("Ayarlar Elemanları")]
    public Slider sesSlider;
    public Slider sensSlider;

    void Start()
    {
        // 1. Oyun her açıldığında ayarları hafızadan çekiyoruz
        sesSlider.value = PlayerPrefs.GetFloat("SesSeviyesi", 1f);
        sensSlider.value = PlayerPrefs.GetFloat("FareHassasiyeti", 2f);

        // Ses ayarını hemen oyuna uygula
        AudioListener.volume = sesSlider.value;

        // Başlangıçta ayarlar paneli kapalı, ana menü açık olsun
        if(ayarlarPaneli != null) ayarlarPaneli.SetActive(false);
        if(anaMenuGrubu != null) anaMenuGrubu.SetActive(true);
    }

    // --- BUTON FONKSİYONLARI ---

    public void StartGame()
    {
        Debug.Log("Mahalleye giriş yapılıyor...");
        Time.timeScale = 1f; 
        SceneManager.LoadScene(1); // Build Settings'deki 1 numaralı sahne
    }

    public void OpenSettings()
    {
        anaMenuGrubu.SetActive(false);
        ayarlarPaneli.SetActive(true);
    }

    public void CloseSettings()
    {
        // Değerleri bilgisayarın beynine (PlayerPrefs) kazı kanka
        PlayerPrefs.SetFloat("SesSeviyesi", sesSlider.value);
        PlayerPrefs.SetFloat("FareHassasiyeti", sensSlider.value);
        PlayerPrefs.Save(); 

        Debug.Log("Ayarlar diske yazıldı!");

        ayarlarPaneli.SetActive(false);
        anaMenuGrubu.SetActive(true);
    }

    public void UpdateVolume()
    {
        // Slider'ı kaydırdıkça sesi anlık değiştirir
        AudioListener.volume = sesSlider.value;
    }

    public void QuitGame()
    {
        Debug.Log("Çıkış yapılıyor...");
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}