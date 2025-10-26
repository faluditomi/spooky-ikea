using UnityEngine;

//Simple door script.
public class ToggleDoor : MonoBehaviour
{
    Animator myAnim;

    bool isInZone;

    private void Start()
    {
        myAnim = GetComponent<Animator>();
    }

    private void Update()
    {
        if(isInZone && Input.GetKeyDown(KeyCode.E))
        {
            bool isOpen = myAnim.GetBool("isOpen");

            myAnim.SetBool("isOpen", !isOpen);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            isInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            isInZone = false;
        }
    }
}
