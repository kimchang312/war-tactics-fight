using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using System.Linq;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;


public class AutoBattleManager : MonoBehaviour
{

    [SerializeField] private AutoBattleUI autoBattleUI;       //UI 관리 스크립트
    private AbilityManager abilityManager = new AbilityManager();

    private readonly GoogleSheetLoader sheetLoader = new();

    private readonly float heavyArmorValue=15.0f;                //중갑 피해 감소
    private readonly float bluntWeaponValue = 15.0f;             //둔기 추가 피해
    private readonly float throwSpearValue = 50.0f;              //투창 추가 피해
    private readonly float overwhelmValue = 1.0f;                //위압 기동력 고정
    private readonly float strongChargeValue = 0.5f;             //강한 돌진 값
    private readonly float defenseValue = 15.0f;                 //수비태세 값
    private readonly float slaughterValue = 10.0f;               //도살 추가 피해
    private readonly float assassinationValue = 2.0f;            //암살 배율
    private readonly float drainHealValue = 20.0f;               //착취 회복량
    private readonly float drainGainAttackValue = 10.0f;         //착취 공격력 증가량
    private float suppressionValue = 1.1f;                 //제압 데미지
    private float thornsDamageValue = 10.0f;                      //가시 데미지
    private float fireDamageValue = 10.0f;                       //작열 데미지
    private float bloodSuckingValue = 1.2f;                       //흡혈 회복량
    private float martyrdomValue = 1.2f;                          //순교 상승량
    private float mybindingAttackDamage = 5;                        //결속 추가 공격력
    private float enemybindingAttackDamage = 5;                     

    private float waittingTime = 500;

    List<RogueUnitDataBase> myUnits = new();
    List<RogueUnitDataBase> enemyUnits = new();
    List<RogueUnitDataBase> myDeathUnits = new();
    List<RogueUnitDataBase> enemyDeathUnits = new();

    //유닛 전투 통계
    private Dictionary<int, UnitCombatStatics> unitStats = new();

    //보유 영웅
    private Dictionary<int, bool> myHeroUnits =new();
    private Dictionary<int, bool> enemyHeroUnits= new();
    //보유 유물
    private List<WarRelic> ownedRelics = new List<WarRelic>();
    // 중복 방지를 위한 HashSet
    private HashSet<int> ownedRelicIds = new HashSet<int>();

    //유닛별 고유 id
    private static int globalUnitId = 0;

    // 유닛 순서
    int myUnitIndex;
    int enemyUnitIndex;

    //유닛 총 갯수
    int myUnitMax;
    int enemyUnitMax;

    //사망 유닛 위치
    int myDeathUnitIndex;
    int enemyDeathUnitIndex =0;

    //후열 유닛 체력 (암살에 사용)
    float myBackUnitHp;
    float enemyBackUnitHp;

    // 아군 원거리 유닛 갯수
    List<int> myRangeUnits = new();
    // 상대 원거리 유닛 갯수
    List<int> enemyRangeUnits = new();

    // 첫 공격
    bool isFirstAttack = true;

    private enum BattleState
    {
        None,
        Preparation,
        Crash,
        Support,
        Death,
        End
    }

    private BattleState currentState = BattleState.None;

    private bool isProcessing= false;

    //데미지 배율
    private float myFinalDamage = 0;
    private float enemyFinalDamage = 0;



    //이 씬이 로드되었을 때== 구매 배치로 전투 씬 입장했을때
    private async void Start()
    {
        currentState = BattleState.None;

        List<int> myIds = PlayerData.Instance.ShowPlacedUnitList();
        List<int> enemyIds = PlayerData.Instance.GetEnemyUnitIndexes();
        if (myIds.Count <= 0) return;

        await InitializeBattle(myIds, enemyIds);
    }

    public async Task<List<RogueUnitDataBase>> LoadRogueUnitData()
    {
        await sheetLoader.LoadUnitSheetData();

        List<RogueUnitDataBase> rogueUnits = new();

        // 전체 유닛 데이터 가져오기
        List<List<string>> unitData = sheetLoader.GetUnitExcelData();
        if (unitData == null) return rogueUnits;

        // 모든 행을 `RogueUnitDataBase`로 변환
        foreach (var row in unitData)
        {
            RogueUnitDataBase unit = RogueUnitDataBase.ConvertToUnitDataBase(row);
            if (unit != null)
            {
                rogueUnits.Add(unit);
            }
        }

        return rogueUnits;
    }

    private async void Update()
    {
        if(isProcessing || Time.timeScale == 0) return;
        
        switch (currentState) 
        { 
            case BattleState.None:
                break;
            case BattleState.Preparation:
                if (HandleEnd()) break;
                // 암살로 인한 시체 유닛 발생 시
                if (myUnits[myUnitIndex].health <= 0)
                {
                    myUnitIndex++;
                    break;
                }
                if (enemyUnits[enemyUnitIndex].health <= 0)
                {
                    enemyUnitIndex++;
                    break;
                }
                await HandlePhase(HandlePreparation);
                break;
            case BattleState.Crash:
                await HandlePhase(HandleCrash);
                break;
            case BattleState.Support:
                await HandlePhase(HandleSupport);
                break;
            case BattleState.End:
                break;
        }
    }


    //유닛 id를 바탕으로 유닛 데이터 저장
    private async Task<(List<RogueUnitDataBase>, List<RogueUnitDataBase>)> GetUnits(List<int> myUnitIds, List<int> enemyUnitIds)
    {
        // Google Sheet에서 전체 유닛 데이터를 로드
        List<RogueUnitDataBase> allUnits = await LoadRogueUnitData();

        // 내 유닛 ID들을 기반으로 유닛을 가져와서 myUnits에 저장
        foreach (int unitId in myUnitIds)
        {
            List<string> rowData = sheetLoader.GetRowUnitData(unitId);

            if (rowData != null)
            {
                RogueUnitDataBase unit = RogueUnitDataBase.ConvertToUnitDataBase(rowData);
                // 고유 ID 추가
                unit.UniqueId = GenerateUniqueUnitId(unit.branchIdx, true, unit.idx);
                // 강화 후 추가
                myUnits.Add(UpgradeManager.Instance.UpgradeRogueLikeUnit(unit));
            }
        }

        // 적의 유닛 ID들을 기반으로 유닛을 가져와서 enemyUnits에 저장
        foreach (int unitId in enemyUnitIds)
        {
            List<string> rowData = sheetLoader.GetRowUnitData(unitId);
            if (rowData != null)
            {
                RogueUnitDataBase unit = RogueUnitDataBase.ConvertToUnitDataBase(rowData);
                // 고유 ID 추가
                unit.UniqueId = GenerateUniqueUnitId(unit.branchIdx, false, unit.idx);
                enemyUnits.Add(unit);
            }
            
        }

        return (myUnits, enemyUnits);
    }


    //유닛 데이터 받고 전투 시작
    public async Task StartBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        await InitializeBattle(_myUnitIds, _enemyUnitIds);
    }


    //페이즈 관리
    private async Task HandlePhase(Func<Task> phaseHandler)
    {
        isProcessing = true;
        await phaseHandler();
        UpdateUnitHp();
        //유닛이 죽지 않았고 끝나지 않음
        if (!ManageUnitDeath() && !(myUnits.Count ==0 || enemyUnits.Count ==0))
        {
            switch (currentState)
            {
                case BattleState.Preparation:
                    currentState = BattleState.Crash;
                    break;
                case BattleState.Crash:
                    currentState = BattleState.Support;
                    break;
                case BattleState.Support:
                    isFirstAttack = false;
                    currentState = BattleState.Preparation;
                    break;
            }
        }
        else    //유닛 사망 시 승패무 채크 후 준비 페이즈로
        {
            HandleEnd();
        }
        await Task.Delay((int)waittingTime); // 0.5초 대기
        isProcessing = false;
        
    }

    //유닛 갯수 최신화
    private void UpdateUnitCount()
    {
        int myUnitCount = 0;
        int enemyUnitCount = 0;

        foreach (RogueUnitDataBase unitData in myUnits)
        {
            if (unitData.health > 0)
            {
                myUnitCount++;
            }
        }
        foreach (RogueUnitDataBase unitData in enemyUnits)
        {
            if(unitData.health > 0)
            {
                enemyUnitCount++;
            }
        }

         autoBattleUI.UpdateUnitCountUI(myUnitCount, enemyUnitCount);
    }


    //유닛 체력 최신화
    private void  UpdateUnitHp()
    {
        //범위 확인
        if (myUnits.Count== 0 || enemyUnits.Count==0) return;
        float myUnitHp = myUnits[0].health;
        float enemyHp = enemyUnits[0].health;
        float myMAxHp = myUnits[0].maxHealth;
        float enemyMaxHp = enemyUnits[0].maxHealth;
       
        if (myUnitHp < 0)
        {
            myUnitHp = 0;
        }
        if (enemyHp < 0)
        {
            enemyHp = 0;
        }
        autoBattleUI.UpateUnitHPUI(MathF.Floor(myUnitHp),MathF.Floor(enemyHp), MathF.Floor(myMAxHp), MathF.Floor(enemyMaxHp));

    }

    //회피율 계산
    private float CalculateDodge(RogueUnitDataBase unit)
    {
        float dodge ;
        float smoke = 0;
        // 키 2가 존재하는지 확인 후 값을 가져옴
        if (unit.effectDictionary.TryGetValue(2, out BuffDebuffData _))
        {
            smoke = 15;
        }
        dodge = (2 + (13 / 9) * (unit.mobility - 1)) +(unit.agility?10.0f:0) + smoke;

        return Mathf.Clamp(dodge, 0, 100);
    }

    //회피 유무 계산
    private bool CalculateAccuracy(RogueUnitDataBase unit,bool isPerfectAccuracy)
    {
        if (isPerfectAccuracy)
            return false; // 필중 특성인 경우 회피 불가

        float dogeRate= CalculateDodge(unit);

        bool result= dogeRate > Random.Range(0, 100);

        return result;
    }

    //돌진 계산
    private float CalculateCharge(float mobility)
    {
         return 1.1f + (1.9f / (9 * (mobility - 1)));
    }

    //암살 발동 가능한지 확인: 상대 후열 유닛 생존
    private bool CheckAssassination(List<RogueUnitDataBase> units,int unitIndex)
    {
        for (int i = unitIndex + 1; i < units.Count; i++)
        {
            if (units[i].health > 0)
            {
                return true;
            }
        }

        return false;
    }

    // 암살 대상 계산: 체력이 가장 낮은 유닛의 인덱스를 반환
    private int CalculateAssassination(List<float> unitsHealth)
    {
        // 최소 체력 유닛 번호 (-1은 유효한 유닛이 없는 경우를 대비)
        int minHealthNumber = -1;
        // 최소 체력 기준 (초기값은 float.MaxValue로 설정)
        float lastHealth = float.MaxValue;

        for (int i = 0; i < unitsHealth.Count; i++)
        {
            // 체력이 0보다 크고, 현재 최소 체력보다 작은 경우 갱신
            if (unitsHealth[i] > 0 && unitsHealth[i] < lastHealth)
            {
                lastHealth = unitsHealth[i];
                minHealthNumber = i;
            }
        }

        // 최소 체력 유닛 번호 반환 (유효한 유닛이 없으면 -1 반환)
        return minHealthNumber+1;
    }


    //데미지 ui 호출
    private void CallDamageText(float damage,string text,bool team,int unitIndex=0)
    {
        autoBattleUI.ShowDamage(MathF.Floor( damage), text, team,unitIndex);
    }

    // 유닛 생성UI 호출
    private void CallCreateUnit()
    {
        List<RogueUnitDataBase> myRangeUnits=new();
        List<RogueUnitDataBase> enemyRangUnits=new();
        List<RogueUnitDataBase> myAliveUnits = new();
        List<RogueUnitDataBase> enemyAliveUnits = new();
        //범위 확인
        int myCurrentIndex = myUnitIndex >= myUnitMax ? myUnitMax - 1 : myUnitIndex;
        int enemyCurrentIndex = enemyUnitIndex >= enemyUnitMax ? enemyUnitMax - 1 : enemyUnitIndex;
        //원거리 공격이 가능한지 검사
        for (int i = myCurrentIndex + 1; i < myUnits.Count; i++)
        {
            if (myUnits[i].rangedAttack && (myUnits[i].range - (i - myCurrentIndex) > 0) && myUnits[i].health>0)
            {
                myRangeUnits.Add(myUnits[i]);
            }
        }
        for (int i = enemyCurrentIndex + 1; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i].rangedAttack && (enemyUnits[i].range - (i - enemyCurrentIndex) > 0) && enemyUnits[i].health > 0)
            {
                enemyRangUnits.Add(enemyUnits[i]);
            }
        }
        foreach (RogueUnitDataBase unit in myUnits)
        {
            if (unit.health > 0)
            {
                myAliveUnits.Add(unit);
            }
        }
        foreach(RogueUnitDataBase unit in enemyUnits)
        {
            if(unit.health > 0)
            {
                enemyAliveUnits.Add(unit);
            }
        }
        if(myAliveUnits.Count == 0)
        {
            myAliveUnits.Add(myUnits.Last());
        }
        if(enemyAliveUnits.Count == 0)
        {
            enemyAliveUnits.Add(enemyUnits.Last());
        }


        autoBattleUI.CreateUnitBox(myAliveUnits, enemyAliveUnits, CalculateDodge(myUnits[myCurrentIndex]), CalculateDodge(enemyUnits[enemyCurrentIndex]),myRangeUnits,enemyRangUnits);
    }
    //준비 페이즈
    private void PreparationPhase()
    {
        abilityManager.ProcessPreparationAbility(myUnits, enemyUnits, isFirstAttack, true, myFinalDamage);
        abilityManager.ProcessPreparationAbility(enemyUnits,myUnits,isFirstAttack, false, enemyFinalDamage);
    }

    //준비 페이즈
    private void PreparationPhases()
    {
        //후열 유닛 체력
        myBackUnitHp = 1;
        enemyBackUnitHp = 1;
        //전투당 한번
        enemyDeathUnitIndex = CalculateFirstStrike(ref myUnits, myUnitIndex,ref enemyUnits, enemyUnitIndex, true);
        myDeathUnitIndex = CalculateFirstStrike(ref enemyUnits, enemyUnitIndex, ref myUnits, myUnitIndex, false);

        //첫공격
        if (isFirstAttack)
        {
            string mySkills = "";
            string enemySkills = "";

            //총 데미지
            float myAllDamage;
            float enemyAllDamage;

            //유산 추가 데미지
            float relicDamage=0;

            //암살 위치
            int myMinHealthIndex=0;
            int enemyMinHealthIndex=0;

            (myAllDamage,enemyMinHealthIndex) = ProcessPreparation(
                ref myUnits,myUnitIndex, ref enemyUnits, enemyUnitIndex, ref mySkills, ref enemyBackUnitHp, ref enemyDeathUnitIndex,myFinalDamage,true);
            (enemyAllDamage,myMinHealthIndex) = ProcessPreparation(
                ref enemyUnits, enemyUnitIndex, ref myUnits, myUnitIndex, ref enemySkills,ref myBackUnitHp,ref myDeathUnitIndex,enemyFinalDamage,false);

            if (enemyAllDamage > 0)
            {
                //유산 28
                if (myUnits[myUnitIndex].health<=0 && RelicManager.HeartGemNecklace()) 
                {
                    float heal = HealHealth(myUnits[myUnitIndex], myUnits[myUnitIndex].maxHealth);
                    myUnits[myUnitIndex].health = heal;

                    CallDamageText(-myUnits[myUnitIndex].maxHealth, "회복", false);
                }

                //15
                relicDamage += RelicManager.ReactiveThornArmor(myUnits[myUnitIndex]);

                myAllDamage += relicDamage;

                //통계 기록: 입힌 피해, 받은 피해
                unitStats[enemyUnits[enemyUnitIndex].UniqueId].AddDamageDealt(enemyAllDamage);
                CallDamageText(enemyAllDamage, enemySkills, false, myMinHealthIndex);
            }

            if (myAllDamage > 0)
            {
                //통계 기록: 입힌 피해, 받은 피해
                unitStats[myUnits[myUnitIndex].UniqueId].AddDamageDealt(myAllDamage);
                CallDamageText(myAllDamage, mySkills, true,enemyMinHealthIndex);
                
            }
        }

    }

    //충돌 페이즈
    private void ChrashPhase()
    {
        abilityManager.ProcessChrashAbility(myUnits, enemyUnits, isFirstAttack, myFinalDamage, true);
        abilityManager.ProcessChrashAbility(enemyUnits,myUnits, isFirstAttack, enemyFinalDamage, false);
    }

    //충돌 페이즈
    private void ChrashPhases()
    {
        float myDamage;
        float enemyDamage;

        string mySkills ="충돌 ";
        string enemySkills ="충돌 ";

        float relicDamage = 0;

        //내 유닛 공격
        myDamage =
             ProcessChrash(ref myUnits,myUnitIndex,ref enemyUnits, enemyUnitIndex , ref mySkills, ref enemySkills,myFinalDamage,true);
        //상대 유닛 공격
        enemyDamage =
             ProcessChrash(ref enemyUnits,enemyUnitIndex,ref myUnits, myUnitIndex, ref enemySkills, ref mySkills,enemyFinalDamage,false);

        if (enemyDamage > 0)
        {
            //유산 28
            if (myUnits[myUnitIndex].health <= 0 && RelicManager.HeartGemNecklace()) myUnits[myUnitIndex].health = myUnits[myUnitIndex].maxHealth;

            //15
            relicDamage += RelicManager.ReactiveThornArmor(myUnits[myUnitIndex]);

            myDamage += relicDamage;
        }

        //통계 기록: 입힌 피해, 받은 피해, 전투 충돌
        unitStats[myUnits[myUnitIndex].UniqueId].AddDamageDealt(myDamage);
        unitStats[myUnits[myUnitIndex].UniqueId].AddDamageTaken(enemyDamage);
        unitStats[enemyUnits[enemyUnitIndex].UniqueId].AddDamageDealt(enemyDamage);
        unitStats[enemyUnits[enemyUnitIndex].UniqueId].AddDamageTaken(myDamage);
        unitStats[myUnits[myUnitIndex].UniqueId].IncrementCombatEncounters();
        unitStats[enemyUnits[enemyUnitIndex].UniqueId].IncrementCombatEncounters();

        //
        CallDamageText(myDamage, mySkills, true);
        //
        CallDamageText(enemyDamage, enemySkills, false);

    }

    //지원 페이즈
    private void SupportPhase()
    {
        abilityManager.ProcessSupportAbility(myUnits, enemyUnits, true, myFinalDamage);
        abilityManager.ProcessSupportAbility(enemyUnits,myUnits, false, enemyFinalDamage);

    }

    //지원 페이즈
    private void SupportPhases()
    {
        float myDamage = 0;
        float enemyDamage = 0;

        float relicDamage = 0;
        
        //상대 공격
        enemyDamage=ProcessRangeAttack(ref enemyUnits, ref myUnits, enemyRangeUnits, enemyUnitIndex, myUnitIndex, false,enemyFinalDamage);
        if (enemyDamage > 0)
        {
            myUnits[myUnitIndex].health -= enemyDamage;
            
            //유산 28
            if(myUnits[myUnitIndex].health <= 0 && RelicManager.HeartGemNecklace()) myUnits[myUnitIndex].health = myUnits[myUnitIndex].maxHealth;

            CallDamageText(enemyDamage, "원거리 ", false);

            //통계 기록: 받은 피해
            unitStats[myUnits[myUnitIndex].UniqueId].AddDamageTaken(enemyDamage);

            //유산 작동
            //15
            relicDamage = RelicManager.ReactiveThornArmor(myUnits[myUnitIndex]);
            myDamage += relicDamage;
        }

        //내 공격
        myDamage = ProcessRangeAttack(ref myUnits, ref enemyUnits, myRangeUnits, myUnitIndex, enemyUnitIndex, true, myFinalDamage);
        if (myDamage > 0)
        {
            enemyUnits[enemyUnitIndex].health -= myDamage;

            CallDamageText(myDamage, "원거리 ", true);

            //통계 기록: 받은 피해
            unitStats[enemyUnits[enemyUnitIndex].UniqueId].AddDamageTaken(myDamage);
        }

        //치유
        ProcessCure(ref myUnits, myUnitIndex,false);
        ProcessCure(ref enemyUnits, enemyUnitIndex,true);

        //지원 종료
        DamageBurning(ref myUnits, myUnitIndex,false);
        DamageBurning(ref enemyUnits, enemyUnitIndex,true);
    }
    

    //유닛 UI최신화
    private void UpdateUnitUI()
    {
        //유닛 생성 UI
        CallCreateUnit();

        //유닛 숫자 UI 최신화
        UpdateUnitCount();

        //유닛 체력 UI 최신화
        UpdateUnitHp();
    }
    //유닛 사망 처리
    private bool ManageUnitDeath()
    {
        return abilityManager.ProcessDeath(ref myUnits, ref enemyUnits, ref myDeathUnits, ref enemyDeathUnits, ref isFirstAttack);
    }

    //유닛 사망 처리 통합본
    private bool ManageUnitDeaths()
    {
        //유닛 사망 검사
        if (myUnits[myUnitIndex].health <= 0 || enemyUnits[enemyUnitIndex].health <= 0 || myBackUnitHp <=0 || enemyBackUnitHp <=0)
        {
            int myAdd=0;
            int enemyAdd=0;
            bool isGuerrilla=false;
            ProcessDeath(ref myUnits,ref enemyUnits, myUnitIndex, enemyUnitIndex, enemyBackUnitHp, enemyDeathUnitIndex, ref enemyAdd, true,ref isGuerrilla);
            ProcessDeath(ref enemyUnits,ref myUnits, enemyUnitIndex,myUnitIndex,myBackUnitHp,myDeathUnitIndex,ref myAdd, false,ref isGuerrilla);
            // 암살+유격으로 죽은게 아닌 경우 == 다음 유닛으로 넘어감
            if (myBackUnitHp > 0 && enemyBackUnitHp > 0)
            {
                myUnitIndex += myAdd;
                enemyUnitIndex += enemyAdd;

                isFirstAttack = true;
            }
            else if (isGuerrilla)
            {
                isFirstAttack = true;
            }
            else
            {
                myDeathUnitIndex = 0;
                enemyDeathUnitIndex = 0;

                return false;
            }
            if (myAdd > 0 && RelicManager.Relic55())
            {
                if (myBackUnitHp > 0)
                {
                    enemyUnits[enemyUnitIndex].health -= myUnits[myUnitIndex].health*0.1f;
                }
                else
                {
                    enemyUnits[enemyUnitIndex].health -= myUnits[myDeathUnitIndex].health * 0.1f;
                }

                ManageUnitDeath();
            }

            myDeathUnitIndex =0;
            enemyDeathUnitIndex = 0;

            return true;
        }
        return false;
    }
    
    //유닛 데이터 초기화
    private async Task InitializeBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        if (autoBattleUI == null)
        {
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        }

        //로딩창 시작
        autoBattleUI.ToggleLoadingWindow();
        
        // 유닛 데이터 받아옴
        (myUnits, enemyUnits) = await GetUnits(_myUnitIds, _enemyUnitIds);

        myUnitIndex = 0;
        enemyUnitIndex = 0;

        myDeathUnitIndex = 0;
        enemyDeathUnitIndex = 0;

        myUnitMax = myUnits.Count;
        enemyUnitMax = enemyUnits.Count;

        myBackUnitHp = 1;
        enemyBackUnitHp = 1;

        myRangeUnits = new List<int>();
        enemyRangeUnits = new List<int>();

        isFirstAttack = true;

        // 원거리 유닛 필터링
        for (int i = 0; i < myUnitMax; i++)
            if (myUnits[i].rangedAttack) myRangeUnits.Add(i);

        for (int i = 0; i < enemyUnitMax; i++)
            if (enemyUnits[i].rangedAttack) enemyRangeUnits.Add(i);

        //유산 및 데이터 저장
        ProcessRelic();

        // 통계 초기화
        foreach (var unit in myUnits)
        {
            if (!unitStats.ContainsKey(unit.UniqueId))
            {
                unitStats[unit.UniqueId] = new UnitCombatStatics(unit.UniqueId, unit.unitName); // 아군
            }
        }

        foreach (var unit in enemyUnits)
        {
            if (!unitStats.ContainsKey(unit.UniqueId))
            {
                unitStats[unit.UniqueId] = new UnitCombatStatics(unit.UniqueId, unit.unitName); // 적군
            }
        }

        ProcessBeforeBattle(ref myUnits,true);
        ProcessBeforeBattle(ref enemyUnits,false);

        //로딩창 종료
        autoBattleUI.ToggleLoadingWindow();

        // 상태를 Preparation으로 설정
        currentState = BattleState.Preparation;
    }

    //준비 페이즈 관리
    private async Task HandlePreparation()
    {
        UpdateUnitUI();

        PreparationPhase();
        await Task.Yield();
    }

    //충돌 페이즈 관리
    private async Task HandleCrash()
    {
        ChrashPhase();

        await Task.Yield();
    }

    //지원 페이즈 관리
    private async Task HandleSupport()
    {
        SupportPhase();

        await Task.Yield();
    }
    //종료 확인
    private int CheckEnd()
    {
        if (myUnits.Count == 0 || enemyUnits.Count == 0)
        {
            if (enemyUnits.Count == 0 || enemyUnits.Count > 0)
            {
                Debug.Log("나의 승리");
                return 0;
            }
            else if (enemyUnits.Count > 0 || myUnits.Count == 0)
            {
                Debug.Log("나의 패배");
                return 1;
            }
            else
            {
                Debug.Log("무승부");
                return 2;
            }
        }

        return 3;
    }
    //종료 확인
    private int CheckEnds()
    {
        if (myUnitIndex >= myUnitMax || enemyUnitIndex >= enemyUnitMax)
        {
            // 전투 종료 후 승리 여부 판단
            if (myUnitIndex < myUnitMax && enemyUnitIndex >= enemyUnitMax)
            {
                Debug.Log($"나의 승리");
                return 0;  // 나의 승리
            }
            else if (enemyUnitIndex < enemyUnitMax && myUnitIndex >= myUnitMax)
            {
                Debug.Log($"나의 패배");
                return 1;  // 적이 승리
            }
            else
            {
                Debug.Log("무승부");
                return 2;  // 양쪽 모두 사망
            }
        }

        //유닛이 남았을 때
        return 3;
    }

    //사망시 투명하게 ui업뎃
    private void ChangeInvisibile(bool isMyUnit, int unitIndex)
    {
        autoBattleUI.ChangeInvisibleUnit(unitIndex, isMyUnit);
    }

    //속도 관리
    public void ChangeWaittingTime(float multiple)
    {
        waittingTime *= multiple;
    }

    //종료관리 전투가 끝났을때 나오게 될것들
    private bool HandleEnd()
    {
        int result = CheckEnd();
        if (result == 3)
        {
            currentState = BattleState.Preparation;
        }
        else
        {
            currentState = BattleState.End;

            //종료 시 UI 표기
            autoBattleUI.FightEnd(result);

            //점수 표기
            ManageScore(result);

            //통계 표시 
            UnitCombatStatics.ProcessBattleStatics(unitStats,autoBattleUI);

            //ui마지막 업데이트
            //유닛 숫자 UI 최신화
            UpdateUnitCount();

            //유닛 체력 UI 최신화
            UpdateUnitHp();

            return true;
        }
        return false;
    }

    //준비 계산
    private (float,int) ProcessPreparation(ref List<RogueUnitDataBase> attackers,int attackerIndex,ref List<RogueUnitDataBase> defenders,int defenderIndex, ref string attackerSkill, ref float backUnitHp,ref int deathUnitIndex,float finalDamage,bool isTeam)
    {
        float allDamage = 0;
        int minHealthIndex = 0;
        //연막
        CalculateSmokeScreen(ref defenders,defenderIndex);
        //위압
        if (attackers[attackerIndex].overwhelm)
        {
            attackerSkill += "위압 ";
            defenders[defenderIndex].mobility =overwhelmValue;

            //통계 기록: 위압 스킬 사용
            unitStats[attackers[attackerIndex].UniqueId].IncrementSkillUsage("위압");
        }
        //투창
        if (attackers[attackerIndex].throwSpear)
        {
            attackerSkill += "투창 ";

            float damage = throwSpearValue*finalDamage;

            //유산 작동
            if (isTeam)
            {
                damage *= RelicManager.TechnicalManual() ? 1.2f : 1;
            }

            //통계 기록: 투창 발동
            unitStats[attackers[attackerIndex].UniqueId].IncrementSkillUsage("투창");
            //회피 계산 
            if (!CalculateAccuracy(defenders[defenderIndex], attackers[attackerIndex].perfectAccuracy))
            {
                allDamage += damage;
                defenders[defenderIndex].health -=damage;

                //통계 기록: 투창 피해, 받은 피해
                unitStats[attackers[attackerIndex].UniqueId].AddDamageBySkill("투창", damage);
                unitStats[defenders[defenderIndex].UniqueId].AddDamageTaken(damage);
            }
            else
            {
                attackerSkill = "회피 ";

                //통계 기록: 회피, 회피한 피해
                unitStats[defenders[defenderIndex].UniqueId].IncrementDodges();
                unitStats[defenders[defenderIndex].UniqueId].AddDodgedDamage(damage);

            }
        }
        //암살 && 방어자 후열 유닛 존재
        if(attackers[attackerIndex].assassination && CheckAssassination(defenders, defenderIndex))
        {
            attackerSkill += "암살 ";
            float damage = attackers[attackerIndex].attackDamage * assassinationValue*finalDamage;

            //유산 작동
            if (isTeam)
            {
                damage *= RelicManager.TechnicalManual() ? 1.2f : 1;
            }

            // 통계 기록: 암살 스킬 사용
            unitStats[attackers[attackerIndex].UniqueId].IncrementSkillUsage("암살");
            if (defenders[defenderIndex].guard)
            {
                attackerSkill += "수호 ";

                // 통계 기록: 수호 스킬 사용
                unitStats[defenders[defenderIndex].UniqueId].IncrementSkillUsage("수호");
                if (!CalculateAccuracy(defenders[defenderIndex], attackers[attackerIndex].perfectAccuracy))
                {
                    float actualDamage = damage * (1 - (defenders[defenderIndex].armor / (10 + defenders[defenderIndex].armor)));

                    defenders[defenderIndex].health -= actualDamage;

                    allDamage += actualDamage;

                    // 통계 기록: 암살 피해, 장갑 감소 피해, 받은 피해
                    unitStats[attackers[attackerIndex].UniqueId].AddDamageBySkill("암살", actualDamage);
                    unitStats[defenders[defenderIndex].UniqueId].AddArmorDamageReduced(actualDamage);
                    unitStats[defenders[defenderIndex].UniqueId].AddDamageTaken(damage-actualDamage);
                }
                else
                {
                    //통계 기록: 회피, 회피한 피해
                    unitStats[defenders[defenderIndex].UniqueId].IncrementDodges();
                    unitStats[defenders[defenderIndex].UniqueId].AddDodgedDamage(damage);

                    attackerSkill += "회피 ";
                }
            }
            else
            {
                //후열 유닛 체력 저장
                List<float> defendersHealth = new();
                for (int i = defenderIndex + 1; i < defenders.Count; i++)
                {
                    defendersHealth.Add(defenders[i].health);
                }

                minHealthIndex = defenderIndex + CalculateAssassination(defendersHealth);

                backUnitHp = defenders[minHealthIndex].health;
                backUnitHp -= damage;
                defenders[minHealthIndex].health = backUnitHp;

                //최종 피해량 추가
                allDamage += damage;

                //통계 기록: 암살 피해, 받은 피해
                unitStats[attackers[attackerIndex].UniqueId].AddDamageBySkill("암살", damage);
                unitStats[defenders[minHealthIndex].UniqueId].AddDamageTaken(damage);

                int deadUnitCount = 0;
                for (int i = defenderIndex + 1; i < minHealthIndex; i++)
                {
                    if (defenders[i].health <= 0)
                    {
                        deadUnitCount++;
                    }
                }
                minHealthIndex = minHealthIndex - defenderIndex - deadUnitCount;
                //후열 유닛 사망시 대입
                if (backUnitHp <= 0 && deathUnitIndex==0)
                {
                    deathUnitIndex = minHealthIndex;

                    //통계 기록: 암살로 처치
                    unitStats[attackers[attackerIndex].UniqueId].IncrementAssassinations();
                }

                //복수
                if (defenders[defenderIndex].vengeance)
                {
                    Debug.Log("복수"+ damage);
                    attackers[attackerIndex].health -= damage;
                }
            }
        }
        //상흔
        if (attackers[attackerIndex].wounding)
        {
            int scarId = 1;
            int type = 1;
            int rank = 1;
            int duration = -1;
            defenders[defenderIndex].effectDictionary[scarId] = new BuffDebuffData(scarId, type, rank, duration);
        }

        return (allDamage,minHealthIndex);
    }


    //충돌 첫공 계산
    private (float, float) CalculateChrashIsFirstAttack(RogueUnitDataBase attacker,ref List<RogueUnitDataBase> defenders,int defenderIndex, ref string attackerSkill, ref string defenderSkill)
    {
        float multiplier = 1;
        float reduceDamage = 0;
        //첫 공 일 때
        if (isFirstAttack)
        {
            //돌격
            if (attacker.charge)
            {
                multiplier = CalculateCharge(attacker.mobility);

                //강한 돌격
                if (attacker.strongCharge)
                {
                    multiplier += strongChargeValue;
                    attackerSkill += "강한돌격 ";

                    //통계 기록: 강한돌격 발동
                    unitStats[attacker.UniqueId].IncrementTraitUsage("강한돌격");
                }
                else
                {
                    attackerSkill += "돌격 ";

                    //통계 기록: 돌격 발동
                    unitStats[attacker.UniqueId].IncrementTraitUsage("돌격");
                }
                //수비태세 = 방어자가 수배태세 일때
                //공격자 데미지 감소
                if (defenders[defenderIndex].defense)
                {
                    reduceDamage += defenseValue;
                }

            }
            if (attacker.defense)
            {
                if (defenders[defenderIndex].charge)
                {
                    reduceDamage -= defenseValue;
                }
                else
                {
                    reduceDamage += defenseValue;
                }
                attackerSkill += "수비태세";

                //통계 기록: 수비태세 발동
                unitStats[attacker.UniqueId].IncrementTraitUsage("수비태세");
            }
        }
        
        return (multiplier, reduceDamage);
    }

    //충돌 계산
    private float ProcessChrash(ref List<RogueUnitDataBase> attackers,int attackerIndex,ref List<RogueUnitDataBase> defenders,int defenderIndex, ref string attackerSkill,ref string defenderSkill,float finalDamage,bool isTeam)
    {
        float multiplier;
        float reduceDamage;
        float damage;
        float actualDamage;

        (multiplier, reduceDamage) = CalculateChrashIsFirstAttack(attackers[attackerIndex],ref defenders,defenderIndex, ref attackerSkill,ref defenderSkill);

        float bonusDamage = -reduceDamage;

        //둔기
        if (attackers[attackerIndex].bluntWeapon && defenders[defenderIndex].heavyArmor)
        {
            bonusDamage += bluntWeaponValue;
            attackerSkill += "둔기 ";

            //통계 기록: 둔기 발동
            unitStats[attackers[attackerIndex].UniqueId].IncrementTraitUsage("둔기");
        }
        //도살
        if (attackers[attackerIndex].slaughter && defenders[defenderIndex].lightArmor)
        {
            bonusDamage += slaughterValue;
            attackerSkill += "도살 ";

            //통계 기록: 도살 발동
            unitStats[attackers[attackerIndex].UniqueId].IncrementTraitUsage("도살");
        }
        //대기병
        if (attackers[attackerIndex].antiCavalry>0 &&(defenders[defenderIndex].branchIdx == 5 || defenders[defenderIndex].branchIdx == 6))
        {
            bonusDamage += attackers[attackerIndex].antiCavalry;
            attackerSkill += "대기병 ";

            //통계 기록: 대기병 발동, 피해
            unitStats[attackers[attackerIndex].UniqueId].IncrementTraitUsage("대기병");
            unitStats[attackers[attackerIndex].UniqueId].AddAdditionalDamageByTrait("대기병", attackers[attackerIndex].antiCavalry);
        }
        //제압
        if (attackers[attackerIndex].suppression && (bonusDamage>0))
        {
            attackerSkill += "제압 ";

            bonusDamage *= suppressionValue;
        }

        damage = attackers[attackerIndex].attackDamage * multiplier;
        actualDamage = damage+ bonusDamage;
        
        //관통
        if (attackers[attackerIndex].pierce)
        {
            attackerSkill += "관통 ";

            //통계 기록: 관통 발동, 관통 추가 피해
            unitStats[attackers[attackerIndex].UniqueId].IncrementTraitUsage("관통");
            unitStats[attackers[attackerIndex].UniqueId].AddPierceDamageGained(damage - (damage * (1 - (defenders[defenderIndex].armor) / (10 + defenders[defenderIndex].armor))));
        }
        else
        {
            //장갑 계산
            damage *= (1 - (defenders[defenderIndex].armor) / (10 + defenders[defenderIndex].armor));
        }

        damage = (damage+ bonusDamage)*finalDamage;
        actualDamage *= finalDamage;

        //팔랑크스
        if (!isTeam && attackers[attackerIndex].branchIdx == 2 && RelicManager.PhalanxTacticsBook())
        {
            damage *= 0.5f;
        }

        float totalDamage = 0;

        for (int i = 0; i < 2; i++)
        {
            if (i == 1)
            {
                attackerSkill += "연발 ";
                if(!attackers[attackerIndex].doubleShot) break; // rapidFire가 없으면 한 번만 공격
            }

            float normalDamage = damage;

            //충격
            if (isFirstAttack && attackers[attackerIndex].charge && attackers[attackerIndex].impact)
            {
                int impactIndex = -1;

                for (int k = defenderIndex + 1; k < defenders.Count; k++)
                {
                    if (defenders[k].health > 0)
                    {
                        impactIndex = k;
                        break;
                    }
                }

                if (impactIndex > 0)
                {
                    if (i == 0) attackerSkill += "충격 ";

                    float impactDamage = normalDamage * (1 - defenders[impactIndex].armor / (defenders[impactIndex].armor + 10));

                    defenders[impactIndex].health -= impactDamage;

                    Debug.Log("충격" + impactDamage);
                    //복수
                    if (defenders[defenderIndex].vengeance)
                    {
                        attackers[attackerIndex].health -= impactDamage;

                        Debug.Log("복수" + impactDamage);
                    }

                }
            }

            // 회피 판정
            if (CalculateAccuracy(defenders[defenderIndex], attackers[attackerIndex].perfectAccuracy))
            {
                if (i == 0)
                {
                    attackerSkill += "회피 ";
                }
                unitStats[defenders[defenderIndex].UniqueId].IncrementDodges();
                unitStats[defenders[defenderIndex].UniqueId].AddDodgedDamage(actualDamage);
                normalDamage = 0;
            }
            else
            {
                //반격
                if(isFirstAttack && defenders[defenderIndex].counter)
                {
                    if(!CalculateAccuracy(attackers[attackerIndex], defenders[defenderIndex].perfectAccuracy))
                    {
                        Debug.Log("반격" + normalDamage);
                        attackers[attackerIndex].health -= normalDamage;
                    }
                    else
                    {
                        Debug.Log("반격회피");
                    }
                }
                else
                {
                    defenders[defenderIndex].health -= normalDamage;
                }

                //작열
                CalculateBurning(attackers[attackerIndex], ref defenders, defenderIndex);
                if (i == 0) attackerSkill += "작열 ";

                // 가시 피해
                if (defenders[defenderIndex].thorns)
                {
                    if(i==0) defenderSkill += "가시 ";
                    attackers[attackerIndex].health -= thornsDamageValue;
                }

                // 흡혈 (한 번만 적용)
                if (attackers[attackerIndex].lifeDrain)
                {
                    if (i == 0) attackerSkill += "흡혈 ";
                    float heal =HealHealth(attackers[attackerIndex], Mathf.Min(Mathf.Round(attackers[attackerIndex].health * bloodSuckingValue), attackers[attackerIndex].maxHealth));
                    attackers[attackerIndex].health = heal;
                }

                unitStats[defenders[defenderIndex].UniqueId].AddArmorDamageReduced(actualDamage - normalDamage);

                
            }

            totalDamage += normalDamage;
            CallDamageText(normalDamage, attackerSkill, isTeam);
        }

        return totalDamage;
    }

    //원거리 공격
    private float ProcessRangeAttack(ref List<RogueUnitDataBase> attackers,ref List<RogueUnitDataBase> defenders,List<int> rangeUnits,int attackerIndex,int defenderIndex,bool isTeam,float finalDamage)
    {
        float allDamage = 0;
        if (rangeUnits.Count > 0)
        {
            for (int i = 0; i < rangeUnits.Count; i++)
            {
                if(rangeUnits[i] > attackerIndex && attackers[rangeUnits[i]].range - (rangeUnits[i] - attackerIndex) >= 1 && attackers[rangeUnits[i]].health > 0)
                {
                    
                    float damage = attackers[rangeUnits[i]].attackDamage*finalDamage;

                    if (!isTeam)
                    {
                        //팔랑크스가 발동 되었을 때 ( 데미지 50%감소)
                        if (RelicManager.PhalanxTacticsBook())
                        {
                            damage *= 0.5f;
                        }
                    }

                    //연발
                    for (int j = 0; j<2; j++)
                    {
                        if(j == 1)
                        {
                            Debug.Log("연발");
                            if (!attackers[rangeUnits[i]].doubleShot) break;
                        }

                        if (!CalculateAccuracy(defenders[defenderIndex], attackers[rangeUnits[i]].perfectAccuracy))    //상대 회피 실패
                        {
                            //공격받는 유닛(상대) 중갑 유무
                            if (defenders[defenderIndex].heavyArmor)
                            {
                                if (attackers[rangeUnits[i]].attackDamage > heavyArmorValue)
                                {
                                    damage -= heavyArmorValue;
                                    //통계 기록: 중갑 발동
                                    unitStats[defenders[defenderIndex].UniqueId].IncrementTraitUsage("중갑");
                                }
                            }
                            allDamage += damage;

                            //작열
                            CalculateBurning(attackers[rangeUnits[i]], ref defenders, defenderIndex);

                            //가시
                            if (defenders[defenderIndex].thorns)
                            {
                                Debug.Log("가시");
                                attackers[attackerIndex].health -= thornsDamageValue;
                            }

                            //통계 기록: 입힌 피해, 원거리 피해
                            unitStats[attackers[rangeUnits[i]].UniqueId].AddDamageDealt(damage);
                            unitStats[attackers[rangeUnits[i]].UniqueId].AddRangedDamageDealt(damage);
                        }
                        else
                        {
                            //통계 기록: 회피, 회피한 피해
                            unitStats[defenders[defenderIndex].UniqueId].IncrementDodges();
                            unitStats[defenders[defenderIndex].UniqueId].AddDodgedDamage(damage);
                        }
                    }

                }
            }
        }
        return allDamage;
    }


    //사망 처리
    private void ProcessDeath(ref List<RogueUnitDataBase> attackers,ref List<RogueUnitDataBase> defenders,int attackerIndex,int defenderIndex,float backUnitHp,int deathUnitIndex,ref int unitAdd,bool isTeam ,ref bool isGuerrilla)
    {
        if (defenders[defenderIndex].health <=0 || backUnitHp <= 0)
        {
            //봉인 풀린 자
            if (isTeam) 
            {
                CalculateUnSealHero(ref defenders, myHeroUnits);
            }
            else
            {
                CalculateUnSealHero(ref defenders,enemyHeroUnits);
            }


            //통계 기록: 처치
            unitStats[attackers[attackerIndex].UniqueId].IncrementKills();

            //유격
            if (attackers[attackerIndex].guerrilla && attackers[attackerIndex].health > 0 && attackerIndex+1 < attackers.Count)
            {
                CallDamageText(0, "유격 ", !isTeam);

                //통계 기록: 유격 발동
                unitStats[attackers[attackerIndex].UniqueId].IncrementSkillUsage("유격");

                (attackers[attackerIndex + 1], attackers[attackerIndex]) = (attackers[attackerIndex],attackers[attackerIndex+1]);
                
                isGuerrilla = true;
            }
            //착취
            if (attackers[attackerIndex].drain && attackers[attackerIndex].health > 0)
            {
                //Debug.Log("착취");

                //상흔 확인
                float heal = HealHealth(attackers[attackerIndex], drainHealValue);
                attackers[attackerIndex].health = MathF.Min(attackers[attackerIndex].maxHealth, heal + attackers[attackerIndex].health);
                attackers[attackerIndex].attackDamage += drainGainAttackValue;

                CallDamageText(-heal, "착취 ", !isTeam);
                //통계 기록: 착취 발동
                unitStats[attackers[attackerIndex].UniqueId].IncrementSkillUsage("착취");
            }
            if (defenders[defenderIndex].health <= 0)
            {
                defenders[defenderIndex].alive=false;
                //무한
                /*
                if (attackers[attackerIndex].infinity)
                {
                    attackers[attackerIndex].energy++;
                }*/

                //순교
                CalculateMartyrdom(ref defenders,defenderIndex);

            }
            if (backUnitHp <= 0)
            {
                defenders[deathUnitIndex].alive=false;

                //순교
                CalculateMartyrdom(ref defenders, deathUnitIndex);

            }

            ChangeInvisibile(!isTeam, deathUnitIndex);
            unitAdd++;
        }

    } 

    //점수 관리
    private void ManageScore(int result)
    {
        float score = 0;

        //상대 사망 유닛 계산
        foreach (RogueUnitDataBase enemy in enemyUnits)
        {
            if(enemy.health <= 0)
            {
                score += enemy.unitPrice;
            }
        }
        if(result ==0)
        {
            //아군 생존 유닛
            foreach (RogueUnitDataBase my in myUnits)
            {
                if (my.health > 0)
                {
                    float healthRatio = my.health / my.maxHealth;
                    score += (healthRatio * my.unitPrice) * 0.5f;
                }
            }

            //남은 금액 덧셈
            score += PlayerData.currency;
        }

        autoBattleUI.UpdateScore((int)score);
    }

    // 유닛 별 고유 ID 생성
    private int GenerateUniqueUnitId(int branchIdx, bool isTeam, int unitIdx)
    {
        int teamBit = isTeam ? 0 : 1; // 팀 정보
        return (teamBit << 31)             // 팀 비트 (31번째 비트)
             | (branchIdx << 21)          // 병종 정보 (21~30번째 비트)
             | ((globalUnitId++ & 0x7FF) << 10) // 글로벌 ID (10~20번째 비트, 11비트 제한)
             | (unitIdx & 0x3FF);         // 유닛 고유 번호 (0~9번째 비트, 10비트 제한)
    }

    //전투 전 발동
    private void ProcessBeforeBattle(ref List<RogueUnitDataBase> units,bool isTeam)
    {
        CheckHeroUnit(units,isTeam);

        CalculateCorpsCommander(units, isTeam);
        CalculataeSolidarity(ref units, isTeam);

    }

    //군단장
    private void CalculateCorpsCommander(List<RogueUnitDataBase> units,bool isTeam)
    {
        if (isTeam && myHeroUnits.ContainsKey(52))
        {
            mybindingAttackDamage += 5;
        }
        else if(!isTeam && enemyHeroUnits.ContainsKey(52))
        {
            enemybindingAttackDamage += 5;
        }
        return;

    }
    //영웅 유닛 채크
    private void CheckHeroUnit(List<RogueUnitDataBase> units, bool isTeam)
    {
        foreach (RogueUnitDataBase unit in units)
        {
            if (unit.branchIdx == 8)
            {
                if (isTeam)
                {
                    myHeroUnits.Add(unit.idx, true);
                }
                else
                {
                    enemyHeroUnits.Add(unit.idx, true);
                }
            }
        }
    }

    //봉인 풀린 자
    private void CalculateUnSealHero(ref List<RogueUnitDataBase> units,Dictionary<int,bool> heroUnits)
    {
        if(heroUnits.ContainsKey(53))
        {
            foreach(RogueUnitDataBase unit in units)
            {
                if(unit.idx==53 && unit.health > 0)
                {
                    unit.maxHealth += 10;
                    unit.health += 10;
                    unit.attackDamage += 5;
                }
            }
        }
        
    }

    //결속 발동
    private void CalculataeSolidarity(ref List<RogueUnitDataBase> units,bool isTeam)
    {
        foreach (var unit in units)
        {
            if (unit.bindingForce) // 결속이 활성화된 유닛만 체크
            {
                foreach (var checkUnit in units)
                {
                    // checkUnit.unitTag와 unit.unitTag에 공통된 태그가 하나라도 있으면 공격력 +5
                    if (unit.tag==checkUnit.tag)
                    {
                        unit.attackDamage += isTeam?mybindingAttackDamage:enemybindingAttackDamage;
                    }
                }
            }
        }
    }

    //작열 발동
    private void CalculateBurning(RogueUnitDataBase attacker, ref List<RogueUnitDataBase> defenders,int defenderIndex)
    {
        if (attacker.scorching)
        {
            int burningId = 0;
            int type = 1;
            int rank = 1;
            int duration = 2;
            
            if (defenders[defenderIndex].effectDictionary.TryGetValue(burningId, out BuffDebuffData effect))
            {
                effect.EffectGrade += 1;
                effect.Duration += 1;
            }
            else
            {
                defenders[defenderIndex].effectDictionary[burningId] = new BuffDebuffData(burningId, type,  rank, duration);
            }
        }
    }

    //작열 데미지
    private void DamageBurning(ref List<RogueUnitDataBase> defenders, int defenderIndex,bool isTeam)
    {
        int burningId = 0;

        // 효과가 존재하지 않거나 지속시간이 0 이면 종료
        if (!defenders[defenderIndex].effectDictionary.TryGetValue(burningId, out BuffDebuffData burningEffect) || burningEffect.Duration == 0)
            return;

        // 지속 시간과 등급 제한 적용
        burningEffect.Duration = Mathf.Min(burningEffect.Duration, 2);
        burningEffect.EffectGrade = Mathf.Min(burningEffect.EffectGrade, 3);

        // 피해량 계산 (최대 fireDamageValue * 3 제한)
        float damage = Mathf.Min(burningEffect.EffectGrade * fireDamageValue, fireDamageValue * 3);

        // 체력 감소 적용
        defenders[defenderIndex].health -= damage;

        // 지속시간 감소
        burningEffect.Duration--;

        Debug.Log("작열");

        CallDamageText(damage, "작열 ", isTeam);
    }

    // 치유(Healing) 기능 - 후열 유닛이 전열 유닛을 치유
    private void ProcessCure(ref List<RogueUnitDataBase> attackers, int attackerIndex,bool isTeam)
    {
        // 치유받을 전열 유닛 (현재 attackerIndex 위치의 유닛)
        RogueUnitDataBase frontUnit = attackers[attackerIndex];
        float heal = 0;

        // 후열 유닛 탐색 (attackerIndex 이후의 유닛)
        for (int i = attackerIndex + 1; i < attackers.Count; i++)
        {
            RogueUnitDataBase healer = attackers[i];

            // 치유 특성이 없거나 사거리가 2 미만이면 스킵
            if (!healer.healing || healer.range < 2)
                continue;

            // 후열에서의 순서 계산 (현재 위치 차이)
            int rearPosition = i - attackerIndex;

            // 치유 조건: (사거리) - (후열에서의 순서) >= 1 확인
            if (healer.range - rearPosition >= 1 && frontUnit.health > 0)
            {
                // 치유량 = 치유하는 유닛의 공격력
                float healAmount = healer.attackDamage;
                //상흔 확인
                healAmount =HealHealth(frontUnit, healAmount);

                heal += healAmount;
                // 최대 체력을 넘지 않도록 제한
                frontUnit.health = Mathf.Min(frontUnit.maxHealth, frontUnit.health + healAmount);

                Debug.Log($"{healer.unitName}가 {frontUnit.unitName}을(를) {healAmount}만큼 치유함!");
            }
        }
        if(heal > 0)
        {
            CallDamageText(-heal, "회복 ", isTeam);
        }
    }

    //체력 회복
    private float HealHealth(RogueUnitDataBase unit,float healValue)
    {
        if (unit.effectDictionary[1] != null) return 0;  
        return healValue;
    }

    //순교
    private void CalculateMartyrdom(ref List<RogueUnitDataBase> defenders,int defenderIndex)
    {
        if (defenders[defenderIndex].martyrdom)
        {
            for (int i = defenderIndex + 1; i < defenders.Count; i++)
            {
                if (defenders[i].alive)
                {
                    Debug.Log("순교");
                    defenders[i].attackDamage = Mathf.Round(defenders[i].attackDamage * martyrdomValue);
                    break;
                }
            }
        }
    }

    //선제 타격
    private int CalculateFirstStrike(ref List<RogueUnitDataBase> attakers,int attakerIndex,ref List<RogueUnitDataBase> defenders,int defenderIndex,bool isTeam)
    {
        if (attakers[attakerIndex].firstStrike && !attakers[attakerIndex].fStriked && CheckAssassination(defenders,defenderIndex))
        {
            int minHealthIndex;
            //상대 후열 유닛 체력 저장
            List<float> defendersHealth = new();
            for (int i = enemyUnitIndex + 1; i < enemyUnitMax; i++)
            {
                defendersHealth.Add(defenders[i].health);
            }
            minHealthIndex = defenderIndex + CalculateAssassination(defendersHealth);

            float damage = attakers[attakerIndex].attackDamage * 2;

            defenders[minHealthIndex].health -= damage;

            int deadUnitCount = 0;
            for (int i = defenderIndex + 1; i < minHealthIndex; i++)
            {
                if (defenders[i].health <= 0)
                {
                    deadUnitCount++;
                }
            }
            minHealthIndex = minHealthIndex - defenderIndex - deadUnitCount;
            //후열 유닛 사망시 대입
            if (defenders[minHealthIndex].health <= 0)
            {
                return minHealthIndex;
            }

            CallDamageText(damage, "선제타격", isTeam, minHealthIndex);
            attakers[attakerIndex].fStriked = true;
            Debug.Log("선제 타격" + attakers[attakerIndex].attackDamage * 2);
        }
        return 0;
    }

    //도전
    private void CalculateChallenge(RogueUnitDataBase attaker,ref List<RogueUnitDataBase> defenders,int defenderIndex)
    {
        if(attaker.challenge && CheckAssassination(defenders, defenderIndex))
        {
            int minHealthIndex;
            //상대 후열 유닛 체력 저장
            List<float> defendersHealth = new();
            for (int i = enemyUnitIndex + 1; i < enemyUnitMax; i++)
            {
                defendersHealth.Add(defenders[i].health);
            }

            minHealthIndex = defenderIndex + CalculateAssassination(defendersHealth);

            RogueUnitDataBase selectUnit= defenders[minHealthIndex];
            defenders.RemoveAt(minHealthIndex);
            defenders.Insert(0, selectUnit);
            Debug.Log("도전");
        }
    }

    //연막
    private void CalculateSmokeScreen(ref List<RogueUnitDataBase> unit,int index)
    {
        if (unit[index].smokeScreen && CheckAssassination(unit, index))
        {
            int id = 2;
            int type = 0;
            int rank = 1;
            int duration = -1;

            unit[index+1].effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);

            Debug.Log("연막");
        }
    }
        
    //유산 호출 및 초기화
    private void ProcessRelic()
    {
        //배율 초기화
        RogueLikeData.Instance.ResetFinalDamage();

        ownedRelics=RelicManager.GetRelicData();

        // 아군, 적군 데이터를 RogueLikeData 싱글톤에 저장
        RogueLikeData.Instance.AllMyUnits(myUnits);
        RogueLikeData.Instance.AllEnemyUnits(enemyUnits);

        //스탯 유산 적용
        RelicManager.RunStateRelic();

        myFinalDamage = RogueLikeData.Instance.GetMyMultipleDamage();
        enemyFinalDamage = RogueLikeData.Instance.GetEnemyMultipleDamage();

        myUnits = RogueLikeData.Instance.GetMyUnits();

        //유산 이미지 생성
        autoBattleUI.CreateWarRelic();
    }
    

    
}


