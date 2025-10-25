using System.Collections;
using UnityEngine;

public class PeeTrigger : MonoBehaviour
{
    [SerializeField] private GameObject peePrompt;
    [SerializeField] private GameObject playerGameObject;
    //[SerializeField] private GameObject generatedDungeon;
    //[SerializeField] private GameObject premadeMap;
    [SerializeField] private ParticleSystem peeParticles;
    private AIConversant aiConversant;

    private bool inRange = false;

    private void Awake()
    {
        aiConversant = GetComponent<AIConversant>();
    }

    private void Start()
    {
        //generatedDungeon.SetActive(false);
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
        peePrompt.SetActive(false);
        
        //unzip sound

        yield return new WaitForSeconds(0.5f);

        peeParticles.Play();

        //pee sound

        //cutscene?

        yield return new WaitForSeconds(3f);

        //premadeMap.SetActive(false);

        //generatedDungeon.SetActive(true);

        peeParticles.Stop();

        playerGameObject.transform.position = new Vector3(0f, 1f, 4f);
        //generate map
        //dungeonGenerator.GenerateDungeon();
    }
}
