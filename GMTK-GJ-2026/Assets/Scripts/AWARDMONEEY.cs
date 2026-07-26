using UnityEngine;

public class AWARDMONEEY : MonoBehaviour
{
    public void AwardGameMoney()
    {
        int money = 200 + GameItems.Items.Count * 50;

        MONEYMANAGER.Instance.AddMoney(money);
    }
}
