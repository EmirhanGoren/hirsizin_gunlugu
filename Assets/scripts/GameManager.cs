using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

[System.Serializable]
public class SaveData
{
    public int savedDay;
    public int savedTarget;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Döngü Ayarları")]
    public int dayCount = 1;
    public int targetMoney = 1000;
    public int currentMoney = 0;
    
    [Header("UI Referansları")]
    public Slider lootProgressBar;
    public TextMeshProUGUI goalText;
    public GameObject finishDayPrompt;
    
    [Header("Paneller")]
    public GameObject pausePanel;    
    public GameObject ayarlarPaneli; 
    public GameObject anaMenuPaneli;

    [Header("Ayar Referansları")] 
    public Slider volumeSlider;
    public Slider sensSlider;

    private bool isPaused = false;
    private bool isSyncing = false;
    private string savePath;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        savePath = Path.Combine(Application.persistentDataPath, "hirsiz_gunlugu.json");
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SetupSceneRoutine(scene.buildIndex));
    }

    IEnumerator SetupSceneRoutine(int buildIndex)
    {
        isSyncing = true;
        yield return null; // Unity'nin sahnede kendine gelmesi için 1 kare bekle

        FindUIElements();
        LoadSettings(); // Ayarları (Ses/Sens) hafızadan çek
        
        UpdateUI(); // SİHİRLİ SATIR: Sahne açılır açılmaz verileri ekrana basar!

        isSyncing = false;

        if (buildIndex == 0) // Ana Menü
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isPaused = false;
            
            // "Ko" (Kayıtlı Oyun) butonu varsa kontrol et
            Button btnKayitli = GameObject.Find("Ko")?.GetComponent<Button>();
            if (btnKayitli == null) btnKayitli = GameObject.Find("Kayıtlı Oyun")?.GetComponent<Button>(); 
            if (btnKayitli != null) btnKayitli.interactable = File.Exists(savePath);
        }
        else { ResumeGame(); }
    }

    void Update()
    {
        // Ana menüdeyken ESC ile pause menüsü açılmasın
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ayarlarPaneli != null && ayarlarPaneli.activeSelf) CloseSettingsFromPause();
            else
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
    }

    // --- PARA EKLEME ---
    public void AddMoney(int amount) 
    { 
        currentMoney += amount; 
        UpdateUI(); 
    }

    // --- KAYIT SİSTEMİ (JSON) ---
    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.savedDay = dayCount;
        data.savedTarget = targetMoney;

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
        Debug.Log("Kayıt Alındı (JSON): " + savePath);
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            dayCount = data.savedDay;
            targetMoney = data.savedTarget;
            currentMoney = 0; 
        }
    }

    // --- BUTON FONKSİYONLARI ---
    public void NewGame()
    {
        dayCount = 1;
        targetMoney = 1000;
        currentMoney = 0;
        SaveGame(); // İlk günü kaydet
        SceneManager.LoadScene(1);
    }

    public void ContinueGame()
    {
        LoadGame();
        SceneManager.LoadScene(1);
    }

    public void FinishDay()
    {
        if (currentMoney >= targetMoney)
        {
            dayCount++;
            targetMoney += 500;
            currentMoney = 0;
            SaveGame(); // YENİ GÜNÜN BAŞINDA KAYDET!
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // --- ARAMA VE BAĞLAMA ---
    void FindUIElements()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;

        Transform[] allTransforms = canvasObj.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            if (t.name == "PausePanel") pausePanel = t.gameObject;
            if (t.name == "AyarlarPaneli") ayarlarPaneli = t.gameObject;
            if (t.name == "ButonGrubu") anaMenuPaneli = t.gameObject;
            if (t.name == "SesSlider") volumeSlider = t.GetComponent<Slider>();
            if (t.name == "SensSlider") sensSlider = t.GetComponent<Slider>();
            if (t.name == "LootProgressBar") lootProgressBar = t.GetComponent<Slider>();
            if (t.name == "GoalText") goalText = t.GetComponent<TextMeshProUGUI>();
            if (t.name == "FinishDayPrompt") finishDayPrompt = t.gameObject;
        }

        Button[] allButtons = canvasObj.GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            if (btn.gameObject.name == "Yo" || btn.gameObject.name == "Yeni Oyun") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(NewGame); }
            else if (btn.gameObject.name == "Ko" || btn.gameObject.name == "Kayıtlı Oyun") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(ContinueGame); }
            else if (btn.gameObject.name == "Ayarlar") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OpenSettings); }
            else if (btn.gameObject.name == "Cikis") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(QuitGame); }
            else if (btn.gameObject.name == "Geri Dön" || btn.gameObject.name == "GeriButonu" || btn.gameObject.name == "Geri") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(CloseSettingsFromPause); }
            else if (btn.gameObject.name == "Ana Menü" || btn.gameObject.name == "AnaMenu") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(GoToMainMenu); }
        }

        if (volumeSlider != null) { volumeSlider.onValueChanged.RemoveAllListeners(); volumeSlider.onValueChanged.AddListener(SetVolume); }
        if (sensSlider != null) { sensSlider.onValueChanged.RemoveAllListeners(); sensSlider.onValueChanged.AddListener(SetSensitivity); }
    }

    void LoadSettings() {
        float v = PlayerPrefs.GetFloat("SesSeviyesi", 1f);
        float s = PlayerPrefs.GetFloat("FareHassasiyeti", 2f);
        AudioListener.volume = v;
        if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(v);
        if (sensSlider != null) sensSlider.SetValueWithoutNotify(s);
    }

    public void SetVolume(float v) { if(!isSyncing) { AudioListener.volume = v; PlayerPrefs.SetFloat("SesSeviyesi", v); PlayerPrefs.Save(); } }
    public void SetSensitivity(float s) { if(!isSyncing) { PlayerPrefs.SetFloat("FareHassasiyeti", s); PlayerPrefs.Save(); } }

    public void PauseGame() { if (pausePanel == null) return; isPaused = true; pausePanel.SetActive(true); Time.timeScale = 0f; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    public void ResumeGame() { isPaused = false; if(pausePanel != null) pausePanel.SetActive(false); if(ayarlarPaneli != null) ayarlarPaneli.SetActive(false); Time.timeScale = 1f; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    public void OpenSettings() { if(pausePanel != null) pausePanel.SetActive(false); if(anaMenuPaneli != null) anaMenuPaneli.SetActive(false); if(ayarlarPaneli != null) ayarlarPaneli.SetActive(true); }
    public void CloseSettingsFromPause() { if(ayarlarPaneli != null) ayarlarPaneli.SetActive(false); if(pausePanel != null) pausePanel.SetActive(true); if(anaMenuPaneli != null) anaMenuPaneli.SetActive(true); }
    public void GoToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene(0); }
    
    public void QuitGame() { 
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void UpdateUI() 
    { 
        if (lootProgressBar != null) lootProgressBar.value = (float)currentMoney / targetMoney; 
        if (goalText != null) goalText.text = $"Gün {dayCount} - Hedef: ${currentMoney} / ${targetMoney}"; 
    }
}