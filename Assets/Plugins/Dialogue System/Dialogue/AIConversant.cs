using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class AIConversant : MonoBehaviour
{
    #region Attributes
    [Header("Dialogue Settings")]
    [Tooltip("The dialogue that this NPC will use.")]
    [SerializeField] private Dialogue dialogue;

    [Tooltip("The dialogue will play itself without the player having to click. (Use Voice Audio Recommended)")]
    [SerializeField] private bool dialoguePlaysItself;

    [Tooltip("The dialogue will use voice audio when displayed.")]
    [SerializeField] private bool useVoiceAudio;

    [Header("Bubble")]
    [Tooltip("The dialogue image that will be displayed when dialoguing.")]
    [SerializeField] private Image dialogueImage;
    [Tooltip("The dialogue text that is inside the bubble.")]
    [SerializeField] private TMP_Text dialogueText;

    [Header("Text Settings")]
    [Tooltip("Text will be displayed with a typewriter effect.")]
    [SerializeField] private bool useTypewriterEffect;

    [SerializeField] private float typewriterSpeed = 0.02f;

    [Tooltip("Text will fade in and out.")]
    [SerializeField] private bool useFadeEffect;

    private Dialogue currentDialogue;
    private DialogueNode currentNode;
    private AudioSource audioSource;
    private DialogueTrigger[] dialogueTriggers;
    private WaitForSeconds typewriterWait;
    private StringBuilder stringBuilder = new StringBuilder();

    private List<DialogueNode> children = new List<DialogueNode>();

    private bool isDialoguing;
    #endregion

    #region MonoBehaviour Methods
    private void Awake()
    {
        if(useVoiceAudio)
        {
            audioSource = GetComponent<AudioSource>();

            if(audioSource == null)
            {
                Debug.LogWarning($"AIConversant on '{gameObject.name}' has 'Use Voice Audio' enabled, but no AudioSource component was found. Disabling voice audio to prevent errors.", this);

                useVoiceAudio = false;
            }
        }

        dialogueTriggers = GetComponents<DialogueTrigger>();

        typewriterWait = new WaitForSeconds(typewriterSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            StartDialogue();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(isDialoguing)
            {
                StartCoroutine(QuitDialogue());
            }
        }
    }
    #endregion

    //Triggers actions, if there are any, when dialoguing.
    private void TriggerActions()
    {
        foreach(DialogueTrigger trigger in dialogueTriggers)
        {
            trigger.Trigger(currentNode.GetTriggerActions());
        }
    }

    //Call this coroutine to start the dialogue.
    public void StartDialogue()
    {
        if(dialogue == null || isDialoguing) return;

        isDialoguing = true;
        currentNode = dialogue.GetRandomRootNode();

        if(currentNode == null)
        {
            Debug.LogWarning("Dialogue has no nodes.", this);

            isDialoguing = false;

            return;
        }

        StartCoroutine(FadeInImageBehaviour(dialogueImage));
        StartCoroutine(ProcessNode());
    }

    private void Next()
    {
        dialogue.GetChildren(currentNode, children);

        if(children.Count > 0)
        {
            currentNode = children[0];

            StartCoroutine(ProcessNode());
        }
        else
        {
            StartCoroutine(QuitDialogue());
        }
    }

    #region Setters
    public void SetNewDialogue(Dialogue newDialogue)
    {
        if(newDialogue != null)
        {
            dialogue = newDialogue;
        }
    }
    #endregion

    #region Coroutines
    private IEnumerator ProcessNode()
    {
        if(useTypewriterEffect)
        {
            yield return StartCoroutine(TypewriterEffectBehaviour());
        }
        else if(useFadeEffect)
        {
            yield return StartCoroutine(FadeEffectBehaviour());
        }
        else
        {
            yield return StartCoroutine(WriteTextBehaviour());
        }

        if(dialogueTriggers.Length > 0 && currentNode.GetTriggerActions().Count > 0)
        {
            TriggerActions();
        }

        bool hasAudio = useVoiceAudio && currentNode.voiceAudioClip != null;

        if(hasAudio)
        {
            audioSource.clip = currentNode.voiceAudioClip;
            audioSource.Play();
        }

        if(dialoguePlaysItself)
        {
            if(hasAudio)
            {
                yield return new WaitUntil(() => !audioSource.isPlaying);
            }
            else
            {
                float waitTime = Mathf.Max(1.0f, currentNode.GetDialogueText().Length * typewriterSpeed);

                yield return new WaitForSeconds(waitTime);
            }
        }
        else
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        Next();
    }

    private IEnumerator QuitDialogue()
    {
        //yield return new WaitForEndOfFrame();

        dialogueText.text = "";

        isDialoguing = false;

        StartCoroutine(FadeOutImageBehaviour(dialogueImage));

        currentNode = null;

        yield return null;
    }
    #endregion

    #region Text Effects
    //Writes the text with a typewriter effect.
    private IEnumerator TypewriterEffectBehaviour()
    {
        StartCoroutine(FadeInBehaviour(dialogueText));

        stringBuilder.Clear();

        dialogueText.text = "";

        string fullText = currentNode.GetDialogueText();

        foreach(char c in fullText)
        {
            if(!isDialoguing) yield break;

            stringBuilder.Append(c);

            dialogueText.text = stringBuilder.ToString();

            yield return typewriterWait;
        }
    }

    //Writes the text with a fade effect.
    private IEnumerator FadeEffectBehaviour()
    {
        dialogueText.color = new Color(dialogueText.color.r, dialogueText.color.g, dialogueText.color.b, 0f);

        dialogueText.text = currentNode.GetDialogueText();

        StartCoroutine(FadeInBehaviour(dialogueText));

        yield return new WaitForEndOfFrame();
    }

    //Writes the text without any effects.
    private IEnumerator WriteTextBehaviour()
    {
        dialogueText.color = new Color(dialogueText.color.r, dialogueText.color.g, dialogueText.color.b, 1f);

        dialogueText.text = currentNode.GetDialogueText();

        yield return new WaitForEndOfFrame();
    }

    //Fades in the text at the start of every node.
    private IEnumerator FadeInBehaviour(TMP_Text text)
    {
        Color objectColor = text.color;

        while(objectColor.a <= 1f)
        {
            objectColor.a += 0.2f * Time.deltaTime;

            text.color = objectColor;

            yield return null;
        }
    }

    //Fades in the dialogue bubble at the start of dialoguing.
    private IEnumerator FadeInImageBehaviour(Image image)
    {
        Color objectColor = image.color;

        if(objectColor.a != 0f)
        {
            objectColor.a = 0f;
        }

        while(objectColor.a <= 1f)
        {
            objectColor.a += 5f * Time.deltaTime;

            image.color = objectColor;

            yield return null;
        }
    }

    //Fades out the dialogue bubble at the end of dialoguing.
    private IEnumerator FadeOutImageBehaviour(Image image)
    {
        Color objectColor = image.color;

        if(objectColor.a != 1f)
        {
            objectColor.a = 1f;
        }

        while(objectColor.a >= 0f)
        {
            objectColor.a -= 5f * Time.deltaTime;

            image.color = objectColor;

            yield return null;
        }
    }
    #endregion
}
