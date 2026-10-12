using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainClassLibrary
{
    public class WpResult
    {
        public List<string> Steps { get; set; } = new List<string>();
        public string FinalFormula { get; set; }
        public string HumanText { get; set; }
    }

    public class WpEngine
    {
        public WpResult CalculateWp(string code, string postCondition)
        {
            var result = new WpResult();

            // Очищаем код от лишних пробелов и разбиваем на строки по точке с запятой
            var lines = code.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrEmpty(l))
                            .ToList();

            string currentPredicate = postCondition;
            result.Steps.Add($"Исходное постусловие (Post): {currentPredicate}");

            // Идем по командам снизу вверх (обратный анализ Дейкстры)
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                string line = lines[i];

                // 1. Обработка ветвления IF-ELSE (Упрощенный парсинг для пресетов)
                if (line.StartsWith("if"))
                {
                    currentPredicate = ProcessIfElse(line, currentPredicate, result.Steps);
                }
                // 2. Обработка обычного присваивания (например, x := e)
                else if (line.Contains(":="))
                {
                    currentPredicate = ProcessAssignment(line, currentPredicate, result.Steps);
                }
            }

            result.FinalFormula = currentPredicate;
            return result;
        }

        private string ProcessAssignment(string line, string currentPredicate, List<string> steps)
        {
            var parts = line.Split(new[] { ":=" }, StringSplitOptions.None);
            string variable = parts[0].Trim();
            string expression = parts[1].Trim();

            // Автоматическая проверка определенности (защита от деления на ноль)
            string safetyCondition = "";
            if (expression.Contains("/"))
            {
                var denominator = expression.Split('/').Last().Trim().Replace(")", "").Replace("(", "");
                safetyCondition = $" && ({denominator} != 0)";
            }

            // Математическая подстановка Дейкстры: заменяем переменную на выражение
            // Используем Regex, чтобы случайно не заменить часть имени другой переменной (например, 'x' в слове 'max')
            string updatedPredicate = Regex.Replace(currentPredicate, $@"\b{variable}\b", $"({expression})");

            if (!string.IsNullOrEmpty(safetyCondition))
            {
                updatedPredicate = $"({updatedPredicate}){safetyCondition}";
            }

            steps.Add($"Протяжка через [{line}]: {updatedPredicate}");
            return updatedPredicate;
        }

        private string ProcessIfElse(string line, string currentPredicate, List<string> steps)
        {
            // Извлекаем условие из if (...)
            var matchCondition = Regex.Match(line, @"if\s*\((.*?)\)");
            string condition = matchCondition.Success ? matchCondition.Groups[1].Value : "B";

            // Ищем операторы веток then и else
            var thenMatch = Regex.Match(line, @"then\s+(.*?)(?=else|$)");
            var elseMatch = Regex.Match(line, @"else\s+(.*)");

            string thenStatement = thenMatch.Success ? thenMatch.Groups[1].Value.Trim() : "";
            string elseStatement = elseMatch.Success ? elseMatch.Groups[1].Value.Trim() : "";

            // Вычисляем wp для каждой ветки отдельно
            string wpThen = currentPredicate;
            string wpElse = currentPredicate;

            if (thenStatement.Contains(":="))
            {
                var parts = thenStatement.Split(new[] { ":=" }, StringSplitOptions.None);
                wpThen = Regex.Replace(currentPredicate, $@"\b{parts[0].Trim()}\b", $"({parts[1].Trim()})");
            }
            if (elseStatement.Contains(":="))
            {
                var parts = elseStatement.Split(new[] { ":=" }, StringSplitOptions.None);
                wpElse = Regex.Replace(currentPredicate, $@"\b{parts[0].Trim()}\b", $"({parts[1].Trim()})");
            }

            // Формула Дейкстры для ветвления: (B && wp(S1, R)) || (!B && wp(S2, R))
            string updatedPredicate = $"(({condition}) && ({wpThen})) || (!({condition}) && ({wpElse}))";

            steps.Add($"Протяжка через ветвление [if ({condition})]: {updatedPredicate}");
            return updatedPredicate;
        }
    }
}
