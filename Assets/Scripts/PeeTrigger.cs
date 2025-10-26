using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PeeTrigger : MonoBehaviour
{
    [SerializeField] private GameObject peePrompt;
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private GameObject premadeMap;
    [SerializeField] private ParticleSystem peeParticles;
    [SerializeField] private PlayerController playerController;
    private AIConversant aiConversant;

    private GuardSpawner guardSpawner;

    private bool inRange = false;

    private void Awake()
    {
        aiConversant = GetComponent<AIConversant>();
        
        guardSpawner = FindFirstObjectByType<GuardSpawner>();
    }

    private void Update()
    {
        if(inRange && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(PeeCoroutine());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            inRange = true;
            aiConversant.StartDialogue();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            inRange = false;
            aiConversant.QuitDialogue();
        }
    }

    private IEnumerator PeeCoroutine()
    {        
        //unzip sound

        yield return new WaitForSeconds(0.5f);

        peeParticles.Play();
        playerController.LockMovement(false);
        AudioManager.instance.PlayOneShot(FMODEvents.instance.playerPeeing, transform.position);

        yield return new WaitForSeconds(6f);

        peeParticles.Stop();
        EventEmitters.instance.musicPhase01.Stop();

        yield return new WaitForSeconds(1.5f);

        AudioManager.instance.SetGlobalParameter("Distance", 100f, ignoreSeekSpeed: true);
        AudioManager.instance.InitializeMusic(FMODEvents.instance.musicPhase02);
        
        
        playerGameObject.transform.position = new Vector3(0f, 1f, 4f);
        guardSpawner.InitialiseGuardSpawner();
        playerController.UnlockMovement();
        premadeMap.SetActive(false);
    }
}
