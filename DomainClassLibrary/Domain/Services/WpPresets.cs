using System;

namespace DomainClassLibrary
{
    public class PresetData
    {
        public string Code { get; set; }
        public string PostFormula { get; set; }
        public string PostHuman { get; set; }
    }

    public static class WpPresets
    {
        public static PresetData GetPreset(string presetType)
        {
            return presetType.ToLower() switch
            {
                "max" => new PresetData
                {
                    Code = "if (x1 >= x2) then max := x1; else max := x2;",
                    PostFormula = "max > 100",
                    PostHuman = "Максимальный вес груза в партии превышает 100 кг"
                },
                "quad" or "equation" => new PresetData // Добавлено "quad"
                {
                    Code = "D := b * b - 4 * a * c;\nif (D >= 0) then x1 := (-b + 10) / (2 * a);",
                    PostFormula = "x1 > 0",
                    PostHuman = "Первый критический коэффициент распределения строго положителен"
                },
                "seq" or "sequence" => new PresetData // Добавлено "seq"
                {
                    Code = "x := x + 10;\ny := x * 2;",
                    PostFormula = "y > 50",
                    PostHuman = "Итоговый суммарный индекс погрузки y больше 50"
                },
                _ => throw new ArgumentException("Неизвестный тип пресета")
            };
        }

    }
}
