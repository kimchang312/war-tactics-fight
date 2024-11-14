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

    [SerializeField] private ObjectPool objectPool;         //  objǮ�� ����

    private Vector3 myTeam = new(11, -1, 0);                 // ���� �ִ� ������ ��ġ
    private Vector3 enemyTeam = new(6, -1, 0);           // ��밡 �ִ� ������ ��ġ


    //���� �� �ʱ�ȭ

    public void UpdateUnitCountUI(int myUnitCount,int enemyUnitCount)

    {
        if (_myUnitCountUI != null && _enemyUnitCountUI)
        {
            _myUnitCountUI.text = $"{myUnitCount}";
            _enemyUnitCountUI.text = $"{enemyUnitCount}";
        }
    }


    //ü�� �ʱ�ȭ

    public void UpateUnitHPUI(float myUnitHP,float enemyUnitHP)

    {
        if (_myUnitHPUI != null && _emyUnitHPUI)
        {
            _myUnitHPUI.text = $"{myUnitHP}";
            _emyUnitHPUI.text = $"{enemyUnitHP}";
        }

    }

    //�̸� �ʱ�ȭ
    public void UpdateName(string myUnitName,string enemyUnitName)
    {
        _myUnitName.text = $"{myUnitName}";
        _enemyUnitName.text=$"{enemyUnitName}";
    }

    //������ �����ִ� �Լ�
    public void ShowDamage(float damage, string text, bool team)
    {
        GameObject damageObj = objectPool.GetDamageText();
        TextMeshPro damagetext = damageObj.GetComponent<TextMeshPro>();

        damagetext.text = $"-{damage} {text}";

        //team = true== �� false == ���
        damageObj.transform.position = team ? myTeam : enemyTeam;
        // ���� �ð� �� ��Ȱ��ȭ
        StartCoroutine(HideAfterDelay(damageObj));
    }

    private IEnumerator HideAfterDelay(GameObject damageObj)
    {
        yield return new WaitForSeconds(0.5f);
        objectPool.ReturnDamageText(damageObj);
    }
}

