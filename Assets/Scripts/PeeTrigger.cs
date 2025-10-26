using System.Collections;
using UnityEngine;
using TMPro;
using FMODUnity;
using FMOD.Studio;

public class PeeTrigger : MonoBehaviour
{
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private GameObject premadeMap;
    [SerializeField] private ParticleSystem peeParticles;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private GameObject objectivePanel;
    private AIConversant aiConversant;

    private GuardSpawner guardSpawner;

    private bool inRange = false;

    private void Awake()
    {
        aiConversant = GetComponent<AIConversant>();
        
        guardSpawner = FindFirstObjectByType<GuardSpawner>();
    }

    private void Start()
    {
        StartCoroutine(YeyeAhhCoroutine());
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
        EventInstance musicPhase01 = AudioManager.instance.CreateEventInstance(FMODEvents.instance.musicPhase01);
        AudioManager.instance.FadeOutMusic(musicPhase01);

        yield return new WaitForSeconds(6f);
        float fade;
        RuntimeManager.StudioSystem.getParameterByName("Fade", out fade);
        Debug.Log(fade);

        peeParticles.Stop();
        

        yield return new WaitForSeconds(1.5f);

        // EventEmitters.instance.musicPhase01.Stop();
        // musicPhase01.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicPhase01.setVolume(0);
        AudioManager.instance.SetGlobalParameter("Distance", 100f, true);
        EventInstance musicPhase02 = AudioManager.instance.CreateEventInstance(FMODEvents.instance.musicPhase02);
        AudioManager.instance.SetParameter(musicPhase02, "Fade", 0f, true);
        musicPhase02.start();
        AudioManager.instance.FadeInMusic(musicPhase02);
        
        
        playerGameObject.transform.position = new Vector3(0f, 1f, 4f);
        guardSpawner.InitialiseGuardSpawner();
        playerController.UnlockMovement();
        

        objectivePanel.SetActive(true);
        objectiveText.text = "Find your bed";

        yield return new WaitForSeconds(10f);
        objectivePanel.SetActive(false);
        premadeMap.SetActive(false);
    }

    private IEnumerator YeyeAhhCoroutine()
    {
        yield return new WaitForSeconds(10f);

        objectiveText.text = "";
        objectivePanel.SetActive(false);
    }
}
