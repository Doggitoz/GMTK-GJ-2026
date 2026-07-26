using UnityEngine;
using UnityEngine.Events;

public class DialogInteractable : MonoBehaviour, IInteractable
{
    [SerializeField]
    private DialogueLine[] lines;

    [SerializeField]
    private bool _randomlySelectSingleLine = false;

    public UnityEvent OnDialogueEnd;

    private bool _isTalking;

    public void Interact()
    {
        if (!_isTalking && DialogueManager.Instance != null)
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
            _isTalking = true;
        }
            
    }
    public void OnInteractorDown(Transform interactor)
    {
        if (_isTalking)
        {
            DialogueManager.Instance.AdvanceDialogue();
        }
        Interact();
    }

    public void EndDialog()
    {
        _isTalking = false;
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
