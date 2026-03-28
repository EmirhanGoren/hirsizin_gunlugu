using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PoliceAI : MonoBehaviour
{
    public enum PoliceState { Patrolling, Chasing, Searching, Alarmed }
    
    [Header("Genel Durum")]
    public PoliceState currentState = PoliceState.Patrolling;

    [Header("Mesafe Ayarları")]
    public float patrolStoppingDistance = 0.5f; 
    public float ihbarStoppingDistance = 2.5f;  // Ev sahibine yaklaşma sınırı
    public float detectionRange = 12f;          // İlk fark etme mesafesi
    public float chaseRange = 25f;              // Kovalamayı bırakma mesafesi (Artırıldı!)

    [Header("Zamanlayıcılar")]
    public float searchTime = 5f;
    public float lostTargetTimeout = 3f;        // Görüşten çıkınca kaç saniye daha kovalasın?
    
    [Header("Hız Ayarları")]
    public List<Transform> waypoints; 
    public Transform player; 
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6.5f; 

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private float searchCounter = 0f;
    private float lostCounter = 0f;
    private Animator anim;
    private Vector3 lastKnownPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
        // DUVARLARA ÇARPMAYI ÖNLEYEN AYAR: Sert dönüşleri kapatıyoruz
        agent.updateRotation = false; 

        if (waypoints != null && waypoints.Count > 0) SetDestinationToWaypoint();
    }

    void Update()
    {
        bool canSee = CanSeePlayer(detectionRange);

        // --- DURUM GEÇİŞLERİ ---
        if (canSee)
        {
            currentState = PoliceState.Chasing;
            lostCounter = 0f; // Görüyorken sayacı sıfırla
        }

        switch (currentState)
        {
            case PoliceState.Patrolling: PatrolLogic(); break;
            case PoliceState.Chasing: ChaseLogic(); break;
            case PoliceState.Searching: SearchLogic(); break;
            case PoliceState.Alarmed: AlarmedLogic(); break;
        }

        // PÜRÜZSÜZ DÖNÜŞ VE ANİMASYON
        SmoothRotation();
        if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude); 
    }

    void SmoothRotation()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    public void IhbaraGit(Vector3 konum)
    {
        currentState = PoliceState.Alarmed;
        agent.stoppingDistance = ihbarStoppingDistance; // Ev sahibine mesafeli dur
        agent.speed = chaseSpeed;
        agent.SetDestination(konum);
    }

    void AlarmedLogic()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = PoliceState.Searching;
            agent.stoppingDistance = patrolStoppingDistance; 
        }
    }

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = patrolStoppingDistance;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            SetDestinationToWaypoint();
        }
    }

    void ChaseLogic()
    {
        agent.speed = chaseSpeed;
        agent.stoppingDistance = 1.2f; 
        
        // Her zaman hırsızın peşinden git
        agent.SetDestination(player.position);

        // EĞER HIRSIZ GÖRÜŞTEN ÇIKARSA VEYA ÇOK UZAKLAŞIRSA:
        if (!CanSeePlayer(chaseRange))
        {
            lostCounter += Time.deltaTime;
            // 3 saniye boyunca (lostTargetTimeout) hala bulamazsa pes et
            if (lostCounter >= lostTargetTimeout)
            {
                currentState = PoliceState.Searching;
                agent.ResetPath();
            }
        }
    }

    void SearchLogic()
    {
        agent.speed = 0;
        searchCounter += Time.deltaTime;
        if (searchCounter >= searchTime)
        {
            searchCounter = 0f;
            currentState = PoliceState.Patrolling;
            SetDestinationToWaypoint();
        }
    }

    // --- EĞİLMEYİ VE MESAFEYİ HESAPLAYAN GELİŞMİŞ GÖRÜŞ SİSTEMİ ---
    bool CanSeePlayer(float range)
    {
        if (player == null) return false;

    // YENİ ŞART: Hırsız saklanıyorsa polis onu asla göremez!
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null && pm.isHidden) return false;
        
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < range)
        {
            Vector3 policeEyes = transform.position + Vector3.up * 1.5f;
            // DÜZELTME: Ayaklar yerine gövde merkezine (0.8m) bakıyoruz
            Vector3 playerTarget = player.position + Vector3.up * 0.8f;
            Vector3 yon = (playerTarget - policeEyes).normalized;

            RaycastHit hit;
            if (Physics.Raycast(policeEyes, yon, out hit, range))
            {
                if (hit.collider.transform == player) 
                {
                    float aci = Vector3.Angle(transform.forward, yon);
                    // Takipteyken arkasına da bakabilsin, devriyeyken sadece önünü (110 derece) görsün
                    if (currentState == PoliceState.Chasing || aci < 55f) 
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void SetDestinationToWaypoint() { if (waypoints != null && waypoints.Count > 0) agent.SetDestination(waypoints[currentWaypointIndex].position); }
}