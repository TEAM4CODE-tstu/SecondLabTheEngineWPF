using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainClassLibrary
{
    public class WpResult
    {
        public List<string> Steps { get; set; } = new List<string>(); // историю изменения формулы на каждом шаге
        public string FinalFormula { get; set; } // итоговое математическое условие
    }

    public class WpEngine
    {
        public WpResult CalculateWp(string code, string postCondition)
        {
            var result = new WpResult();            
            var lines = code.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries) 
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrEmpty(l))
                            .ToList();

            string currentPredicate = postCondition;
            result.Steps.Add($"Исходное постусловие (Post): {currentPredicate}");
                        
            for (int i = lines.Count - 1; i >= 0; i--) // обратный анализ Дейкстры (цикл от конца к началу)
            {
                string line = lines[i];
                                
                if (line.StartsWith("if")) // парсинг для пресетов                
                    currentPredicate = ProcessIfElse(line, currentPredicate, result.Steps);                                
                else if (line.Contains(":=")) // обработка обычного присваивания (например, x := e)                
                    currentPredicate = ProcessAssignment(line, currentPredicate, result.Steps);                
            }
            result.FinalFormula = currentPredicate;
            return result;
        }

        private string ProcessAssignment(string line, string currentPredicate, List<string> steps)
        {
            // Динамически режем строку по знаку присваивания
            var parts = line.Split(new[] { ":=" }, StringSplitOptions.None);
            if (parts.Length < 2) return currentPredicate; // Защита от кривых строк

            string variable = parts[0].Trim();
            string expression = parts[1].Trim();

            // Автоматическая проверка определенности (защита от деления на ноль)
            string safetyCondition = "";
            if (expression.Contains("/"))
            {
                var denominator = expression.Split('/').Last().Trim().Replace(")", "").Replace("(", "");
                safetyCondition = $" && ({denominator} != 0)";
            }

            // Математическая подстановка Дейкстры: ищет имя переменной \b...\b в любой формуле
            // и меняет её на то, что пришло из интерфейса, сохраняя знаки сравнения и введённые числа
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
            // 1. Динамически выковыриваем условие развилки из скобок if (...)
            var matchCondition = Regex.Match(line, @"if\s*\((.*?)\)");
            string condition = matchCondition.Success ? matchCondition.Groups[1].Value : "B";

            // 2. Вырезаем код веток then и else, игнорируя регистр букв и пробелы
            var thenMatch = Regex.Match(line, @"then\s+(.*?)(?=else|$)");
            var elseMatch = Regex.Match(line, @"else\s+(.*)");

            string thenStatement = thenMatch.Success ? thenMatch.Groups[1].Value.Trim() : "";
            string elseStatement = elseMatch.Success ? elseMatch.Groups[1].Value.Trim() : "";

            string wpThen = currentPredicate;
            string wpElse = currentPredicate;

            // 3. Динамическая подстановка для ветки THEN (подставит вместо переменной выражение)
            if (thenStatement.Contains(":="))
            {
                var parts = thenStatement.Split(new[] { ":=" }, StringSplitOptions.None);
                string variable = parts[0].Trim();
                string expression = parts[1].Trim();
                wpThen = Regex.Replace(currentPredicate, $@"\b{variable}\b", $"({expression})");
            }

            // 4. Динамическая подстановка для ветки ELSE (сработает аналогично)
            if (elseStatement.Contains(":="))
            {
                var parts = elseStatement.Split(new[] { ":=" }, StringSplitOptions.None);
                string variable = parts[0].Trim();
                string expression = parts[1].Trim();
                wpElse = Regex.Replace(currentPredicate, $@"\b{variable}\b", $"({expression})");
            }

            // 5. Собираем итоговый предикат по строгому правилу Дейкстры для ветвления
            string updatedPredicate = $"(({condition}) && ({wpThen})) || (!({condition}) && ({wpElse}))";

            steps.Add($"Протяжка через ветвление [if ({condition})]: {updatedPredicate}");
            return updatedPredicate;
        }


        //private string ProcessAssignment(string line, string currentPredicate, List<string> steps)
        //{
        //    var parts = line.Split(new[] { ":=" }, StringSplitOptions.None); // когда приходит строка типа x := x + 10
        //    string variable = parts[0].Trim(); // слева - переменная, справа - выражение
        //    string expression = parts[1].Trim();

        //    string safetyCondition = ""; // проверка определенности (защита от деления на ноль)
        //    if (expression.Contains("/"))
        //    {
        //        var denominator = expression.Split('/').Last().Trim().Replace(")", "").Replace("(", "");
        //        safetyCondition = $" && ({denominator} != 0)";
        //    }

        //    // математическая подстановка Дейкстры (замена переменной на выражение)
        //    // Regex берет текущую формулу цели, находит в ней переменную и меняет на выражение в ()
        //    // \b чтобы случайно не заменить часть имени другой переменной ('x' в слове 'max')
        //    string updatedPredicate = Regex.Replace(currentPredicate, $@"\b{variable}\b", $"({expression})");

        //    if (!string.IsNullOrEmpty(safetyCondition))
        //    {
        //        updatedPredicate = $"({updatedPredicate}){safetyCondition}";
        //    }

        //    steps.Add($"Протяжка через [{line}]: {updatedPredicate}");
        //    return updatedPredicate;
        //}

        //private string ProcessIfElse(string line, string currentPredicate, List<string> steps) // метод обработки развилки if-else
        //{           
        //    var matchCondition = Regex.Match(line, @"if\s*\((.*?)\)"); // ищет в коде if и вытаскивает всё из ()
        //    string condition = matchCondition.Success ? matchCondition.Groups[1].Value : "B"; // иначе запишется просто буква булева условия

        //    var thenMatch = Regex.Match(line, @"then\s+(.*?)(?=else|$)"); // ищет операторы веток then до else
        //    var elseMatch = Regex.Match(line, @"else\s+(.*)"); // ищет операторы после слова else

        //    string thenStatement = thenMatch.Success ? thenMatch.Groups[1].Value.Trim() : ""; // очистка вырезанного кода от пробелов
        //    string elseStatement = elseMatch.Success ? elseMatch.Groups[1].Value.Trim() : "";

        //    string wpThen = currentPredicate; // wp для каждой ветки отдельно
        //    string wpElse = currentPredicate;

        //    if (thenStatement.Contains(":=")) // когда сработал if 
        //    {
        //        var parts = thenStatement.Split(new[] { ":=" }, StringSplitOptions.None); 
        //        wpThen = Regex.Replace(currentPredicate, $@"\b{parts[0].Trim()}\b", $"({parts[1].Trim()})"); // max меняется на х1
        //    }
        //    if (elseStatement.Contains(":=")) // когда сработал else
        //    {
        //        var parts = elseStatement.Split(new[] { ":=" }, StringSplitOptions.None);
        //        wpElse = Regex.Replace(currentPredicate, $@"\b{parts[0].Trim()}\b", $"({parts[1].Trim()})"); // max меняется на х2
        //    }

        //    // формула Дейкстры для ветвления: (B && wp(S1, R)) || (!B && wp(S2, R))
        //    string updatedPredicate = $"(({condition}) && ({wpThen})) || (!({condition}) && ({wpElse}))";

        //    steps.Add($"Протяжка через ветвление [if ({condition})]: {updatedPredicate}");
        //    return updatedPredicate;
        //}
    }
}
