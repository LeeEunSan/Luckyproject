using System;
using System.Collections.Generic;

public static class IntExtension
{
    private static List<Tuple<int, string>> units = new List<Tuple<int, string>>()
    {
        new Tuple<int, string>(15, "Q"),  
        new Tuple<int, string>(12, "T"),  
        new Tuple<int, string>(9, "B"),   
        new Tuple<int, string>(6, "M"),   
        new Tuple<int, string>(3, "K"),   
    };
    
    public static string ToStringKilo(this int num)
    {
        if (num < 1000) return num.ToString();
        
        foreach (var unit in units)
        {
            if (num >= Math.Pow(10, unit.Item1))
            {
                double result = num / Math.Pow(10, unit.Item1);
                return $"{Math.Floor(result * 100) / 100}<size=80%>{unit.Item2}</size>";
            }
        }

        return num.ToString();
    }
}