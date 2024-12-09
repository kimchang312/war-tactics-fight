using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBattleUnit : MonoBehaviour
{
    // 기본 정보
    public int idx;            // 유닛의 고유 인덱스
    public string unitName;    // 유닛 이름
    public string unitBranch;  // 병종 이름
    public int branchIdx;      // 병종 인덱스
    public int unitId;         // 유닛 ID
    public string unitExplain; // 유닛 설명

    public string unitImg;        // 유닛 이미지 ID

    public string unitFaction; // 유닛이 속한 진영
    public int factionIdx;     // 진영 인덱스
    public int unitPrice;      // 유닛 가격

    // 스탯 정보
    public float maxHealth;    // 유닛 최대 체력
    public float health;       // 유닛 체력
    public float armor;        // 유닛 장갑
    public float attackDamage; // 공격력
    public float mobility;     // 기동성
    public float range;        // 사거리
    public float antiCavalry;  // 대기병 능력

    // 특성 및 기술
    public bool lightArmor;     // 경갑 유무
    public bool heavyArmor;     // 중갑 유무
    public bool rangedAttack;   // 원거리 공격 유무
    public bool bluntWeapon;    // 둔기 유무
    public bool pierce;         // 관통 유무
    public bool agility;        // 날쌤 유무
    public bool strongCharge;   // 강한 돌격 유무
    public bool perfectAccuracy;// 필중 유무


    public string blink = "빈";           //빈칸


    // 추가적인 능력치
    public bool charge;         // 돌격
    public bool defense;        // 방어
    public bool throwSpear;     // 창 던지기
    public bool slaughter;      // 학살
    public bool guerrilla;      // 게릴라
    public bool guard;          // 경호
    public bool assassination;  // 암살
    public bool drain;          // 흡수
    public bool overwhelm;      // 압도


}
