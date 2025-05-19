using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private Transform relicBox;
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private GameObject itemToolTip;
    [SerializeField] private Button xbtn;

    private List<WarRelic> cacheData;

    private void Awake()
    {
         xbtn.onClick.AddListener(CloseRelic);
    }

    private void OnEnable()
    {
        foreach (Transform relic in relicBox)
        {
            objectPool.ReturnWarRelic(relic.gameObject);
        }

        List<WarRelic> relics = RogueLikeData.Instance.GetAllOwnedRelics();
        Debug.Log(relics.Count);
        foreach (WarRelic relic in relics)
        {
            GameObject obj = objectPool.GetWarRelic();

            ItemInformation itemInfo = obj.GetComponent<ItemInformation>();
            ExplainItem explainItem = obj.GetComponent<ExplainItem>();

            Image relicImg = obj.GetComponent<Image>();
            relicImg.sprite = SpriteCacheManager.GetSprite($"KIcon/WarRelic/{relic.id}");

            itemInfo.isItem = false;
            itemInfo.relicId = relic.id;
            explainItem.ItemToolTip = itemToolTip;

            obj.transform.SetParent(relicBox, false);
        }
        Debug.Log(relicBox.transform.childCount);
    }

    private void CloseRelic()
    {
        gameObject.SetActive(false);
    }
}
