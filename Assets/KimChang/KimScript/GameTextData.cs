using System.Collections.Generic;

public class GameTextData
{
    // 데이터 맵
    private static readonly Dictionary<int, (string[] Names, string[] Descriptions)> dataMap = new Dictionary<int, (string[], string[])>
    {
        // UI 텍스트
        { 0, (new[] { "WTF", "WTF", "WTF" }, new string[0]) },
        { 1, (new[] { "WAR TACTICS FIGHT", "WAR TACTICS FIGHT", "WAR TACTICS FIGHT" }, new string[0]) },
        { 2, (new[] { "게임 시작", "GAME START", "ゲーム開始" }, new string[0]) },
        { 3, (new[] { "게임 설정", "GAME SETTING", "設定" }, new string[0]) },
        { 4, (new[] { "게임 종료", "GAME QUIT", "ゲーム終了" }, new string[0]) },
        { 5, (new[] { "설정", "Setting", "設定" }, new string[0]) },
        { 6, (new[] { "언어 변경", "Language", "言語" }, new string[0]) },
        { 7, (new[] { "효과음", "Sound Effect", "効果音" }, new string[0]) },
        { 8, (new[] { "배경음", "BGM", "背景音" }, new string[0]) },
        { 9, (new[] { "켜기", "ON", "オン" }, new string[0]) },
        { 10, (new[] { "끄기", "OFF", "オフ" }, new string[0]) },


        // 버튼
        { 11, (new[] { "한국어", "한국어", "한국어" }, new string[0]) },
        { 12, (new[] { "English", "English", "English" }, new string[0]) },
        { 13, (new[] { "日本語", "日本語", "日本語" }, new string[0]) },
        { 15, (new[] { "예", "Yes", "はい" }, new string[0]) },
        { 16, (new[] { "아니오", "No", "いいえ" }, new string[0]) },
        { 18, (new[] { "쉬움", "Easy", "簡単" }, new string[0]) },
        { 19, (new[] { "보통", "Normal", "普通" }, new string[0]) },
        { 20, (new[] { "어려움", "Hard", "難しい" }, new string[0]) },
        { 21, (new[] { "도전", "Challenge", "チャレンジ" }, new string[0]) },

        // UI 텍스트와 기타 항목
        { 14, (new[] { "게임을 종료하시겠습니까?", "Would you like to exit the game?", "ゲームを終了しますか？" }, new string[0]) },
        { 17, (new[] { "난이도", "Difficulty", "難易度" }, new string[0]) },
        { 22, (new[] { "제국", "Empire", "帝国" }, new string[0]) },
        { 23, (new[] { "신성국", "Divinitas", "神聖国" }, new string[0]) },
        { 24, (new[] { "칠왕연합", "Heptarchy", "七王連合" }, new string[0]) },
        { 25, (new[] { "공용", "Common", "共通" }, new string[0]) },
        { 26, (new[] { "구매 단계", "Purchase Phase", "購入段階" }, new string[0]) },
        { 27, (new[] { "진영: ", "Faction: ", "陣営" }, new string[0]) },
        { 28, (new[] { "G", "G", "G" }, new string[0]) },
        { 29, (new[] { "배치 단계로", "To Placement Phase", "配置段階へ" }, new string[0]) },
        { 30, (new[] { "유닛 상세", "Unit Details", "ユニット詳細" }, new string[0]) },
        { 31, (new[] { "적 유닛 정보", "Enemy Unit Information", "敵ユニット情報" }, new string[0]) },
        { 32, (new[] { "병종", "Unit Type", "兵種" }, new string[0]) },
        { 33, (new[] { "특성", "Traits", "特性" }, new string[0]) },
        { 34, (new[] { "기술", "Skills", "スキル" }, new string[0]) },
        { 35, (new[] { "계속하기", "Continue", "続ける" }, new string[0]) },
        { 36, (new[] { "포기", "Surrender", "降参" }, new string[0]) },

        // 유닛 툴팁 (70~95)
        { 70, (new[] { "징집된 비전문 창병입니다. 기병 돌격을 방어하는데 특화되어 있습니다. 지속적인 근접 전투와 원거리 공격에 취약합니다.", "A conscripted, non-professional spearman. Specializes in defending against cavalry charges. Vulnerable to sustained melee combat and ranged attacks.", "徴兵された非専門の槍兵です。騎兵の突撃を防御することに特化しています。持続的な近接戦闘や遠距離攻撃に弱いです。" }, new string[0]) },
        { 71, (new[] { "기병에 매우 강력한 창병입니다. 지속적인 근접 전투와 원거리 공격에 취약합니다.", "A powerful spearman with high effectiveness against cavalry. Vulnerable to sustained melee combat and ranged attacks.", "騎兵に非常に強力な槍兵です。持続的な近接戦闘や遠距離攻撃に弱いです。" }, new string[0]) },
        { 72, (new[] { "도를 들고 싸우는 전사입니다. 근접 전투 능력이 뛰어나지만 원거리 공격과 기병에 취약합니다.", "A warrior wielding a mace. Excels in melee combat but is vulnerable to ranged attacks and cavalry.", "刀を持って戦う戦士です。近接戦闘に優れていますが、遠距離攻撃や騎兵に弱いです。" }, new string[0]) },
        { 73, (new[] { "징집된 비전문 궁병입니다. 약한 원거리 공격을 가하며 근접 전투에 매우 취약합니다.", "A conscripted, non-professional bowman. Deals weak ranged attacks and is highly vulnerable in close combat.", "徴兵された非専門の弓兵です。弱い遠距離攻撃を行い、近接戦闘に非常に弱いです。" }, new string[0]) },
        { 74, (new[] { "적 처치 시 재빠르게 도망할 수 있는 궁병이다. 약한 원거리 공격을 가하며 근접 전투에 매우 취약하다.", "A bowman who can quickly escape after defeating an enemy. Delivers weak ranged attacks and is highly vulnerable in close combat.", "敵を倒した際に素早く逃げられる弓兵です。弱い遠距離攻撃を行い、近接戦闘に非常に弱いです。" }, new string[0]) },
        { 75, (new[] { "방패와 검을 장비한 중보병이다. 공격력이 낮지만 막대한 원거리 피해를 막아낼 수 있다. 후열의 유닛들을 보호할 수 있다.", "A heavy infantry unit equipped with a shield and sword. Has low attack power but can absorb significant ranged damage, protecting rear-line units.", "盾と剣を装備した重歩兵です。攻撃力は低いですが、大量の遠距離ダメージを防ぐことができ、後衛のユニットを守ることができます。" }, new string[0]) },
        { 76, (new[] { "중갑에 추가 피해를 입히는 철퇴를 장비한 중보병이다. 중갑의 적을 상대로 강력하다.", "A heavy infantry unit wielding a mace that deals additional damage to heavily armored enemies. Strong against armored foes.", "重装甲の敵に追加ダメージを与えるメイスを装備した重歩兵です。重装甲の敵に対して強力です。" }, new string[0]) },
        { 77, (new[] { "후열의 적에게 막대한 피해를 입히는 암살 유닛이다. 회피율이 높지만 체력과 장갑이 낮아 적의 공격에 취약하다.", "An assassin unit that deals massive damage to backline enemies. Has a high evasion rate but is vulnerable to enemy attacks due to low health and armor.", "後衛の敵に大きなダメージを与える暗殺ユニットです。回避率が高いですが、体力と装甲が低いため、敵の攻撃に弱いです。" }, new string[0]) },
        { 78, (new[] { "매우 강력한 기동력을 바탕으로 돌격하는 유닛이다. 기병 상대 보너스가 있으며 지속 전투와 창병에 취약하다.", "A unit that charges with great mobility. Has a bonus against cavalry but is vulnerable to sustained combat and spearmen.", "非常に高い機動力を基に突撃するユニットです。騎兵に対するボーナスがあり、持続戦闘と槍兵に弱いです。" }, new string[0]) },
        { 79, (new[] { "매우 강력한 기동력을 바탕으로 적을 습격하는 원거리 유닛이다. 창병과 지속 전투에 취약하다.", "A ranged unit that strikes enemies with high mobility. Vulnerable to spearmen and sustained combat.", "非常に高い機動力を基に敵を急襲する遠距離ユニットです。槍兵や持続戦闘に弱いです。" }, new string[0]) },
        { 80, (new[] { "중갑과 창으로 무장한 기병이다. 기병 상대 보너스가 있으며 장갑을 무시한다. 창병에 취약하다.", "A cavalry unit armored with heavy armor and a spear. Has a bonus against cavalry and ignores armor but is vulnerable to spearmen.", "重装甲と槍で武装した騎兵です。騎兵に対するボーナスがあり、装甲を無視しますが、槍兵に弱いです。" }, new string[0]) },
        { 81, (new[] { "투창으로 전투를 개시하는 창병이다. 기병 방어보다는 근접 전투에 더 강력하다.", "A spearman that opens combat with a javelin throw. Stronger in melee combat than in defending against cavalry.", "投槍で戦闘を開始する槍兵です。騎兵防御よりも近接戦闘に優れています。" }, new string[0]) },
        { 82, (new[] { "도끼를 장비한 전사다. 경갑 적을 상대로 근접전이 매우 강력하다. 원거리 공격에 매우 취약하다.", "A warrior equipped with two axes. Very powerful in close combat against lightly armored enemies but highly vulnerable to ranged attacks.", "斧を両手に装備した戦士です。軽装甲の敵に対する近接戦闘が非常に強力で、遠距離攻撃には非常に弱いです。" }, new string[0]) },
        { 83, (new[] { "곤봉으로 적의 중갑에 추가 피해를 입히는 중보병이다.", "A heavy infantry unit that deals additional damage to heavily armored enemies with a club.", "棍棒で敵の重装甲に追加ダメージを与える重歩兵です。" }, new string[0]) },
        { 84, (new[] { "적을 처치할 수록 강해지는 돌격 특화 기병 유닛이다. 지속 전투와 창병에 매우 취약하다.", "A cavalry unit specialized in charging that grows stronger with each enemy it defeats. Highly vulnerable to sustained combat and spearmen.", "敵を倒すごとに強くなる突撃特化の騎兵ユニットです。持続戦闘や槍兵に非常に弱いです。" }, new string[0]) },
        { 85, (new[] { "드레이크에 탑승한 기병이다. 적의 기동력을 약화시키며 기병과 중갑에 매우 강력하다.", "A cavalry unit mounted on a drake. Weakens enemy mobility and is highly effective against cavalry and heavy armor.", "ドレイクに乗った騎兵です。敵の機動力を弱体化させ、騎兵や重装甲に非常に強力です。" }, new string[0]) },
        { 86, (new[] { "중갑으로 무장한 최정예 창병이다. 기병 상대와 원거리 공격 방어에 뛰어나다.", "An elite spearman clad in heavy armor. Excels at countering cavalry and defending against ranged attacks.", "重装甲で武装した最精鋭の槍兵です。騎兵への対抗と遠距離攻撃の防御に優れています。" }, new string[0]) },
        { 87, (new[] { "검으로 돌격하는 전사다. 높은 체력과 공격력을 지녔지만 원거리 공격에 취약하다.", "A warrior charging with a sword. Possesses high health and attack power but is vulnerable to ranged attacks.", "剣で突撃する戦士です。高い体力と攻撃力を持っていますが、遠距離攻撃に弱いです。" }, new string[0]) },
        { 88, (new[] { "강력한 원거리 공격을 하는 궁병이다. 근접 전투에 매우 취약하다.", "A bowman with a powerful ranged attack. Highly vulnerable in close combat.", "強力な遠距離攻撃を行う弓兵です。近接戦闘に非常に弱いです。" }, new string[0]) },
        { 89, (new[] { "후열의 적에게 막대한 피해를 입히는 암살 유닛이다. 적을 연속적으로 암살할 수 있다.", "An assassin unit that deals massive damage to backline enemies, capable of executing enemies consecutively.", "後衛の敵に大きなダメージを与える暗殺ユニットです。連続して敵を暗殺することができます。" }, new string[0]) },
        { 90, (new[] { "중갑과 검으로 무장한 기병이다. 장갑을 무시하고 창병에 취약하다.", "A cavalry unit equipped with heavy armor and a sword. Ignores enemy armor but is vulnerable to spearmen.", "重装甲と剣で武装した騎兵です。敵の装甲を無視し、槍兵に弱いです。" }, new string[0]) },
        { 91, (new[] { "장창으로 무장한 잘 훈련된 창병이다. 기병 방어와 근접 전투 모두 강력하다.", "A well-trained spearman armed with a pike. Strong in both cavalry defense and melee combat.", "パイクで武装した訓練された槍兵です。騎兵防御と近接戦闘の両方に強力です。" }, new string[0]) },
        { 92, (new[] { "양손 검을 장비한 전사다. 지속 전투에 매우 강하다.", "A warrior wielding a two-handed sword. Extremely strong in sustained combat.", "両手剣を装備した戦士です。持続戦闘に非常に強いです。" }, new string[0]) },
        { 93, (new[] { "사거리가 긴 원거리 공격을 하는 궁병이다. 근접 전투에 매우 취약하다.", "A bowman with long-range attacks. Highly vulnerable in close combat.", "長い射程を持つ遠距離攻撃を行う弓兵です。近接戦闘に非常に弱いです。" }, new string[0]) },
        { 94, (new[] { "월도로 무장한 강력한 중보병이다. 후열의 아군을 보호한다.", "A powerful heavy infantry unit armed with a halberd. Protects allied units in the backline.", "月刀で武装した強力な重歩兵です。後衛の味方を保護します。" }, new string[0]) },
        { 95, (new[] { "중갑과 전투망치로 무장한 기병이다. 중갑에 특히 강력하다.", "A cavalry unit equipped with heavy armor and a war hammer. Especially strong against heavily armored enemies.", "重装甲と戦闘ハンマーで武装した騎兵です。特に重装甲に対して強力です。" }, new string[0]) },

        // 스탯
        { 103, (new[] { "체력", "Health", "体力" }, new string[0]) },
        { 104, (new[] { "장갑", "Armor", "装甲" }, new string[0]) },
        { 105, (new[] { "공격력", "Attack Damage", "攻撃力" }, new string[0]) },
        { 106, (new[] { "사거리", "Range", "射程" }, new string[0]) },
        { 107, (new[] { "대기병", "Anti Calvary", "対騎兵" }, new string[0]) },
        { 108, (new[] { "회피", "Dodge", "回避" }, new string[0]) },

        // 특성
        { 109, (new[] { "경갑", "Light Armor", "軽装甲" }, new[] { "낮거나 보통 수준의 장갑입니다.", "Low or average level armor.", "低または平均レベルの装甲です。" }) },
        { 110, (new[] { "중갑", "Heavy Armor", "重装甲" }, new[] { "높은 수준의 장갑입니다. 받는 지원 사격 피해가 감소합니다.","High level armor. Reduces damage taken from supporting fire.","高レベルの装甲です。受ける支援射撃のダメージが減少します。" }) },
        { 111, (new[] { "원거리 공격", "Ranged Attack", "遠距離攻撃" }, new[] { "지원 사격을 할 수 있습니다.", "Can perform supporting fire.", "支援射撃を行うことができます。" }) },
        { 112, (new[] { "둔기", "Blunt Weapon", "鈍器" }, new[] { "중갑 상대에게 추가 피해를 입힙니다.", "Deals additional damage to heavy armored opponents.", "重装甲の相手に追加ダメージを与えます。" }) },
        { 113, (new[] { "관통", "Pierce", "貫通" }, new[] { "상대의 장갑을 무시하고 공격합니다.", "Attacks ignore the opponent's armor.", "相手の装甲を無視して攻撃します。" }) },
        { 114, (new[] { "날쌤", "Agility", "敏捷" }, new[] { "회피율이 증가합니다.", "Increases dodge rate.", "回避率が増加します。" }) },
        { 115, (new[] { "강한 돌격", "Strong Charge", "猛突撃" }, new[] { "돌격이 더욱 강해집니다.", "Charge becomes stronger.", "突撃がさらに強力になります。" }) },
        { 116, (new[]  {"필중","Perfect Accuracy", "必中" }, new[]{"상대의 회피를 무시하고 공격합니다.", "Attacks ignore the opponent's evasion.", "相手の回避を無視して攻撃します。"}) },


        // 기술
        { 125, (new[] { "돌격", "Charge", "突撃" },  new[] { "첫 공격이 기동력에 비례해 큰 피해를 입힙니다.",
                       "The first attack deals significant damage proportional to mobility.",
                       "最初の攻撃が機動力に比例して大きなダメージを与えます。" }) },
        { 126, (new[] { "수비 태세", "Defensive Stance", "防御態勢" }, new[] { "첫 공격이 돌격 중인 상대에게 강해집니다. 그렇지 않다면 약해집니다.",
                       "The first attack is stronger against charging opponents; otherwise, it is weaker.",
                       "最初の攻撃が突撃中の相手に対して強力になります。それ以外の場合は弱くなります。" }) },
        { 127, (new[] { "투창", "Javelin Throw", "投槍" }, new[] { "첫 공격 전에 적에게 창을 던져 피해를 입힙니다.",
                       "Throws a spear at the enemy, dealing damage before the first attack.",
                       "最初の攻撃前に敵に槍を投げてダメージを与えます。" }) },
        { 128, (new[] { "도살", "Slaugther", "屠殺" }, new[] { "경갑 상대에게 추가 피해를 입힙니다.",
                       "Deals additional damage to lightly armored opponents.",
                       "軽装甲の相手に追加ダメージを与えます。" }) },
        { 129, (new[] { "유격", "Skirmish", "遊撃" }, new[] { "적을 처치하면 후열로 이동합니다.",
                       "Moves to the backline after defeating an enemy.",
                       "敵を倒すと後列に移動します。" }) },
        { 130, (new[] { "수호", "Guard", "守護" }, new[] { "후열 유닛이 받는 피해를 대신 입습니다.",
                       "Takes damage on behalf of backline units.",
                       "後列のユニットが受けるダメージを代わりに受けます。" }) },
        { 131, (new[] { "암살", "Assassination", "暗殺" }, new[] { "상대 후열 유닛에게 피해를 입힙니다.",
                       "Deals damage to the opponent's backline units.",
                       "相手の後列ユニットにダメージを与えます。" }) },
        { 132, (new[] { "착취", "Drain", "搾取" }, new[] { "상대 유닛을 처치하면 체력과 공격력을 얻습니다.",
                       "Gains health and attack power upon defeating an enemy unit.",
                       "敵ユニットを倒すと体力と攻撃力を得ます。" }) },
        { 133, (new[] { "위압", "Overwhelm", "威圧" }, new[] { "첫 공격 전에 적의 기동력을 크게 낮춥니다.",
                       "Greatly reduces the enemy’s mobility before the first attack.",
                       "最初の攻撃前に敵の機動力を大幅に低下させます。" }) },

    };

    // 역 매핑 데이터 (정규화된 문자열 -> idx)
    private static readonly Dictionary<string, int> reverseMap = BuildReverseMap();

    // 정규화 함수: 공백 제거, 소문자로 변환
    private static string Normalize(string input) => input?.Trim().ToLower().Replace(" ", "");

    // 역 매핑 데이터 생성
    private static Dictionary<string, int> BuildReverseMap()
    {
        var map = new Dictionary<string, int>();
        foreach (var kvp in dataMap)
        {
            int idx = kvp.Key;
            foreach (var name in kvp.Value.Names)
            {
                string normalized = Normalize(name);
                if (!string.IsNullOrEmpty(normalized) && !map.ContainsKey(normalized))
                {
                    map[normalized] = idx;
                }
            }
        }
        return map;
    }

    // idx로 데이터 가져오기
    public static (string Name, string Description) GetLocalizedText(int idx)
    {
        int language = LanguageManager.GetLanguage();
        if (dataMap.TryGetValue(idx, out var values))
        {
            string name = language >= 0 && language < values.Names.Length ? values.Names[language] : "Invalid language index.";
            string description = language >= 0 && language < values.Descriptions.Length ? values.Descriptions[language] : "No description available.";
            return (name, description);
        }
        return ("Invalid index.", "Invalid index.");
    }

    // 문자열로 idx 가져오기
    public static int? GetIdxFromString(string input)
    {
        string normalized = Normalize(input);
        if (reverseMap.TryGetValue(normalized, out int idx))
        {
            return idx;
        }
        return null; // 매칭되는 idx가 없는 경우
    }

}

