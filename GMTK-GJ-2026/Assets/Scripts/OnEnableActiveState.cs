using UnityEngine;

public class OnEnableActiveState : MonoBehaviour
{
    [System.Serializable]
    public struct ObjectState {
        public GameObject targetObject;
       public  bool shouldBeEnabled;
    }
    [SerializeField]
    ObjectState[] _states;

    void OnEnable()
    {
        foreach(var state in _states)
        {
            state.targetObject.SetActive(state.shouldBeEnabled);
        }
    }

}
