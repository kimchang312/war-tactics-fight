using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopupUI : MonoBehaviour
{
    private const string SkipAllText = "모든 가이드 자동 표시 끄기";
    private const string SkipHelperText = "설정에서 다시 켜거나 가이드를 다시 볼 수 있습니다.";

    [SerializeField] private GameObject popupRoot;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text pageCountText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text confirmButtonText;
    [SerializeField] private Toggle skipAllToggle;
    [SerializeField] private TMP_Text skipAllLabelText;
    [SerializeField] private TMP_Text skipAllHelperText;

    private readonly List<TutorialManualPageEntry> manualPages = new List<TutorialManualPageEntry>();
    private TutorialGuideData activeGuide;
    private TutorialMode mode;
    private int pageIndex;
    private bool isOpen;
    private Action<TutorialPopupResult> onClosed;
    private CanvasGroup fallbackCanvasGroup;
    private bool initialized;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            CloseFromDismiss();
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            ShowNextOrComplete();
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            ShowPreviousPage();
    }

    private void LateUpdate()
    {
        if (isOpen)
            BringToFront();
    }

    public void OpenAuto(TutorialGuideData guide, Action<TutorialPopupResult> closed)
    {
        InitializeIfNeeded();

        if (guide == null || guide.Pages == null || guide.Pages.Count == 0)
            return;

        BindButtons();

        mode = TutorialMode.Auto;
        activeGuide = guide;
        manualPages.Clear();
        pageIndex = 0;
        onClosed = closed;

        if (skipAllToggle != null)
            skipAllToggle.SetIsOnWithoutNotify(false);

        SetVisible(true);
        ShowCurrentPage();
    }

    public void OpenManual(List<TutorialManualPageEntry> pages, Action<TutorialPopupResult> closed)
    {
        InitializeIfNeeded();

        if (pages == null || pages.Count == 0)
            return;

        BindButtons();

        mode = TutorialMode.Manual;
        activeGuide = null;
        manualPages.Clear();
        manualPages.AddRange(pages);
        pageIndex = 0;
        onClosed = closed;

        if (skipAllToggle != null)
            skipAllToggle.SetIsOnWithoutNotify(false);

        SetVisible(true);
        ShowCurrentPage();
    }

    public void InitializeIfNeeded()
    {
        if (initialized)
            return;

        AutoBindReferences();
        BindButtons();
        SetVisible(false);
        initialized = true;
    }

    private void AutoBindReferences()
    {
        if (popupRoot == null)
            popupRoot = FindDeepChild(transform, "TutorialPanel")?.gameObject ?? gameObject;

        Transform searchRoot = popupRoot != null ? popupRoot.transform : transform;

        if (tutorialImage == null)
            tutorialImage = FindComponentInChildrenByName<Image>(searchRoot, "TutorialImg");
        if (titleText == null)
            titleText = FindComponentInChildrenByName<TMP_Text>(searchRoot, "TutorialText");
        if (descriptionText == null)
            descriptionText = FindComponentInChildrenByName<TMP_Text>(searchRoot, "TutorialDescriptionText");
        if (pageCountText == null)
            pageCountText = FindComponentInChildrenByName<TMP_Text>(searchRoot, "TutorialPageCounText")
                ?? FindComponentInChildrenByName<TMP_Text>(searchRoot, "TutorialPageCountText");
        if (confirmButton == null)
            confirmButton = FindComponentInChildrenByName<Button>(searchRoot, "TutorialOkBtn");
        if (closeButton == null)
            closeButton = FindComponentInChildrenByName<Button>(searchRoot, "CloseBtn");
        if (previousButton == null)
            previousButton = FindComponentInChildrenByName<Button>(searchRoot, "PreviousBtn")
                ?? FindComponentInChildrenByName<Button>(searchRoot, "PrevBtn")
                ?? FindComponentInChildrenByName<Button>(searchRoot, "LeftBtn");
        if (nextButton == null)
            nextButton = FindComponentInChildrenByName<Button>(searchRoot, "NextBtn")
                ?? FindComponentInChildrenByName<Button>(searchRoot, "RightBtn");
        if (skipAllToggle == null)
            skipAllToggle = FindComponentInChildrenByName<Toggle>(searchRoot, "Toggle");
        if (confirmButtonText == null && confirmButton != null)
            confirmButtonText = confirmButton.GetComponentInChildren<TMP_Text>(true);
        if (skipAllLabelText == null)
            skipAllLabelText = FindComponentInChildrenByName<TMP_Text>(searchRoot, "SkipAllText");
        if (skipAllHelperText == null)
            skipAllHelperText = FindComponentInChildrenByName<TMP_Text>(searchRoot, "SkipHelperText")
                ?? FindComponentInChildrenByName<TMP_Text>(searchRoot, "AgainMethodText");
        if (skipAllLabelText == null && skipAllToggle != null)
            skipAllLabelText = GetFirstText(skipAllToggle.transform);

        if (tutorialImage != null)
            tutorialImage.preserveAspect = true;

        if (skipAllLabelText != null)
            skipAllLabelText.text = SkipAllText;
        if (skipAllHelperText != null)
            skipAllHelperText.text = SkipHelperText;
    }

    private void BindButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseFromDismiss);
            closeButton.onClick.AddListener(CloseFromDismiss);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ShowNextOrComplete);
            confirmButton.onClick.AddListener(ShowNextOrComplete);
        }

        if (previousButton != null)
        {
            previousButton.onClick.RemoveListener(ShowPreviousPage);
            previousButton.onClick.AddListener(ShowPreviousPage);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(ShowNextPage);
            nextButton.onClick.AddListener(ShowNextPage);
        }
    }

    private void ShowCurrentPage()
    {
        TutorialGuideData guide = GetCurrentGuide();
        TutorialPageData page = GetCurrentPage();
        if (guide == null || page == null)
            return;

        if (titleText != null)
            titleText.text = guide.Title;

        if (descriptionText != null)
            descriptionText.text = page.Description;

        page.ResolveImage();
        if (tutorialImage != null)
        {
            tutorialImage.sprite = page.Image;
            tutorialImage.enabled = page.Image != null;
        }

        int totalPages = GetTotalPageCount();
        if (pageCountText != null)
            pageCountText.text = $"{pageIndex + 1} / {totalPages}";

        bool isLastPage = pageIndex >= totalPages - 1;
        if (confirmButtonText != null)
            confirmButtonText.text = isLastPage
                ? (mode == TutorialMode.Auto ? "확인" : "닫기")
                : "다음";

        EnsureButtonReceivesInput(confirmButton);
        EnsureButtonReceivesInput(nextButton);

        if (previousButton != null)
            previousButton.interactable = pageIndex > 0;
        if (nextButton != null)
            nextButton.interactable = !isLastPage;

        bool showSkipAll = mode == TutorialMode.Auto;
        if (skipAllToggle != null)
            skipAllToggle.gameObject.SetActive(showSkipAll);
        if (skipAllLabelText != null)
            skipAllLabelText.gameObject.SetActive(showSkipAll);
        if (skipAllHelperText != null)
            skipAllHelperText.gameObject.SetActive(showSkipAll);
    }

    private void ShowNextOrComplete()
    {
        if (!isOpen)
            return;

        int totalPages = GetTotalPageCount();
        if (pageIndex < totalPages - 1)
        {
            pageIndex++;
            ShowCurrentPage();
            return;
        }

        TutorialPopupCloseReason reason = mode == TutorialMode.Auto
            ? TutorialPopupCloseReason.Confirmed
            : TutorialPopupCloseReason.ManualClosed;
        Close(reason);
    }

    private void ShowNextPage()
    {
        if (!isOpen)
            return;

        int totalPages = GetTotalPageCount();
        if (pageIndex >= totalPages - 1)
            return;

        pageIndex++;
        ShowCurrentPage();
    }

    private void ShowPreviousPage()
    {
        if (!isOpen || pageIndex <= 0)
            return;

        pageIndex--;
        ShowCurrentPage();
    }

    private void CloseFromDismiss()
    {
        if (!isOpen)
            return;

        TutorialPopupCloseReason reason = mode == TutorialMode.Auto
            ? TutorialPopupCloseReason.Dismissed
            : TutorialPopupCloseReason.ManualClosed;
        Close(reason);
    }

    private void Close(TutorialPopupCloseReason reason)
    {
        bool skipAll = skipAllToggle != null && skipAllToggle.isOn;
        Action<TutorialPopupResult> closed = onClosed;

        isOpen = false;
        onClosed = null;
        activeGuide = null;
        manualPages.Clear();
        SetVisible(false);

        closed?.Invoke(new TutorialPopupResult
        {
            CloseReason = reason,
            SkipAllAuto = skipAll
        });
    }

    private TutorialGuideData GetCurrentGuide()
    {
        if (mode == TutorialMode.Auto)
            return activeGuide;

        return pageIndex >= 0 && pageIndex < manualPages.Count
            ? manualPages[pageIndex].Guide
            : null;
    }

    private TutorialPageData GetCurrentPage()
    {
        if (mode == TutorialMode.Auto)
        {
            if (activeGuide == null || activeGuide.Pages == null || pageIndex < 0 || pageIndex >= activeGuide.Pages.Count)
                return null;

            return activeGuide.Pages[pageIndex];
        }

        return pageIndex >= 0 && pageIndex < manualPages.Count
            ? manualPages[pageIndex].Page
            : null;
    }

    private int GetTotalPageCount()
    {
        if (mode == TutorialMode.Auto)
            return activeGuide?.Pages?.Count ?? 0;

        return manualPages.Count;
    }

    private void SetVisible(bool visible)
    {
        if (visible && !gameObject.activeSelf)
            gameObject.SetActive(true);

        if (visible)
            BringToFront();

        isOpen = visible;

        if (popupRoot != null && popupRoot != gameObject)
        {
            popupRoot.SetActive(visible);
            return;
        }

        if (fallbackCanvasGroup == null)
            fallbackCanvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        fallbackCanvasGroup.alpha = visible ? 1f : 0f;
        fallbackCanvasGroup.interactable = visible;
        fallbackCanvasGroup.blocksRaycasts = visible;
    }

    private void BringToFront()
    {
        SetAsLastSiblingIfNeeded(transform);

        if (popupRoot != null && popupRoot != gameObject)
            SetAsLastSiblingIfNeeded(popupRoot.transform);
    }

    private static void SetAsLastSiblingIfNeeded(Transform target)
    {
        if (target == null || target.parent == null)
            return;

        int lastSiblingIndex = target.parent.childCount - 1;
        if (target.GetSiblingIndex() != lastSiblingIndex)
            target.SetAsLastSibling();
    }

    private static void EnsureButtonReceivesInput(Button button)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(true);
        button.interactable = true;

        if (button.targetGraphic != null)
            button.targetGraphic.raycastTarget = true;
    }

    private static Transform FindDeepChild(Transform root, string targetName)
    {
        if (root == null)
            return null;

        if (root.name == targetName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeepChild(root.GetChild(i), targetName);
            if (found != null)
                return found;
        }

        return null;
    }

    private static T FindComponentInChildrenByName<T>(Transform root, string targetName) where T : Component
    {
        Transform found = FindDeepChild(root, targetName);
        return found != null ? found.GetComponent<T>() : null;
    }

    private static TMP_Text GetFirstText(Transform root)
    {
        if (root == null)
            return null;

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        return texts != null && texts.Length > 0 ? texts[0] : null;
    }
}
