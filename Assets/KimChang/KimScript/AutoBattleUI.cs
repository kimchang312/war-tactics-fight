using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.Playables;
#endif



public class AutoBattleUI : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private GameObject canvas;

    [SerializeField] private FightResult fightResult;           //전투 종료 시 보여주는 화면
    [SerializeField] private RewardUI rewardUI;

    [SerializeField] private TextMeshProUGUI _myUnitCountUI;
    [SerializeField] private TextMeshProUGUI _enemyUnitCountUI;
    [SerializeField] private TextMeshProUGUI _myUnitHPUI;
    [SerializeField] private TextMeshProUGUI _emyUnitHPUI;
    [SerializeField] private Image myMostDamImg;               
    [SerializeField] private TextMeshProUGUI myMostDamageText;   //가장 높은 피해 내 유닛
    [SerializeField] private Image myMostTakenImg;
    [SerializeField] private TextMeshProUGUI myMostTakenText;   //가장 많은 피해 받은 내 유닛
    [SerializeField] private Image enemyMostDamImg;
    [SerializeField] private TextMeshProUGUI enemyMostDamageText;   //가장 높은 피해 적 유닛
    [SerializeField] private Image enemyMostTakenImg;  
    [SerializeField] private TextMeshProUGUI enemyMostTakenText;    //가장 많은 피해 받은 적 유닛

    [SerializeField] private TextMeshProUGUI moraleText;        //사기 수치
    [SerializeField] private ObjectPool objectPool;            //  obj풀링
    [SerializeField] private GameObject abilityPool;              //기술+특성 풀

    [SerializeField] private GameObject battleUnit;           // 전투 화면 유닛
    [SerializeField] private TextMeshProUGUI _myDodge;        // 내 회피율
    [SerializeField] private TextMeshProUGUI _enemyDodge;        // 상대 회피율
    [SerializeField] private Slider myHpBar;                    // 내 체력 바
    [SerializeField] private Slider enemyHpBar;                 // 상대 체력 바
    [SerializeField] private GameObject myRangeCount;                    //내 원거리 유닛 수
    [SerializeField] private GameObject enemyRangeCount;                 //상대 원거리 유닛 수
    [SerializeField] private GameObject staticsWindow;
    [SerializeField] private Button staticsToggleBtn;
    [SerializeField] private Button GoTestBtn;
    [SerializeField] private Button rewardArrowBtn;

    [SerializeField] private GameObject loadingWindow;        //로딩창

    [SerializeField] private GameObject endWindow;

    [SerializeField] private GameObject rewardWindow;          //보상 창

    [SerializeField] private GameObject relicBox;           

    [SerializeField] private ExplainAbility explainAbility;     //능력 설명 툴팁
    [SerializeField] private ExplainRelic explainRelic;         //유산 툴팁

    [SerializeField] private TMP_InputField relicInput;             //유물 id 받을 창

    private Vector3 myTeam = new(270, 280, 0);                 // 아군 데미지 뜨는 위치
    private Vector3 enemyTeam = new(-270, 280, 0);           // 상대 데미지지 뜨는 위치

    private float waittingTime = 500f;        //애니메이션 대기 시간

    private Dictionary<int, GameObject> myUnitCache = new();
    private Dictionary<int, GameObject> enemyUnitCache = new();

    private void Start()
    {
        ResetUIActive();

        myHpBar.interactable = false;
        enemyHpBar.interactable = false;
        staticsToggleBtn.onClick.AddListener(ToggleStaticsWindow);
        GoTestBtn.onClick.AddListener(ClickGoTestBtn);
        rewardArrowBtn.onClick.AddListener(OpenRewardWindow);
        // 입력 완료 시 처리하는 이벤트 등록
        relicInput.onEndEdit.AddListener(OnEndEdit);

        UpdateMorale();
    }
    //유닛 갯수 
    public void UpdateUnitCountUI(int myUnitCount, int enemyUnitCount)
    {
        _myUnitCountUI.text = $"{myUnitCount}";
        _enemyUnitCountUI.text = $"{enemyUnitCount}";

    }
    //유닛 체력
    public void UpateUnitHPUI(float myUnitHP, float enemyUnitHP, float myMaxHp, float enemyMaxHp)
    {
        if (_myUnitHPUI != null && _emyUnitHPUI)
        {
            _myUnitHPUI.text = $"{myUnitHP}/{myMaxHp}";
            _emyUnitHPUI.text = $"{enemyUnitHP}/{enemyMaxHp}";
        }

        //체력바 업데이트
        myHpBar.maxValue = myMaxHp;
        myHpBar.value = myUnitHP;

        enemyHpBar.maxValue = enemyMaxHp;
        enemyHpBar.value = enemyUnitHP;
    }

    // 데미지 표시
    public void ShowDamage(float _damage, string text, bool team, bool isAttack, int unitIndex)
    {
        float offsetX = 50f;
        float damage = -_damage;

        // 전투 애니메이션 실행
        BattleAnimation(damage, text, team, isAttack);

        // 0.25초 뒤에 데미지 텍스트를 표시하도록 코루틴 실행
        StartCoroutine(DelayedDamageDisplay(damage, text, team, unitIndex, offsetX));
    }
    private IEnumerator DelayedDamageDisplay(float damage, string text, bool team, int unitIndex, float offsetX)
    {
        yield return new WaitForSeconds(waittingTime*0.0005f);  // 0.25초 대기

        GameObject damageObj = objectPool.GetDamageText();  // 오브젝트 풀에서 가져오기
        damageObj.SetActive(true);  // 이제 오브젝트를 활성화

        TextMeshProUGUI damagetext = damageObj.GetComponent<TextMeshProUGUI>();
        damagetext.color = damage >= 0 ? Color.green : Color.red;
        damagetext.text = damage == 0 ? $"{text}" : $"{damage} {text}";

        RectTransform rectTransform = damageObj.GetComponent<RectTransform>();

        // team = true(내 머리위), false(상대 머리위)
        if (team)
        {
            if (unitIndex == 0)
            {
                rectTransform.anchoredPosition = myTeam;
            }
            else
            {
                GameObject unit = FindUnit(unitIndex, !team);
                RectTransform unitRect = unit.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = unitRect.anchoredPosition + new Vector2(offsetX, 0);
            }
        }
        else
        {
            if (unitIndex == 0)
            {
                rectTransform.anchoredPosition = enemyTeam;
            }
            else
            {
                GameObject unit = FindUnit(unitIndex, !team);
                RectTransform unitRect = unit.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = unitRect.anchoredPosition + new Vector2(offsetX, 0);
            }
        }

        // 능력 효과 표시
        CreateAbility(text, team);

        // 데미지 제거
        StartCoroutine(HideAfterDelay(damageObj));
    }

    private void BattleAnimation(float damage, string text, bool team,bool isAttack)
    {
        // team = true -> 상대 공격, false -> 나의 공격
        if (!isAttack) return;

        GameObject unit = FindUnit(0, team);
        RectTransform rectTransform = unit.GetComponent<RectTransform>();

        Vector2 originPos = rectTransform.anchoredPosition; // 원래 UI 위치
        float direction = team ? 1f : -1f; // team이 true면 반대 방향

        Vector2 moveBackPos = originPos + new Vector2(direction * -10f, 0f); // 뒤로 이동할 위치
        Vector2 moveForwardPos = originPos + new Vector2(direction * 25f, 0f); // 앞으로 이동할 위치

        // DOTween 애니메이션 시퀀스
        DG.Tweening.Sequence attackSequence = DOTween.Sequence();
        attackSequence.Append(rectTransform.DOAnchorPos(moveBackPos, 0.05f)) // 뒤로 0.05초 이동
                      .AppendInterval(0.2f) // 0.2초 대기
                      .Append(rectTransform.DOAnchorPos(moveForwardPos, 0.2f)) // 앞으로 0.2초 이동
                      .Append(rectTransform.DOAnchorPos(originPos, 0.05f)); // 원래 위치로 0.05초 이동

        rectTransform.DOKill();
    }
    private IEnumerator HideAfterDelay(GameObject damageObj)
    {
        yield return new WaitForSeconds(waittingTime/1000f);
        objectPool.ReturnDamageText(damageObj);
    }

    //유닛 갯수 만큼 유닛 이미지 생성
    public void CreateUnitBox(List<RogueUnitDataBase> myUnits, List<RogueUnitDataBase> enemyUnits, float myDodge, float enemyDodge, List<RogueUnitDataBase> myRangeUnits, List<RogueUnitDataBase> enemyRangeUnits)
    {
        Vector3[] myPositions = { new Vector3(-180, 94, 0), new Vector3(-131, -385, 0), new Vector3(-397, -385, 0) };
        Vector3[] enemyPositions = { new Vector3(200, 94, 0), new Vector3(152, -385, 0), new Vector3(430, -385, 0) };
        Vector3 myRangeUnitPos = new Vector3(-833, -143, 0);
        Vector3 enemyRangeUnitPos = new Vector3(837, -143, 0);

        float firstSize = 250;
        float secondSize = 200;
        float unitInterval = 210;

        //유닛 초기화
        ClearExistingUnitImages();

        //능력 아이콘 초기화
        ClearExistingAbilityIcons();

        // 내 능력 아이콘 생성
        CreateAbilityIcons(myUnits[0], true);

        // 상대 능력 아이콘 생성
        CreateAbilityIcons(enemyUnits[0], false);

        // 나의 유닛 생성
        CreateUnitImages(myUnits, myPositions, firstSize, secondSize, unitInterval, true, myDodge);

        //나의 원거리 유닛 생성
        CreateRangeUnit(myRangeUnits.Count, myRangeUnitPos, myRangeCount, true);

        // 적의 유닛 생성
        CreateUnitImages(enemyUnits, enemyPositions, firstSize, secondSize, unitInterval, false, enemyDodge);

        //상대 원거리 유닛 생성
        CreateRangeUnit(enemyRangeUnits.Count, enemyRangeUnitPos, enemyRangeCount, false);

    }

    // 기존 유닛 이미지 반환 및 비활성화
    private void ClearExistingUnitImages()
    {
        foreach (var unit in objectPool.GetActiveBattleUnits())
        {
            objectPool.ReturnBattleUnit(unit); // 유닛 비활성화 및 반환
        }
    }

    //원거리 유닛 생성
    private void CreateRangeUnit(int rangeUnitCount, Vector3 position, GameObject number, bool isMyTeam)
    {
        float size = 200;
        string myTeam = isMyTeam ? "My" : "Enemy";
        Image numberImg = number.GetComponent<Image>();

        if (rangeUnitCount == 0)
        {
            numberImg.color = new Color(255, 255, 255, 0);
            return;
        }
        numberImg.color = new Color(255, 255, 255, 255);

        GameObject unit = objectPool.GetBattleUnit();
        RectTransform rectTransform = unit.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(size - 10, size - 10);
        rectTransform.anchoredPosition = position;

        // 이미지 설정 & 투명도 정상화
        Sprite sprite = Resources.Load<Sprite>($"KIcon/AbilityIcon/rangedAttack");
        Image img = unit.GetComponent<Image>();
        Color originalColor = img.color;
        img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        img.sprite = sprite;

        //자식 관리
        Transform childUnit = unit.transform.GetChild(0);
        RectTransform childReckTransform = childUnit.GetComponent<RectTransform>();
        Image childImg = childUnit.GetComponent<Image>();

        childReckTransform.sizeDelta = new Vector2(size, size);
        Sprite childSprite = Resources.Load<Sprite>($"KIcon/UI_{myTeam}SecondUnit");
        childImg.sprite = childSprite;

        //숫자 관리
        Sprite numberSprite = Resources.Load<Sprite>($"KIcon/UI_{myTeam}X{rangeUnitCount}");
        numberImg.sprite = numberSprite;

        //숫자 이미지 뒤로
        int siblingCount = number.transform.parent.childCount;
        number.transform.SetSiblingIndex(siblingCount - 1);

        //유닛 이름 변경
        unit.name = $"{myTeam}RangeUnit";
    }

    //유닛 이미지 생성
    private void CreateUnitImages(List<RogueUnitDataBase> units, Vector3[] positions, float firstSize, float secondSize, float unitInterval, bool isMyUnit, float dodge)
    {
        for (int i = 0; i < units.Count ; i++)
        {
            string unitTeam = isMyUnit ? "My" : "Enemy";
            if (units[i].health > 0)
            {
                GameObject unitImage = objectPool.GetBattleUnit();
                Transform childUnit = unitImage.transform.GetChild(0);
                RectTransform rectTransform = unitImage.GetComponent<RectTransform>();
                RectTransform childRectTrasform = childUnit.GetComponent<RectTransform>();
                Image unitFrame = childUnit.GetComponent<Image>();

                // 위치와 크기 설정
                if (i < positions.Length)
                {
                    rectTransform.anchoredPosition = positions[i];
                    switch (i)
                    {
                        case 0:
                            unitTeam += "FirstUnit";
                            break;
                        case 1:
                            unitTeam += "SecondUnit";
                            break;
                        case 2:
                            unitTeam += "BackUnit";
                            break;
                    }

                }
                else
                {
                    float offsetX = isMyUnit ? -unitInterval : unitInterval;
                    rectTransform.anchoredPosition = new Vector3(positions[2].x + offsetX * (i - positions.Length + 1), positions[2].y);

                    unitTeam += "BackUnit";
                }
                //크기 설정
                rectTransform.sizeDelta = i == 0 ? new Vector2(firstSize - 10, firstSize - 10) : new Vector2(secondSize - 10, secondSize - 10);
                childRectTrasform.sizeDelta = i == 0 ? new Vector2(firstSize, firstSize) : new Vector2(secondSize, secondSize);

                // 이미지 설정 & 투명도 정상화
                Sprite sprite = Resources.Load<Sprite>($"UnitImages/{units[i].unitImg}");
                Image img = unitImage.GetComponent<Image>();
                Color originalColor = img.color;
                img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); 
                img.sprite = sprite;

                //유닛 테두리 설정
                Sprite frameSprite = Resources.Load<Sprite>($"KIcon/UI_{unitTeam}");
                //활성화
                unitFrame.sprite = frameSprite;

                //유닛 이름 설정
                unitImage.name = $"{(isMyUnit ? "My" : "Enemy")}Unit{i}";
            }
            

        }

        // 회피율 UI 업데이트
        if (isMyUnit)
        {
            _myDodge.text = $"회피율: {dodge}%";
            //myAbility.text = ability;
        }
        else
        {
            _enemyDodge.text = $"회피율: {dodge}%";
            //enemyAbility.text = ability;
        }
    }

    // 능력치 아이콘 생성
    private void CreateAbilityIcons(RogueUnitDataBase unit, bool isTeam)
    {
        float posY = -103;
        float myTeamPosX = -280;
        float enemyTeamPosX = 88;
        float interval = 62;

        // 팀에 따라 포지션 설정
        float teamPosX = isTeam ? myTeamPosX : enemyTeamPosX;

        // 유닛의 불리언 속성 가져오기
        var boolAttributes = unit.GetType().GetFields()
            .Where(f => f.FieldType == typeof(bool))
            .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(unit) });

        int i = 0;

        foreach (var attr in boolAttributes)
        {
            if (attr.Value)
            {
                // 원거리 특성 제외
                if (attr.Name != "rangedAttack" && attr.Name !="alive")
                {
                    GameObject iconImage = objectPool.GetAbility();
                    
                    RectTransform rectTransform = iconImage.GetComponent<RectTransform>();

                    // 팀에 따라 위치 설정
                    rectTransform.anchoredPosition = new Vector2(teamPosX + (i * interval), posY);

                    // 크기 초기화
                    rectTransform.localScale = Vector3.one;

                    // 이미지 설정
                    Sprite sprite = Resources.Load<Sprite>($"KIcon/AbilityIcon/{attr.Name}");
                    Image img = iconImage.GetComponent<Image>();
                    img.sprite = sprite;

                    // 이름 설정
                    if (attr.Name== "defense")
                    {
                        iconImage.name = "126";
                    }
                    else
                    {
                        int? idx = GameTextData.GetIdxFromString(attr.Name);
                        iconImage.name = idx.HasValue ? $"{idx}" : attr.Name;
                    }
                    
                    //코드 추가
                    explainAbility = iconImage.AddComponent<ExplainAbility>();

                    i++;
                }
            }
        }
    }

    //능력치 아이콘 제거
    private void ClearExistingAbilityIcons()
    {
        foreach (var unit in objectPool.GetActiveAbilitys())
        {
            objectPool.ReturnAbility(unit); // 능력 아이콘 비활성화 및 반환
        }
    }

    //전투 종료
    public void FightEnd(int result)
    {
        fightResult.EndGame(result);

        rewardUI.CreateRelics();
    }

    //점수 보여주기
    public void UpdateScore(int result)
    {
        fightResult.ViewScore(result);
    }

    //능력 창 띄위기
    public void CreateAbility(string ability, bool myTeam)
    {
        List<string> abilityList = new List<string>(ability.Split(' '));

        int team = myTeam ? 0 : 5;

        if (abilityList.Count > 0)
        {
            //유격 착취 제외하고
            for (int i = 0; i < abilityList.Count - 1; i++)
            {
                // 첫 번째 능력이 "유격" 또는 "착취"인 경우 team 값을 반전
                if (abilityList[0] == "유격" || abilityList[0] == "착취")
                {
                    team = (team == 0) ? 5 : 0;
                }
                Transform child = abilityPool.transform.GetChild(i + team);
                child.gameObject.SetActive(true);
                child.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = abilityList[i];
            }

        }
    }

    //유닛 사망 시 투명하게
    public void ChangeInvisibleUnit(int unitIndex, bool isMyUnit)
    {
        GameObject unit= FindUnit(unitIndex, isMyUnit);
        if (unit==null)
        {
            Debug.Log($"{unitIndex}{isMyUnit}");
            Debug.Log("유닛 비어있음 오류임");
            
        }
        FadeOutUnit(unit);
    }
    // 생성된 유닛 검색 (캐싱 없이 항상 새로 검색)
    private GameObject FindUnit(int unitIndex, bool isMyUnit)
    {
        string unitName = $"{(isMyUnit ? "MyUnit" : "EnemyUnit")}{unitIndex}";

        foreach (Transform child in canvasTransform)
        {
            if (child.name == unitName && child.gameObject.activeSelf)
            {
                return child.gameObject; // 정확한 유닛을 찾아 반환
            }
        }

        return null; // 유닛을 찾지 못하면 null 반환
    }

    //유닛 투명
    private void FadeOutUnit(GameObject unit)
    {
        Image unitImage = unit.GetComponent<Image>();
        unitImage.DOFade(0f, waittingTime / 1000f).OnComplete(() =>
        {
            unit.SetActive(false); // 투명화 후 유닛을 비활성화
        });
    }

    //대기 시간 변경
    public void ChangeWaittingTime(float multiple)
    {
        waittingTime *= multiple;

    }

    //종료 시 통계 보여주기
    public void ViewStatics(string max, string text, int num,int unitIdx)
    {
        switch (num)
        {
            //내 최뎀유닛
            case 0:
                ChangeStaticImage(myMostDamImg, myMostDamageText,max, text, unitIdx);
                break;
            // 내 최받유닛
            case 1:
                ChangeStaticImage(myMostTakenImg, myMostTakenText, max, text, unitIdx);
                break;
            //상대 최뎀유닛
            case 2:
                ChangeStaticImage(enemyMostDamImg, enemyMostDamageText, max, text, unitIdx);
                break;
            //상대 최받유닛
            case 3:
                ChangeStaticImage(enemyMostTakenImg, enemyMostTakenText, max, text, unitIdx);
                break;
            default:
                Debug.Log("숫자가 0~3이 아님");
                break;
        }

    }

    //이미지 변경
    private void ChangeStaticImage(Image img,TextMeshProUGUI textUI,string max,string text,int idx)
    {
        Sprite sprite = Resources.Load<Sprite>($"UnitImages/Unit_Img_{idx}");
        img.sprite = sprite;
        textUI.text = $"{max}\n{text}";
    }

    //통계창 온오프
    private void ToggleStaticsWindow()
    {
        // 활성화 상태를 반대로 설정
        if (staticsWindow != null)
        {
            staticsWindow.SetActive(!staticsWindow.activeSelf);
        }
    }

    //유산 생성
    public void CreateWarRelic()
    {
        var warRelics = RogueLikeData.Instance.GetAllOwnedRelics();

        if (warRelics == null || warRelics.Count == 0)
            return;
        float startX = -900f;
        float startY = 350f;
        float spacingX = 100f;

        for (int i = 0; i < warRelics.Count; i++)
        {
            GameObject relicObject = objectPool.GetWarRelic(); // ObjectPool에서 유물 오브젝트 가져오기

            Sprite sprite = Resources.Load<Sprite>($"KIcon/WarRelic/{warRelics[i].id}");
            Image relicImg= relicObject.GetComponent<Image>();
            relicImg.sprite = sprite;

            RectTransform relicTransform = relicObject.GetComponent<RectTransform>();
            
            relicTransform.anchoredPosition = new Vector2(startX+(spacingX*i), startY);

            relicObject.name = $"{warRelics[i].id}";

            // 부모 설정
            relicObject.transform.SetParent(relicBox.transform, false);

            explainRelic = relicObject.AddComponent<ExplainRelic>();
        }
    }

    // 입력 완료 시 처리
    private void OnEndEdit(string input)
    {
        // 숫자만 필터링
        string filteredInput = Regex.Replace(input, "[^0-9]", "");

        // 정수로 변환
        if (int.TryParse(filteredInput, out int relicId))
        {
            // 1~61 범위 내에 있을 경우에만 처리
            if (relicId >= 1 && relicId <= 61)
            {
                RogueLikeData.Instance.AcquireRelic(relicId);
                Debug.Log($"유산 획득: ID {relicId}");
            }
            else
            {
                Debug.Log("입력값이 1~61 범위를 벗어남");
            }
        }
        else
        {
            Debug.Log("숫자 변환 실패");
        }

        // 입력 필드 초기화 및 다시 입력 가능하도록 포커스 유지
        relicInput.text = "";
        relicInput.ActivateInputField();
    }

    //로딩창 간단하게 구현
    public void ToggleLoadingWindow()
    {
        loadingWindow.SetActive(!loadingWindow.activeSelf);
    }

    //테스트 화면으로
    private void ClickGoTestBtn()
    {
        SceneManager.LoadScene("Upgrade");
    }

    //보상 창 열기+ 통계,점수,승패 창 닫기 
    private void OpenRewardWindow()
    {
        SceneManager.LoadScene("Event");
        /*
        staticsWindow.SetActive(false);
        endWindow.SetActive(false);
        staticsToggleBtn.gameObject.SetActive(false);

        rewardWindow.SetActive(true);*/
    }
    //보상 창 닫기
    private void CloseRewardWindow()
    {
        rewardWindow.SetActive(false);
    }

    //ui 활성화 비활성화 초기화
    private void ResetUIActive()
    {
        fightResult.CloseThis();
        endWindow.SetActive(true);
        GoTestBtn.gameObject.SetActive(true);
        rewardArrowBtn.gameObject.SetActive(true);
        rewardWindow.SetActive(false);
        //staticsToggleBtn.gameObject.SetActive(true);
    }

    //사기 값 수정
    public void UpdateMorale()
    {
        int morale = RogueLikeData.Instance.GetMorale();
        moraleText.text = $"{morale}";
    }

   
}

