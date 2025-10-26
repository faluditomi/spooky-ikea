using UnityEngine;

public class ToiletAnim : MonoBehaviour
{
    private Animator animator;
    private AIConversant aiConversant;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        aiConversant = GetComponent<AIConversant>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            animator.SetTrigger("ToiletTrigger");

            if(aiConversant != null)
            {
                aiConversant.StartDialogue();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(aiConversant != null)
            {
                aiConversant.QuitDialogue();
            }
        }
    }
}
