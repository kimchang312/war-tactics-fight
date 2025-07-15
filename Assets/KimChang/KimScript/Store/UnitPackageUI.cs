using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitPackageUI : MonoBehaviour
{
    [SerializeField] private StoreUI storeUI;
    ItemInformation itemInfo=new();
    List<RogueUnitDataBase> units=new();
    Vector2 originPos;

    private void Awake()
    {
        if (storeUI == null) storeUI = FindObjectOfType<StoreUI>(true);
        originPos = transform.position;
    }

    //패키지 생성 시 데이터 세팅
    public void SetUnitPackage(List<RogueUnitDataBase> _units,StoreItemData storeItem,int price)
    {
        units = _units;
        itemInfo.item = storeItem;
        itemInfo.price = price;
        int unitGroupCount = transform.childCount;
        int unitCount = units.Count;
        for (int i = 0; i < unitGroupCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (i < unitCount)
            {
                RogueUnitDataBase unit = units[i];
                child.SetActive(true);
                OneUnitUI oneUnitUI = child.GetComponent<OneUnitUI>();
                oneUnitUI.SetOneUnit(unit);
            }
            else
            {
                child.SetActive(false);
            }
        }

        UpdateUnitPackage();
    }

    private void ClickUnitPackage(Button btn)
    {
        transform.SetAsLastSibling();
        // 뒤 배경, 가격, 버튼 등 UI 활성화 필요 시 여기서 처리
        storeUI.OpenPackageBack();

        btn.onClick.RemoveAllListeners();

        RectTransform selfRect = transform as RectTransform;
        if (selfRect == null) return;

        int totalChildCount = transform.childCount;
        if (totalChildCount == 0) return;

        // 활성화된 자식만 추출
        List<RectTransform> activeChildren = new List<RectTransform>();
        for (int i = 0; i < totalChildCount; i++)
        {
            Transform t = transform.GetChild(i);
            if (t.gameObject.activeSelf)
            {
                RectTransform rt = t as RectTransform;
                if (rt != null)
                    activeChildren.Add(rt);
            }
        }

        int activeCount = activeChildren.Count;
        if (activeCount == 0) return;

        float offsetX = 320f;
        float startX = -offsetX * (activeCount - 1) / 2f;

        Sequence sequence = DOTween.Sequence();

        // Step 1: 본인(컨테이너) 0,0으로 이동
        sequence.Append(selfRect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic));

        // Step 2: 자식 정렬
        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < activeCount; i++)
            {
                Vector2 targetPos = new Vector2(startX + offsetX * i, 0f);
                activeChildren[i].DOAnchorPos(targetPos, 0.5f).SetEase(Ease.OutCubic);
            }
        });
    }

    public void UpdateUnitPackage()
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        if (gold < itemInfo.price)
        {
            ResetChildBtnInteractable(false);
        }
        else
        {
            ResetChildBtnInteractable(true);
            //필요는 없을거 같지만 만약 상점에 들어온 후 금화가 증가하는 상황이 발생한다면 버튼에 이벤트 재 추가
            Button btn = GetLastChildBtn();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ClickUnitPackage(btn));
        }
    }

    private void ResetChildBtnInteractable(bool isInteractable)
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<Button>().interactable = isInteractable;
        }
    }
    private Button GetLastChildBtn()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null)
                    return btn;
            }
        }
        return null;
    }

    public void ResetUnitPackage()
    {
        RectTransform selfRect = transform as RectTransform;
        if (selfRect == null) return;

        int totalChildCount = transform.childCount;
        if (totalChildCount == 0) return;

        List<RectTransform> activeChildren = new List<RectTransform>();
        for (int i = 0; i < totalChildCount; i++)
        {
            Transform t = transform.GetChild(i);
            if (t.gameObject.activeSelf)
            {
                RectTransform rt = t as RectTransform;
                if (rt != null)
                    activeChildren.Add(rt);
            }
        }

        if (activeChildren.Count == 0) return;

        Sequence sequence = DOTween.Sequence();

        // Step 1: 자식들 모두 중앙 (0,0)으로 되돌리기
        foreach (var child in activeChildren)
        {
            sequence.Join(child.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic));
        }

        // Step 2: 이 오브젝트를 원래 위치로 이동
        sequence.Append(selfRect.DOAnchorPos(originPos, 0.5f).SetEase(Ease.OutCubic));
    }

}
