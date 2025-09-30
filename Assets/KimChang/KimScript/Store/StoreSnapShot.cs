using System.Collections.Generic;
using System;

[Serializable]
public enum StoreSlotType { UnitPackage, Relic, Item, Reroll }

// 유닛 패키지 슬롯 스냅샷 (사용처: 상점 UI 구성, 구매/잠금 처리)
[Serializable]
public class UnitPackageOffer
{
    public int[] unitIdxs;     // ConvertToUnitDataBase에 넣을 idx들
    public int price;        // 최종가
    public bool sold;         // 구매됨 여부
}

// 유물/소모품/리롤 공용 슬롯 스냅샷 (사용처: 상점 UI 구성, 구매/잠금 처리)
[Serializable]
public class SimpleOffer
{
    public int id;             // relicId 또는 itemId
    public int price;          // 최종가
    public bool sold;          // 구매됨 여부
}

// 상점 전체 스냅샷 (사용처: 저장/복원, 재진입 시 UI 바인딩)
[Serializable]
public class StoreSnapshot
{
    public int chapter;        // 현재 챕터(있다면)
    public int stageX;         // 상점 좌표
    public int stageY;
    public StageType stageType; // 반드시 Shop(또는 Store)로 세팅

    public List<UnitPackageOffer> unitPacks = new();
    public List<SimpleOffer> relics = new();
    public List<SimpleOffer> items = new();
    public SimpleOffer reroll; // 상점의 리롤 아이템 칸(게임 리롤 통화 +n 같은)
}