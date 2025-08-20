
using UnityEngine;
using UnityEngine.UI;

public static class WarRelicBoxUI
{ 

    public static void SetRelicBox(GameObject box, GameObject itemToolTip, ObjectPool objectPool)
    {
        var warRelics = RogueLikeData.Instance.GetAllOwnedRelics();

        if (warRelics == null || warRelics.Count == 0)
            return;

        for (int i = 0; i < warRelics.Count; i++)
        {
            if (warRelics[i].used) return;
            GameObject relicObject = objectPool.GetWarRelic();
            ItemInformation itemInfo = relicObject.GetComponent<ItemInformation>();
            ExplainItem explainItem = relicObject.GetComponent<ExplainItem>();

            Image relicImg = relicObject.GetComponent<Image>();
            relicImg.sprite = SpriteCacheManager.GetSprite($"KIcon/WarRelic/{warRelics[i].id}");

            itemInfo.data.isItem = false;
            itemInfo.data.relicId = warRelics[i].id;

            explainItem.ItemToolTip = itemToolTip;

            relicObject.transform.SetParent(box.transform, false);

        }
    }



}
