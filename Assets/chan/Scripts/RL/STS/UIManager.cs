using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 텍스트 레퍼런스")]

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI moraleText;
    public TextMeshProUGUI rerollText;
    public TextMeshProUGUI chapterText;

    private Dictionary<int, UnitUIPrefab> _unitUIs = new();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);   // 씬 전환 시에도 파괴되지 않도록
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UIUpdateAll();
    }


    public void UIUpdateAll()
    {
        UpdateGold();
        UpdateMorale();
        UpdateReroll();
    }

    public void UpdateGold()
    {
        int g = RogueLikeData.Instance.GetCurrentGold();
        goldText.text = g.ToString(); ;
    }

    public void UpdateMorale()
    {
        int m = RogueLikeData.Instance.GetMorale();
        moraleText.text = m.ToString(); ;
    }

    public void UpdateReroll()
    {
        int r = RogueLikeData.Instance.GetRerollChance();
        rerollText.text = r.ToString(); ;
    }
    public void UpdateChapter(int chapter)
    {
        chapterText.text = $"Chapter {chapter}";
    }


    //금화 증감 애니메이션
    public void AnimateGoldChange(int baseGold,int newGold)
    {
        int startValue = baseGold;
        int endValue = baseGold + newGold;

        // 기존 트윈이 있으면 중지 (중복 방지)
        DOTween.Kill(this);

        // 정수값을 부드럽게 증가시키는 DOTween 트윈
        DOVirtual.Int(startValue, endValue, 0.7f, value =>
        {
            goldText.text = value.ToString();
        }).SetEase(Ease.OutCubic).SetTarget(this);
    }
    //사기 증감 애니메이션
    public void AnimateMoraleChange(int baseMorale,int newMorale)
    {
        int startValue = baseMorale;
        int endValue = baseMorale + newMorale;

        // 기존 트윈이 있으면 중지 (중복 방지)
        DOTween.Kill(this);

        // 정수값을 부드럽게 증가시키는 DOTween 트윈
        DOVirtual.Int(startValue, endValue, 0.7f, value =>
        {
            moraleText.text = value.ToString();
        }).SetEase(Ease.OutCubic).SetTarget(this);
    }
}
