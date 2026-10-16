using System;

namespace DomainClassLibrary
{
    public class PresetData
    {
        public string Code { get; set; } // программный код
        public string PostFormula { get; set; } // математическая цель (краткое постусловие)
        public string PostHuman { get; set; } // перевод этой цели на человеческий язык 
    }

    public static class WpPresets
    {
        public static PresetData GetPreset(string presetType) // метод выдачи пресетов
        {
            return presetType.ToLower() switch 
            {
                "max" => new PresetData
                {
                    Code = "if (x1 >= x2) then max := x1 else max := x2",
                    PostFormula = "max > 100",
                    PostHuman = "Максимальный вес груза в партии превышает 100 кг"
                },
                "quad" or "equation" => new PresetData
                {
                    Code = "D := b * b - 4 * a * c;\nif (D >= 0) then x1 := (-b + sqrt(D)) / (2 * a);",
                    PostFormula = "x1 > 0",
                    PostHuman = "Найденный корень уравнения существует и является положительным числом"
                },
                "seq" or "sequence" => new PresetData
                {
                    Code = "x := x + 10;\ny := x * 2;",
                    PostFormula = "y > 50",
                    PostHuman = "Вес груза с учётом упаковки (+10 кг) и страхового запаса (х2) превышает 50 кг"
                },
                _ => throw new ArgumentException("Неизвестный тип пресета")
            };
        }

    }
}
