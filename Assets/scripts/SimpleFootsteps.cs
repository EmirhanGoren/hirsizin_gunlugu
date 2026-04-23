using UnityEngine;

public class SimpleFootsteps : MonoBehaviour
{
    public AudioSource audioSource;
    public float stepInterval = 0.5f;
    private float timer;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Hızı konumsal değişimden hesaplıyoruz (Her hareket yönteminde çalışır)
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        float currentSpeed = distanceMoved / Time.deltaTime;
        lastPosition = transform.position;

        if (currentSpeed > 1f) // Karakter milim bile oynasa algılar
        {
            timer += Time.deltaTime;
            if (timer >= stepInterval)
            {
                PlayFootstep();
                timer = 0;
            }
        }
        else
        {
            timer = stepInterval; 
        }
    }

    void PlayFootstep()
{
    // Eğer halihazırda bir ayak sesi çalıyorsa, yenisini başlatma
    if (audioSource != null && !audioSource.isPlaying) 
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play(); // PlayOneShot yerine Play kullanarak çakışmayı önleyebilirsin
    }
}
}