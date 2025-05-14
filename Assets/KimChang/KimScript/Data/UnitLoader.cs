using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class UnitLoader
{
    // 캐시를 위한 딕셔너리 (idx → RogueUnitDataBase)
    private static Dictionary<int, RogueUnitDataBase> unitCache;

    private static UnitLoader instance;
    public static UnitLoader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UnitLoader();
            }
            return instance;
        }
    }
    private UnitLoader() { }

    // JSON 파일에서 유닛 데이터를 읽고 캐시함
    public void LoadUnitsFromJson()
    {
        // 캐시가 없다면 초기화
        if (unitCache == null)
        {
            unitCache = new Dictionary<int, RogueUnitDataBase>();
        }
        // JSON 파일을 읽어오기
        TextAsset jsonFile = Resources.Load<TextAsset>("JsonData/UnitStatus");
        List<Dictionary<string, object>> unitsData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonFile.text);

        // 각 유닛을 처리하고 캐시에 저장
        foreach (var unitData in unitsData)
        {
            int idx = int.Parse(unitData["idx"].ToString());
            string unitName = unitData["unitName"].ToString();
            string unitBranch = unitData["unitBranch"].ToString();
            int branchIdx = int.Parse(unitData["branchIdx"].ToString());
            string unitId = unitData["unitId"].ToString();
            string unitExplain = unitData["unitExplain"].ToString();
            string unitImg = unitData["unitImg"].ToString();
            string unitFaction = unitData["unitFaction"].ToString();
            int factionIdx = int.Parse(unitData["factionIdx"].ToString());
            string tag = unitData["tag"].ToString();
            int tagIdx = int.Parse(unitData["tagIdx"].ToString());
            int unitPrice = int.Parse(unitData["unitPrice"].ToString());
            int rarity = int.Parse(unitData["rarity"].ToString());

            // 기본 스탯 정보
            float health = float.Parse(unitData["health"].ToString());
            float armor = float.Parse(unitData["armor"].ToString());
            float attackDamage = float.Parse(unitData["attackDamage"].ToString());
            float mobility = float.Parse(unitData["mobility"].ToString());
            float range = float.Parse(unitData["range"].ToString());
            float antiCavalry = float.Parse(unitData["antiCavalry"].ToString());
            int energy = int.Parse(unitData["energy"].ToString());

            // 불리언 특성
            bool lightArmor = bool.Parse(unitData["lightArmor"].ToString());
            bool heavyArmor = bool.Parse(unitData["heavyArmor"].ToString());
            bool rangedAttack = bool.Parse(unitData["rangedAttack"].ToString());
            bool bluntWeapon = bool.Parse(unitData["bluntWeapon"].ToString());
            bool pierce = bool.Parse(unitData["pierce"].ToString());
            bool agility = bool.Parse(unitData["agility"].ToString());
            bool strongCharge = bool.Parse(unitData["strongCharge"].ToString());
            bool perfectAccuracy = bool.Parse(unitData["perfectAccuracy"].ToString());
            bool slaughter = bool.Parse(unitData["slaughter"].ToString());
            bool bindingForce = bool.Parse(unitData["bindingForce"].ToString());
            bool bravery = bool.Parse(unitData["bravery"].ToString());
            bool suppression = bool.Parse(unitData["suppression"].ToString());
            bool plunder = bool.Parse(unitData["plunder"].ToString());
            bool doubleShot = bool.Parse(unitData["doubleShot"].ToString());
            bool scorching = bool.Parse(unitData["scorching"].ToString());
            bool thorns = bool.Parse(unitData["thorns"].ToString());
            bool endless = bool.Parse(unitData["endless"].ToString());
            bool impact = bool.Parse(unitData["impact"].ToString());
            bool healing = bool.Parse(unitData["healing"].ToString());
            bool lifeDrain = bool.Parse(unitData["lifeDrain"].ToString());

            // 추가 특성
            bool charge = bool.Parse(unitData["charge"].ToString());
            bool defense = bool.Parse(unitData["defense"].ToString());
            bool throwSpear = bool.Parse(unitData["throwSpear"].ToString());
            bool guerrilla = bool.Parse(unitData["guerrilla"].ToString());
            bool guard = bool.Parse(unitData["guard"].ToString());
            bool assassination = bool.Parse(unitData["assassination"].ToString());
            bool drain = bool.Parse(unitData["drain"].ToString());
            bool overwhelm = bool.Parse(unitData["overwhelm"].ToString());
            bool martyrdom = bool.Parse(unitData["martyrdom"].ToString());
            bool wounding = bool.Parse(unitData["wounding"].ToString());
            bool vengeance = bool.Parse(unitData["vengeance"].ToString());
            bool counter = bool.Parse(unitData["counter"].ToString());
            bool firstStrike = bool.Parse(unitData["firstStrike"].ToString());
            bool challenge = bool.Parse(unitData["challenge"].ToString());
            bool smokeScreen = bool.Parse(unitData["smokeScreen"].ToString());

            // RogueUnitDataBase 객체 생성
            RogueUnitDataBase newUnit = new RogueUnitDataBase(
                idx, unitName, unitBranch, branchIdx, unitId, unitExplain, unitImg, unitFaction, factionIdx, tag, tagIdx, unitPrice, rarity,
                health, armor, attackDamage, mobility, range, antiCavalry, energy, health, armor, attackDamage, mobility, range, antiCavalry, energy,
                lightArmor, heavyArmor, rangedAttack, bluntWeapon, pierce, agility, strongCharge, perfectAccuracy, slaughter,
                bindingForce, bravery, suppression, plunder, doubleShot, scorching, thorns, endless, impact, healing, lifeDrain,
                charge, defense, throwSpear, guerrilla, guard, assassination, drain, overwhelm,
                martyrdom, wounding, vengeance, counter, firstStrike, challenge, smokeScreen,
                health, energy, true, false, -1, new Dictionary<int, BuffDebuffData>()
            );

            // 유닛을 캐시에 저장
            unitCache[idx] = newUnit;
        }
    }

    // id 기반으로 유닛을 복사하여 반환하는 함수
    public RogueUnitDataBase GetCloneUnitById(int id ,bool isTeam=true)
    {
        if (unitCache != null && unitCache.ContainsKey(id))
        {
            RogueUnitDataBase unit = unitCache[id].Clone();
            unit.UniqueId = RogueUnitDataBase.BuildUnitUniqueId(unit.branchIdx, unit.idx,isTeam);
            return unit;
        }
        return null;
    }
    // 모든 캐시된 유닛을 리스트로 반환
    public List<RogueUnitDataBase> GetAllCachedUnits()
    {
        return unitCache?.Values.ToList() ?? new List<RogueUnitDataBase>();
    }
}
