using UnityEngine;
using UnityEngine.UI;

public class CoolTimeFill : MonoBehaviour
{
    PlayerController player;
    [SerializeField] Image coolTimeBar;
    [SerializeField] GameObject coolTimeObj;

    public void SetPlayerCtr(PlayerController _player)
    {
        player = _player;
    }

    public void FillCoolTime(float time, float coolTime)
    {
        float coolTimeRate = time / coolTime;

        if (coolTimeRate >= 1)
        {
            coolTimeObj.SetActive(false);
        }
        else
        {
            coolTimeObj.SetActive(true);
        }

        coolTimeBar.fillAmount = coolTimeRate;
    }
}
