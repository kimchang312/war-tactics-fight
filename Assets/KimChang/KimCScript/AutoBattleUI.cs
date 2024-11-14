using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutoBattleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _myUnitCountUI;
    [SerializeField] private TextMeshProUGUI _enemyUnitCountUI;
    [SerializeField] private TextMeshProUGUI _myUnitHPUI;
    [SerializeField] private TextMeshProUGUI _emyUnitHPUI;

    [SerializeField] private TextMeshPro _myUnitName;
    [SerializeField] private TextMeshPro _enemyUnitName;

    [SerializeField] private ObjectPool objectPool;         //  obj풀링 연결

    private Vector3 myTeam = new(11, -1, 0);                 // 내가 주는 데미지 위치
    private Vector3 enemyTeam = new(6, -1, 0);           // 상대가 주는 데미지 위치


    //유닛 수 초기화
    public void UpdateUnitCountUI(int myUnitCount,int enemyUnitCount)
    {
        if (_myUnitCountUI != null && _enemyUnitCountUI)
        {
            _myUnitCountUI.text = $"{myUnitCount}";
            _enemyUnitCountUI.text = $"{enemyUnitCount}";
        }
    }

    //체력 초기화
    public void UpateUnitHPUI(float myUnitHP,float enemyUnitHP)
    {
        if (_myUnitHPUI != null && _emyUnitHPUI)
        {
            _myUnitHPUI.text = $"{myUnitHP}";
            _emyUnitHPUI.text = $"{enemyUnitHP}";
        }
    }

    //이름 초기화
    public void UpdateName(string myUnitName,string enemyUnitName)
    {
        _myUnitName.text = $"{myUnitName}";
        _enemyUnitName.text=$"{enemyUnitName}";
    }

    //데미지 보여주는 함수
    public void ShowDamage(float damage, string text, bool team)
    {
        GameObject damageObj = objectPool.GetDamageText();
        TextMeshPro damagetext = damageObj.GetComponent<TextMeshPro>();

        damagetext.text = $"-{damage} {text}";

        //team = true== 나 false == 상대
        damageObj.transform.position = team ? myTeam : enemyTeam;
        // 일정 시간 후 비활성화
        StartCoroutine(HideAfterDelay(damageObj));
    }

    private IEnumerator HideAfterDelay(GameObject damageObj)
    {
        yield return new WaitForSeconds(0.5f);
        objectPool.ReturnDamageText(damageObj);
    }
}
