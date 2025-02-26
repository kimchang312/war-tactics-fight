using System.Collections.Generic;

public class UnitPriceDatabase
{
    private Dictionary<int, int> unitPriceDict;

    public UnitPriceDatabase()
    {
        unitPriceDict = new Dictionary<int, int>
        {
            { 0, 105 }, { 1, 135 }, { 2, 150 }, { 3, 85 }, { 4, 115 },
            { 5, 195 }, { 6, 180 }, { 7, 165 }, { 8, 190 }, { 9, 145 },
            { 10, 270 }, { 11, 170 }, { 12, 190 }, { 13, 235 }, { 14, 190 },
            { 15, 425 }, { 16, 210 }, { 17, 230 }, { 18, 170 }, { 19, 200 },
            { 20, 340 }, { 21, 180 }, { 22, 225 }, { 23, 120 }, { 24, 225 },
            { 25, 330 }, { 26, 150 }, { 27, 150 }, { 28, 150 }, { 29, 120 },
            { 30, 220 }, { 31, 205 }, { 32, 220 }, { 33, 140 }, { 34, 95 },
            { 35, 100 }, { 36, 245 }, { 37, 225 }, { 38, 205 }, { 39, 180 },
            { 40, 260 }, { 41, 195 }, { 42, 145 }, { 43, 230 }, { 44, 215 },
            { 45, 345 }, { 46, 290 }, { 47, 140 }, { 48, 175 }, { 49, 210 },
            { 50, 320 }, { 51, 195 }, { 52, 560 }, { 53, 510 }, { 54, 340 },
            { 55, 400 }, { 56, 390 }, { 57, 380 }, { 58, 525 }
        };
    }

    // 특정 idx에 대한 가격 반환
    public int GetUnitPrice(int idx)
    {
        return unitPriceDict.TryGetValue(idx, out int price) ? price : -1;
    }

    // 여러 idx들의 가격 합산
    public int GetTotalPrice(List<int> ids)
    {
        int totalPrice = 0;
        foreach (int id in ids)
        {
            if (unitPriceDict.TryGetValue(id, out int price))
            {
                totalPrice += price;
            }
        }
        return totalPrice;
    }
}
