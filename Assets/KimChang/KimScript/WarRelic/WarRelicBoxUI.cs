using UnityEngine;
using UnityEngine.UI;

public static class WarRelicBoxUI
{
    // 사용처: 보유 중인 전쟁 유산 목록을 기준으로 box 내부의 유산 아이콘 UI를 동기화할 때 사용
    public static void SetRelicBox(GameObject box, GameObject itemToolTip, ObjectPool objectPool)
    {
        // 보유 유산 목록 가져오기 (매번 새 리스트를 만들기 때문에 너무 자주 호출하는 건 권장하지 않음)
        var warRelics = RogueLikeData.Instance.GetAllOwnedRelics();
        if (warRelics == null)
            return;

        // 박스 내부 실제 유산이 배치될 컨테이너 (레이아웃 구조는 그대로 사용)
        Transform relicBox = box.transform.GetChild(0).GetChild(0);

        int initialChildCount = relicBox.childCount;
        int childIndex = 0;
        int relicCount = warRelics.Count;

        // 1) 보유 중이고 used == false 인 유산들의 UI를 앞에서부터 차례로 채움
        for (int i = 0; i < relicCount; i++)
        {
            var relic = warRelics[i];
            if (relic.used)
                continue;

            GameObject relicObject;

            // 이미 있는 child를 재사용
            if (childIndex < initialChildCount)
            {
                relicObject = relicBox.GetChild(childIndex).gameObject;
            }
            // 모자라면 풀에서 새로 가져옴
            else
            {
                relicObject = objectPool.GetWarRelic();
                relicObject.transform.SetParent(relicBox, false);
            }

            if (!relicObject.activeSelf)
                relicObject.SetActive(true);

            ItemInformation itemInfo = relicObject.GetComponent<ItemInformation>();
            ExplainItem explainItem = relicObject.GetComponent<ExplainItem>();
            Image relicImg = relicObject.GetComponent<Image>();

            // 스프라이트 및 데이터 세팅
            relicImg.sprite = SpriteCacheManager.GetSprite($"KIcon/WarRelic/{relic.id}");

            itemInfo.data.isItem = false;
            itemInfo.data.relicId = relic.id;

            explainItem.ItemToolTip = itemToolTip;

            childIndex++;
        }

        // 2) 남는 child는 더 이상 쓸 유산이 없으므로 비활성화
        //    (원하면 이 구간을 objectPool.ReturnXXX(child)로 교체해서 풀로 돌려도 됨)
        int totalChildCount = relicBox.childCount;
        for (int i = childIndex; i < totalChildCount; i++)
        {
            GameObject child = relicBox.GetChild(i).gameObject;
            if (child.activeSelf)
                child.SetActive(false);
        }
    }
}
