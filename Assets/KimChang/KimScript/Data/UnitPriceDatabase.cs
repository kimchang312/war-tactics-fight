using System.Collections.Generic;

public class UnitPriceDatabase
{
    private Dictionary<int, int> unitPriceDict;

    public UnitPriceDatabase()
    {
        unitPriceDict = new Dictionary<int, int>
        {
            { 0, 95 }, { 1, 125 }, { 2, 150 }, { 3, 100 }, { 4, 130 },
            { 5, 190 }, { 6, 175 }, { 7, 165 }, { 8, 185 }, { 9, 160 },
            { 10, 290 }, { 11, 160 }, { 12, 190 }, { 13, 235 }, { 14, 200 },
            { 15, 425 }, { 16, 200 }, { 17, 230 }, { 18, 185 }, { 19, 200 },
            { 20, 360 }, { 21, 170 }, { 22, 225 }, { 23, 135 }, { 24, 220 },
            { 25, 340 }, { 26, 140 }, { 27, 155 }, { 28, 150 }, { 29, 120 },
            { 30, 220 }, { 31, 220 }, { 32, 220 }, { 33, 155 }, { 34, 135 },
            { 35, 115 }, { 36, 265 }, { 37, 225 }, { 38, 210 }, { 39, 205 },
            { 40, 260 }, { 41, 205 }, { 42, 140 }, { 43, 225 }, { 44, 220 },
            { 45, 355 }, { 46, 310 }, { 47, 140 }, { 48, 190 }, { 49, 225 },
            { 50, 320 }, { 51, 210 }, { 52, 540 }, { 53, 520 }, { 54, 415 },
            { 55, 445 }, { 56, 405 }, { 57, 380 }, { 58, 540 }, { 59, 425 },
            { 60, 400 }, { 61, 235 }, { 62, 220 }, { 63, 285 }, { 64, 260 },
            { 65, 425 }, { 66, 260 }
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
