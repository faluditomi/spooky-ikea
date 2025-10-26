using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TriggerEndScene : MonoBehaviour
{
    private AIConversant aiConversant;
    private Image panelImage;
    [SerializeField] GameObject cameraPos;
    private GameObject player;
    private bool inRange = false;

    private void Awake()
    {
        aiConversant = GetComponent<AIConversant>();
        panelImage = GameObject.Find("EndGamePanel").GetComponent<Image>();
        player = GameObject.FindWithTag("Player");

        if(panelImage != null)
        {
            panelImage.gameObject.SetActive(false);
            panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 0f);
        }
    }

    private void Update()
    {
        if(inRange && Input.GetKeyDown(KeyCode.E))
        {
            aiConversant.QuitDialogue();
            player.SetActive(false);
            cameraPos.SetActive(true);

            if(panelImage != null)
            {
                panelImage.gameObject.SetActive(true);
                StartCoroutine(FadeImageIn());
            }
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

    private IEnumerator EndSceneCoroutine()
    {
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator FadeImageIn()
    {
        float elapsedTime = 0f;

        Color color = panelImage.color;

        while(elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;

            float newAlpha = Mathf.Lerp(0f, 1f, elapsedTime / 2f);

            panelImage.color = new Color(color.r, color.g, color.b, newAlpha);

            yield return null;
        }

        panelImage.color = new Color(color.r, color.g, color.b, 1f);
    }
}
