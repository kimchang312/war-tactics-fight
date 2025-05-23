using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeStateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI spearAttackText;
    [SerializeField] private TextMeshProUGUI spearDeffenseText;
    [SerializeField] private TextMeshProUGUI swordAttackText;
    [SerializeField] private TextMeshProUGUI swaorDeffenseText;
    [SerializeField] private TextMeshProUGUI bowAttackText;
    [SerializeField] private TextMeshProUGUI bowDeffenseText;
    [SerializeField] private TextMeshProUGUI heavyAttackText;
    [SerializeField] private TextMeshProUGUI heavyDeffenseText;
    [SerializeField] private TextMeshProUGUI assasinAttackText;
    [SerializeField] private TextMeshProUGUI assasinDeffenseText;
    [SerializeField] private TextMeshProUGUI lightAttackText;
    [SerializeField] private TextMeshProUGUI lightDeffenseText;
    [SerializeField] private TextMeshProUGUI heavyCAttackText;
    [SerializeField] private TextMeshProUGUI heavyCDeffenseText;
    [SerializeField] private TextMeshProUGUI supAttackText;
    [SerializeField] private TextMeshProUGUI supDeffenseText;

    [SerializeField] private Button xBtn;

    private UnitUpgrade[] cacheData;

    private void Awake()
    {
        xBtn.onClick.AddListener(()=>gameObject.SetActive(false));  
    }

    private void OnEnable()
    {
        UnitUpgrade[] upgrades = RogueLikeData.Instance.GetUpgradeValue();
        /* 전술개량 업그레이드 후 바로 ui 반영하기 위해서 주석처리.
         * 캐시데이터는 받아놓고 실시간 업데이트가 안됌.
        if (cacheData == upgrades) return;

        cacheData = upgrades;*/

        spearAttackText.text = upgrades[0].attackLevel.ToString();
        spearDeffenseText.text = upgrades[0].defenseLevel.ToString();

        swordAttackText.text = upgrades[1].attackLevel.ToString();
        swaorDeffenseText.text = upgrades[1].defenseLevel.ToString();

        bowAttackText.text = upgrades[2].attackLevel.ToString();
        bowDeffenseText.text = upgrades[2].defenseLevel.ToString();

        heavyAttackText.text = upgrades[3].attackLevel.ToString();
        heavyDeffenseText.text = upgrades[3].defenseLevel.ToString();

        assasinAttackText.text = upgrades[4].attackLevel.ToString();
        assasinDeffenseText.text = upgrades[4].defenseLevel.ToString();

        lightAttackText.text = upgrades[5].attackLevel.ToString();
        lightDeffenseText.text = upgrades[5].defenseLevel.ToString();

        heavyCAttackText.text = upgrades[6].attackLevel.ToString();
        heavyCDeffenseText.text = upgrades[6].defenseLevel.ToString();

        supAttackText.text = upgrades[7].attackLevel.ToString();
        supDeffenseText.text = upgrades[7].defenseLevel.ToString();
    }


}
