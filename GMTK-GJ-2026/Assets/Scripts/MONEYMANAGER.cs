using UnityEngine;

public class MONEYMANAGER : MonoBehaviour
{
    public int MONEY => Save.Manager.Instance.CurrentSave.money;
    public static MONEYMANAGER Instance;
    [SerializeField]
    GameObject _balance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            QuestionDialogueTrigger.OnCorrectAnswer += AddMoney;
            GameManager.Instance.OnGameStart += HideBalance;
            GameManager.Instance.OnGameStop += ShowBalance;
            GameManager.Instance.OnTutorialEnd += ShowBalance;

        } else
        {
            Destroy(this);
            return;
        }

       
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Destroy(this);
            QuestionDialogueTrigger.OnCorrectAnswer -= AddMoney;
        }
    }

    public bool CanAfford(int price)
    {
        return MONEY >= price;
    }

    public void LoseMoney(int amount)
    {
        Save.Manager.Instance.CurrentSave.money -= amount;
        Save.Manager.Instance.CurrentSave.money =
            Mathf.Clamp(Save.Manager.Instance.CurrentSave.money, 0, 999);

        Save.Manager.Instance.SaveGame();
    }

    public void AddMoney(int amount)
    {
        Save.Manager.Instance.CurrentSave.money += amount;
        Save.Manager.Instance.CurrentSave.money =
            Mathf.Clamp(Save.Manager.Instance.CurrentSave.money, 0, 999);

        Save.Manager.Instance.SaveGame();
    }

    [ContextMenu("Add $100")]
    public void AddHundred()
    {
        AddMoney(100);
    }

    private void HideBalance()
    {
        _balance.SetActive(false);
    }

    private void ShowBalance()
    {
        _balance.SetActive(true);
    }
}
