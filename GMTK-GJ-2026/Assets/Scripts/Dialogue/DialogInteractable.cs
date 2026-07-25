using UnityEngine;

public class DialogInteractable : MonoBehaviour, IInteractable
{

[SerializeField]
    private DialogueLine[] lines;

    public void Interact()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(lines);
    }
    public void OnInteractorDown(Transform interactor)
    {
        Interact();
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
