using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("사기 이미지UI")]
    [SerializeField] private Image moraleIconImage;
    [SerializeField] private Sprite normalMoraleSprite;
    [SerializeField] private Sprite mediumMoraleSprite;
    [SerializeField] private Sprite highMoraleSprite;

    [Header("UI 텍스트 레퍼런스")]

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI moraleText;
    public TextMeshProUGUI rerollText;
    public TextMeshProUGUI chapterText;

    private Dictionary<int, UnitUIPrefab> _unitUIs = new();

    //금화, 사기 애니메이션을 위한 변수
    private Tween goldTween;
    private Tween moraleTween;
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
        moraleText.text = m.ToString();
        UpdateMoraleIcon(m);
    }

    public void UpdateReroll()
    {
        (int, bool) r = RogueLikeData.Instance.GetRerollChance();
        rerollText.text = r.Item1.ToString();
    }
    public void UpdateChapter(int chapter)
    {
        chapterText.text = $"Chapter {chapter}";
    }


    // 사용처: 금화 증감 시 금화 UI만 독립적으로 애니메이션
    public void AnimateGoldChange(int baseGold, int deltaGold)
    {
        int startValue = baseGold;
        int endValue = baseGold + deltaGold;

        goldTween?.Kill();

        goldTween = DOVirtual.Int(startValue, endValue, 0.7f, value =>
        {
            goldText.text = value.ToString();
        })
        .SetEase(Ease.OutCubic)
        .SetTarget(goldText);
    }

    private void UpdateMoraleIcon(int moraleValue)
    {
        if (moraleIconImage == null)
            return;

        if (moraleValue <= 30)
            moraleIconImage.sprite = normalMoraleSprite;
        else if (moraleValue <= 70)
            moraleIconImage.sprite = mediumMoraleSprite;
        else
            moraleIconImage.sprite = highMoraleSprite;
    }

    // 사용처: 사기 증감 시 사기 UI만 독립적으로 애니메이션
    public void AnimateMoraleChange(int baseMorale, int deltaMorale)
    {
        int startValue = baseMorale;
        int endValue = baseMorale + deltaMorale;

        moraleTween?.Kill();

        moraleTween = DOVirtual.Int(startValue, endValue, 0.7f, value =>
        {
            moraleText.text = value.ToString();
            UpdateMoraleIcon(value);
        })
        .SetEase(Ease.OutCubic)
        .SetTarget(moraleText);
    }
}
