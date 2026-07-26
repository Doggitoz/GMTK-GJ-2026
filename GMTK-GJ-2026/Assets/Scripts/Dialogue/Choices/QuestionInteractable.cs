using UnityEngine;

/// <summary>
/// Put on the same GameObject as a QuestionDialogueTrigger. Starts the
/// question sequence only when the player actually interacts with it,
/// instead of automatically on scene load / object enable.
/// </summary>
[RequireComponent(typeof(QuestionDialogueTrigger))]
public class QuestionInteractable : MonoBehaviour, IInteractable
{
    private QuestionDialogueTrigger questionTrigger;

    private void Awake()
    {
        questionTrigger = GetComponent<QuestionDialogueTrigger>();
    }

    public void Interact()
    {
        questionTrigger.BeginQuestion();
    }

    public void OnInteractorDown(Transform interactor)
    {
        Interact();
    }

    public void OnInteractorHover(Transform interactor)
    {
        // no-op
    }

    public void OnInteractorLeave(Transform interactor)
    {
        // no-op
    }

    public void OnInteractorStay(Transform interactor)
    {
        // no-op
    }

    public void OnInteractorUp(Transform interactor)
    {
        // no-op
    }
}
