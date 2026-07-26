using UnityEngine;
using UnityEngine.Events;

public class DialogInteractable : MonoBehaviour, IInteractable
{
    [SerializeField]
    private DialogueLine[] lines;

    [SerializeField]
    private bool _randomlySelectSingleLine = false;

    public UnityEvent OnDialogueEnd;

    public void Interact()
    {
        if (DialogueManager.Instance != null)
        {
            if (_randomlySelectSingleLine)
            {
                int index = Random.Range(0, lines.Length);
                DialogueManager.Instance.StartDialogue(new DialogueLine[] { lines[index] });
            } else
            {
                DialogueManager.Instance.StartDialogue(lines);
            }
            
            DialogueManager.Instance.OnDialogEnd += EndDialog;
        }
            
    }
    public void OnInteractorDown(Transform interactor)
    {
        Interact();
    }

    public void EndDialog()
    {
        DialogueManager.Instance.OnDialogEnd -= EndDialog;
        OnDialogueEnd?.Invoke();
    }

    public void OnInteractorHover(Transform interactor)
    {
        //throw new System.NotImplementedException();
    }

    public void OnInteractorLeave(Transform interactor)
    {
        //throw new System.NotImplementedException();
    }

    public void OnInteractorStay(Transform interactor)
    {
        //throw new System.NotImplementedException();
    }

    public void OnInteractorUp(Transform interactor)
    {
        //throw new System.NotImplementedException();
    }
}
