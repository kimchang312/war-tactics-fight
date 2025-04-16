using System;
using System.Collections;
using System.Reflection;
using System.Text;

public static class UnitCombatStaticsExtensions
{
    // 모든 속성을 콘솔에 출력하는 확장 메서드
    public static void PrintAllProperties(this UnitCombatStatics unit)
    {
        // UnitCombatStatics 타입의 모든 속성을 가져옴
        PropertyInfo[] properties = unit.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Console.WriteLine($"Unit: {unit.UnitName} (ID: {unit.UnitID})");

        foreach (PropertyInfo property in properties)
        {
            // 속성 이름과 값을 출력
            object value = property.GetValue(unit);
            Console.WriteLine($"{property.Name}: {value}");
        }

        // Dictionary 필드도 출력
        FieldInfo[] fields = unit.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(unit);
            if (value is IDictionary dictionary)
            {
                Console.WriteLine($"{field.Name}:");
                foreach (var key in dictionary.Keys)
                {
                    Console.WriteLine($"  {key}: {dictionary[key]}");
                }
            }
            else
            {
                Console.WriteLine($"{field.Name}: {value}");
            }
        }
    }

    // 모든 속성을 문자열로 반환하는 확장 메서드
    public static string GetAllPropertiesAsText(this UnitCombatStatics unit)
    {
        var result = new StringBuilder();
        result.AppendLine($"Unit: {unit.UnitName} (ID: {unit.UnitID})");

        // 모든 속성 출력
        var properties = unit.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var value = property.GetValue(unit);
            result.AppendLine($"{property.Name}: {value}");
        }

        // Dictionary 같은 필드 출력
        var fields = unit.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(unit);
            if (value is IDictionary dictionary)
            {
                result.AppendLine($"{field.Name}:");
                foreach (var key in dictionary.Keys)
                {
                    result.AppendLine($"  {key}: {dictionary[key]}");
                }
            }
            else
            {
                result.AppendLine($"{field.Name}: {value}");
            }
        }

        return $"{result}"; // 최종 문자열 반환
    }
}
