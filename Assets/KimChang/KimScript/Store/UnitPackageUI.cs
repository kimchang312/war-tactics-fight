using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitPackageUI : MonoBehaviour
{
    [SerializeField] private StoreUI storeUI;
    [SerializeField] Vector2 originPos;
    [SerializeField] private Transform unitBox;
    [SerializeField] private TextMeshProUGUI packageName;
    [SerializeField] private TextMeshProUGUI packagePrice;
 
    ItemInformation itemInfo = new();
    List<RogueUnitDataBase> units = new();
    RectTransform rect;
    private readonly Vector2 centerPos = new Vector2(0, 100);
    private const float aniTime = 0.5f; 

    private void Awake()
    {
        rect = transform as RectTransform;

        if (storeUI == null) storeUI = FindObjectOfType<StoreUI>(true);
    }

    //패키지 생성 시 데이터 세팅
    public void SetUnitPackage(List<RogueUnitDataBase> _units, StoreItemData storeItem, int price)
    {
        rect = transform as RectTransform;
        if (rect != null)
            rect.anchoredPosition = originPos;

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform childRect = transform.GetChild(i) as RectTransform;
            if (childRect != null)
                childRect.anchoredPosition = Vector2.zero;
        }

        units = _units;
        itemInfo.item = storeItem;
        itemInfo.price = price;

        // 텍스트 초기화
        packageName.text = itemInfo.item.itemName;
        packagePrice.text = itemInfo.price.ToString();
        packageName.gameObject.SetActive(true);
        packagePrice.gameObject.SetActive(true);

        int unitGroupCount = unitBox.childCount;
        int unitCount = units.Count;
        for (int i = 0; i < unitGroupCount; i++)
        {
            GameObject child = unitBox.GetChild(i).gameObject;

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

    //유닛 페키지 눌렀을때 유닛 펼처지기
    private void ClickUnitPackage(Button btn)
    {
        UnitPackageUI unitPackageUI = GetComponent<UnitPackageUI>();    
        storeUI.AnimatePackageBackTrue();

        transform.SetAsLastSibling();
        // 뒤 배경, 가격, 버튼 등 UI 활성화 필요 시 여기서 처리
        storeUI.ClickUnitPackage(unitPackageUI,units,itemInfo.price);

        btn.onClick.RemoveAllListeners();

        // 텍스트 숨김
        packageName.gameObject.SetActive(false);
        packagePrice.gameObject.SetActive(false);


        rect = transform as RectTransform;
        if (rect == null) return;

        int totalChildCount = unitBox.childCount;
        if (totalChildCount == 0) return;

        List<RectTransform> activeChildren = new List<RectTransform>();
        for (int i = 0; i < totalChildCount; i++)
        {
            Transform t = unitBox.GetChild(i);
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
        sequence.Append(rect.DOAnchorPos(centerPos, aniTime).SetEase(Ease.OutCubic));

        // Step 2: 자식 정렬
        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < activeCount; i++)
            {
                Vector2 targetPos = new Vector2(startX + offsetX * i, 0f);
                activeChildren[i].DOAnchorPos(targetPos, aniTime).SetEase(Ease.OutCubic);
                activeChildren[i].GetComponent<OneUnitUI>().SetAbleUI();
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
            Button btn = GetLastChildBtn();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ClickUnitPackage(btn));
        }
    }

    private void ResetChildBtnInteractable(bool isInteractable)
    {
        foreach (Transform child in unitBox)
        {
            child.GetComponent<Button>().interactable = isInteractable;
        }

    }
    private Button GetLastChildBtn()
    {
        for (int i = unitBox.childCount - 1; i >= 0; i--)
        {
            Transform child = unitBox.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null)
                    return btn;
            }
        }
        return null;

    }

    public void ReturnUnitPackage()
    {
        rect = transform as RectTransform;
        if (rect == null) return;

        int totalChildCount = unitBox.childCount;
        if (totalChildCount == 0) return;

        List<RectTransform> activeChildren = new List<RectTransform>();
        for (int i = 0; i < totalChildCount; i++)
        {
            Transform t = unitBox.GetChild(i);
            if (t.gameObject.activeSelf)
            {
                RectTransform rt = t as RectTransform;
                if (rt != null)
                    activeChildren.Add(rt);
                t.GetComponent<OneUnitUI>().SetDisableUI();
            }
        }

        if (activeChildren.Count == 0) return;

        Sequence sequence = DOTween.Sequence();

        // Step 1: 자식들 모두 중앙 (0,0)으로 되돌리기
        foreach (var child in activeChildren)
        {
            sequence.Join(child.DOAnchorPos(Vector2.zero, aniTime).SetEase(Ease.OutCubic));
        }

        // Step 2: 이 오브젝트를 원래 위치로 이동 + 텍스트 다시 보이기
        sequence.Append(rect.DOAnchorPos(originPos, aniTime).SetEase(Ease.OutCubic))
                .OnComplete(() => {
                    packageName.gameObject.SetActive(true);
                    packagePrice.gameObject.SetActive(true);
                });

    }



}
