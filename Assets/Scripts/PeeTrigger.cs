using System.Collections;
using UnityEngine;

public class PeeTrigger : MonoBehaviour
{
    [SerializeField] private GameObject peePrompt;
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private GameObject premadeMap;
    [SerializeField] private ParticleSystem peeParticles;
    [SerializeField] private PlayerController playerController;
    private AIConversant aiConversant;

    private bool inRange = false;

    private void Awake()
    {
        aiConversant = GetComponent<AIConversant>();
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
        //pee sound

        yield return new WaitForSeconds(3f);

        peeParticles.Stop();
        playerGameObject.transform.position = new Vector3(0f, 1f, 4f);
        playerController.UnlockMovement();
        premadeMap.SetActive(false);
    }
}
