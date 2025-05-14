using System;
using UnityEngine;

// Resources 폴더 내 JSON 파일로부터 불러올 데이터 클래스
[Serializable]
public class StoreItemData
{
    public int itemId= -1;
    public string itemName;
    public int price;
    public float priceRateMin;
    public float priceRateMax;
    public int rarity;

    public string type;     // Energy, Morale, Unit, Relic 등
    public string form;     // Add, Fixed, Random, Select, All 등
    public string value;    // 수치, 범위, ID 등
    public int count;       // 효과 적용 횟수
    public string condition; // 조건 설명 (ex: "LowEnergyOnly")
    public string description;
    public StoreItemData Clone()
    {
        return new StoreItemData
        {
            itemId = this.itemId,
            itemName = this.itemName,
            price = this.price,
            priceRateMin = this.priceRateMin,
            priceRateMax = this.priceRateMax,
            rarity = this.rarity,
            type = this.type,
            form = this.form,
            value = this.value,
            count = this.count,
            condition = this.condition,
            description = this.description
        };
    }

}
