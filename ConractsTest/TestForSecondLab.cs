using DomainClassLibrary;
using Lab1.Domain.Services;
using Lab1.Infrastructure;

public class SecondLabTest
{
    private readonly WpEngine _engine = new();

    // Блок 1. Проверки WpPrseets

    /// <summary>
    /// Проверяет, что метод WpPresets.GetPreset корректно возвращает заранее заготовленный шаблон
    /// для задачи поиска максимального значения, когда ему передают ключ "max".
    /// </summary>
    [Theory]
    [InlineData("max")]
    public void GetPreset_Max_ReturnsExpectedData(string presetType)
    {
        var result = WpPresets.GetPreset(presetType);

        Assert.NotNull(result);
        Assert.Equal("if (x1 >= x2) then max := x1 else max := x2", result.Code);
        Assert.Equal("max > 100", result.PostFormula);
        Assert.Equal("Максимальный вес груза в партии превышает 100 кг", result.PostHuman);
    }

    /// <summary>
    /// Проверяет, что система корректно возвращает шаблон (пресет) для решения квадратного уравнения, 
    /// и делает это одинаково хорошо для двух разных названий-синонимов: "quad" и "equation".
    /// </summary>
    [Theory]
    [InlineData("quad")]
    [InlineData("equation")]
    public void GetPreset_QuadOrEquation_ReturnsExpectedData(string presetType)
    {
        var result = WpPresets.GetPreset(presetType);

        Assert.NotNull(result);
        Assert.Contains("D := b * b - 4 * a * c", result.Code);
        Assert.Contains("/ (2 * a)", result.Code);
        Assert.Equal("x1 > 0", result.PostFormula);
    }

    /// <summary>
    /// Проверяет, что система корректно выдает шаблон для последовательности действий, 
    /// и поддерживает два разных названия для этого шаблона: короткое "seq" и полное
    /// </summary>
    [Theory]
    [InlineData("seq")]
    [InlineData("sequence")]
    public void GetPreset_SeqOrSequence_ReturnsExpectedData(string presetType)
    {
        var result = WpPresets.GetPreset(presetType);

        Assert.NotNull(result);
        Assert.Contains("x := x + 10", result.Code);
        Assert.Contains("y := x * 2", result.Code);
        Assert.Equal("y > 50", result.PostFormula);
    }

    /// <summary>
    /// проверяет, как система реагирует на заведомо ошибочный запрос 
    /// (запрос несуществующего пресета "unknown").
    /// </summary>
    [Fact]
    public void GetPreset_UnknownType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => WpPresets.GetPreset("unknown"));
    }

    // Блок 2. Тестирование класса WpEngine

    /// <summary>
    /// Проверяет простейшую замену. 
    /// Если после кода x := x + 10 должно быть x > 20, 
    /// то до выполнения кода должно быть (x + 10) > 20.
    /// </summary>
    [Fact]
    public void CalculateWp_SingleAssignment_PerformsSubstitution()
    {
        string code = "x := x + 10;";
        string post = "x > 20";

        var result = _engine.CalculateWp(code, post);

        Assert.Equal(2, result.Steps.Count);

        string finalFormula = result.FinalFormula;

        Assert.Contains("x + 10", finalFormula);
        Assert.Contains("> 20", finalFormula);
    }

    /// <summary>
    /// Проверяет, что множественные присваивания 
    /// обрабатываются в ОБРАТНОМ порядке (снизу вверх)
    /// </summary>
    [Fact]
    public void CalculateWp_MultipleAssignments_ProcessesInReverseOrder()
    {
        string code = @"x := x + 10;
                        y := x * 2;";

        string post = "y > 50";

        var result = _engine.CalculateWp(code, post);

        Assert.Equal(3, result.Steps.Count);
        Assert.True(result.Steps[0].StartsWith("Исходное постусловие"));
        Assert.True(result.Steps[1].StartsWith("Протяжка через [y := x * 2]:"));
        Assert.True(result.Steps[2].StartsWith("Протяжка через [x := x + 10]:"));

        Assert.Contains("(x + 10)", result.FinalFormula);
        Assert.Contains("* 2", result.FinalFormula);
        Assert.Contains("> 50", result.FinalFormula);
    }

    /// <summary>
    /// Проверяет генерацию условий безопасности.
    /// При наличии операции деления движок должен автоматически добавить 
    /// требование, что знаменатель не равен нулю.
    /// </summary>
    [Fact]
    public void CalculateWp_Division_AddsSafetyCondition()
    {
        string code = "x := (a + b) / c;";
        string post = "x > 0";

        var result = _engine.CalculateWp(code, post);

        Assert.Equal(2, result.Steps.Count);
        Assert.True(result.Steps[0].StartsWith("Исходное постусловие"));
        Assert.True(result.Steps[1].StartsWith("Протяжка через [x := (a + b) / c]:"));

        string formula = result.FinalFormula;

        Assert.Contains("c", formula);
        Assert.Contains("!=", formula);
        Assert.Contains("0", formula);

        Assert.Contains("a", formula);
        Assert.Contains("+", formula);
        Assert.Contains("b", formula);
        Assert.Contains("/", formula);
        Assert.Contains("> 0", formula);
    }

    /// <summary>
    /// Проверяет корректный парсинг сложного знаменателя (выражения в скобках).
    /// Условие безопасности должно применяться ко всему выражению в знаменателе целиком.
    /// </summary>
    [Fact]
    public void CalculateWp_ComplexDenominator_ExtractsLastPartAfterSlash()
    {
        string code = "x := a / (b + c);";
        string post = "x < 10";

        var result = _engine.CalculateWp(code, post);

        Assert.Equal(2, result.Steps.Count);

        string formula = result.FinalFormula;

        Assert.Contains("b", formula);
        Assert.Contains("+", formula);
        Assert.Contains("c", formula);
        Assert.Contains("!=", formula);
        Assert.Contains("0", formula);

        Assert.Contains("a", formula);
        Assert.Contains("/", formula);
        Assert.Contains("< 10", formula);
    }

    /// <summary>
    ///  Проверяет, что для if-else в итоговой формуле появляются обе части:
    ///  условие (x1 >= x2) и его отрицание !(x1 >= x2).
    /// </summary>
    [Fact]
    public void CalculateWp_IfElse_GeneratesDijkstraFormula()
    {
        string code = "if (x1 >= x2) then max := x1 else max := x2";
        string post = "max > 100";

        var result = _engine.CalculateWp(code, post);

        Assert.Equal(2, result.Steps.Count);
        Assert.True(result.Steps[1].StartsWith("Протяжка через ветвление [if (x1 >= x2)]:"));

        string formula = result.FinalFormula;
        Assert.Contains("(x1 >= x2)", formula);
        Assert.Contains("!(x1 >= x2)", formula);
    }

    /// <summary>
    /// Проверяет, что подстановка значений (10 и 20)
    /// происходит внутри обеих веток условия.
    /// </summary>
    [Fact]
    public void CalculateWp_IfWithAssignmentInThen_SubstitutesThenBranch()
    {
        string code = "if (B) then x := 10 else x := 20";
        string post = "x > 5";

        var result = _engine.CalculateWp(code, post);

        Assert.Equal(2, result.Steps.Count);
        string final = result.FinalFormula;

        Assert.Contains("B", final);
        Assert.Contains("10", final);
        Assert.Contains("20", final);
    }

    /// <summary>
    /// Проверяет ситуацию, когда есть только if, но нет else.
    /// </summary>
    [Fact]
    public void CalculateWp_IfWithoutElse_TreatsElseAsSkip()
    {
        string code = "if (B) then x := 10";
        string post = "x > 0";

        var result = _engine.CalculateWp(code, post);

        Assert.Equal(2, result.Steps.Count);
        string final = result.FinalFormula;

        Assert.Contains("B", final);
        Assert.Contains("x > 0", final);
    }

    /// <summary>
    /// Передает пустую строку "" в качестве кода.
    /// </summary>
    [Fact]
    public void CalculateWp_EmptyCode_ReturnsOriginalPostconditionAsFinal()
    {
        string code = "";
        string post = "x > 0";

        var result = _engine.CalculateWp(code, post);

        Assert.Single(result.Steps);
        Assert.StartsWith("Исходное постусловие", result.Steps[0]);
        Assert.Equal(post, result.FinalFormula);
    }

    /// <summary>
    /// Проверяет, что код, состоящий исключительно из пробельных символов, 
    /// обрабатывается так же, как и полностью пустой код.
    /// </summary>
    [Fact]
    public void CalculateWp_CodeWithOnlyWhitespace_BehavesLikeEmpty()
    {
        string code = "   \n\t   ";
        string post = "y < 10";

        var result = _engine.CalculateWp(code, post);

        Assert.Single(result.Steps);
        Assert.Equal(post, result.FinalFormula);
    }

    /// <summary>
    /// Передача null вместо кода программы 
    /// должна приводить к предсказуемому исключению.
    /// </summary>
    [Fact]
    public void CalculateWp_NullCode_ThrowsException()
    {
        Assert.Throws<NullReferenceException>(() => _engine.CalculateWp(null!, "x > 0"));
    }

    /// <summary>
    /// Передача null вместо постусловия 
    /// должна приводить к предсказуемому исключению.
    /// </summary>
    [Fact]
    public void CalculateWp_NullPostCondition_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.CalculateWp("x := 1;", null!));
    }

    /// <summary>
    /// Проверяет корректность замены имен переменных с использованием границ слов.
    /// Замена переменной "max" не должна случайно изменять подстроку внутри другого слова (например, "maximum").
    /// </summary>
    [Fact]
    public void CalculateWp_VariableNameAsSubstring_UsesWordBoundary()
    {
        string code = "max := x1;";
        string post = "maximum > 10 && max > 5";

        var result = _engine.CalculateWp(code, post);

        Assert.Equal(2, result.Steps.Count);
        string final = result.FinalFormula;

        Assert.DoesNotContain("(x1)imum", final);
        Assert.Contains("maximum", final);
        Assert.Contains("(x1)", final);
    }

    // Блок 3: Тестирование класса SortingService

    /// <summary>
    /// Вспомогательный метод для проверки мультимножества.
    /// Гарантирует, что два массива содержат одинаковые элементы 
    /// с одинаковой частотой, игнорируя их исходный порядок.
    /// </summary>
    private static void AssertMultisetEqual(int[] expected, int[] actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        var expectedSorted = expected.OrderBy(x => x).ToArray();
        var actualSorted = actual.OrderBy(x => x).ToArray();
        Assert.Equal(expectedSorted, actualSorted);
    }

    /// <summary>
    /// Параметризованный тест корректности сортировки.
    /// Проверяет разные алгоритмы ("Подсчётом", "Быстрая", "Слиянием") 
    /// на разных входных данных. Убеждается, что массив отсортирован 
    /// по неубыванию и ни один элемент не был потерян или добавлен.
    /// </summary>
    [Theory]
    [InlineData(new[] { 3, 1, 2 }, "Подсчётом")]
    [InlineData(new[] { 5, 5, 2, 2, 9 }, "Быстрая")]
    [InlineData(new[] { -1, -5, 0, 3 }, "Слиянием")]
    public void Sort_ValidInput_ReturnsSortedArrayAndPreservesMultiset(
        int[] input, string algorithmName)
    {
        var (result, milliseconds) = SortingService.Sort(input, algorithmName);

        Assert.True(milliseconds >= 0, "Время должно быть неотрицательным");

        for (int i = 1; i < result.Length; i++)
        {
            Assert.True(result[i - 1] <= result[i], "Массив не отсортирован по неубыванию");
        }

        AssertMultisetEqual(input, result);
    }

    /// <summary>
    /// Проверяет защитный механизм: передача null 
    /// должна прерывать выполнение и выбрасывать ArgumentException.
    /// </summary>
    [Fact]
    public void Sort_NullArray_ThrowsArgumentExceptionViaGuard()
    {
        int[] input = null;
        string algorithmName = "Быстрая";

        var ex = Assert.Throws<ArgumentException>(() =>
            SortingService.Sort(input, algorithmName));

        Assert.Contains("не должен быть null или пустым", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Проверяет защитный механизм для пустого массива. 
    /// В данной реализации сортировка пустого массива считается недопустимой операцией.
    /// </summary>
    [Fact]
    public void Sort_EmptyArray_ThrowsArgumentExceptionViaGuard()
    {
        int[] input = System.Array.Empty<int>();
        string algorithmName = "Слиянием";

        var ex = Assert.Throws<ArgumentException>(() =>
            SortingService.Sort(input, algorithmName));

        Assert.Contains("не должен быть null или пустым", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Проверяет, что программа корректно реагирует на запрос 
    /// несуществующего алгоритма сортировки, выбрасывая ArgumentException.
    /// </summary>
    [Fact]
    public void Sort_UnknownAlgorithm_ThrowsArgumentException()
    {
        int[] input = new[] { 3, 1, 2 };
        string algorithmName = "NonExistentSort";

        var ex = Assert.Throws<ArgumentException>(() =>
            SortingService.Sort(input, algorithmName));

        Assert.NotNull(ex);
        Assert.Contains("Неизвестный алгоритм", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Граничный случай: массив из одного элемента. 
    /// Результат должен быть идентичен исходному массиву.
    /// </summary>
    [Fact]
    public void Sort_SingleElement_ReturnsSameElement()
    {
        int[] input = new[] { 42 };
        string algorithmName = "Подсчётом";

        var (result, _) = SortingService.Sort(input, algorithmName);

        Assert.Single(result);
        Assert.Equal(42, result[0]);
        AssertMultisetEqual(input, result);
    }

    /// <summary>
    /// Проверяет поведение алгоритма на уже отсортированном массиве.
    /// Алгоритм должен корректно завершить работу, не нарушив порядок 
    /// и не потеряв элементы.
    /// </summary>
    [Fact]
    public void Sort_AlreadySorted_ReturnsSameArrayWithoutChangesToMultiset()
    {
        int[] input = new[] { 1, 2, 3, 4, 5 };
        string algorithmName = "Быстрая";

        var (result, _) = SortingService.Sort(input, algorithmName);

        for (int i = 1; i < result.Length; i++)
            Assert.True(result[i - 1] <= result[i]);

        AssertMultisetEqual(input, result);
    }

    /// <summary>
    /// Проверяет базовую функциональность утилиты замера времени.
    /// Убеждается, что переданное действие выполняется ровно один раз, 
    /// возвращает правильный результат, а замеренное время неотрицательно.
    /// </summary>
    [Fact]
    public void Measure_ActionExecutesAndReturnsResult()
    {
        const int expectedValue = 42;
        int executedCount = 0;

        var (milliseconds, result) = TimeMeasurement.Measure<int>(() =>
        {
            executedCount++;
            return expectedValue;
        });

        Assert.Equal(1, executedCount);
        Assert.Equal(expectedValue, result);
        Assert.True(milliseconds >= 0);
    }

    /// <summary>
    /// Проверяет, что утилита замера времени корректно фиксирует реальные задержки.
    /// Искусственная пауза (Thread.Sleep) должна отразиться в измеренном времени 
    /// (с небольшим допуском на погрешность планировщика операционной системы).
    /// </summary>
    [Fact]
    public void Measure_MeasuresNonZeroTimeForSlowAction()
    {
        var delayMs = 100;

        var (milliseconds, _) = TimeMeasurement.Measure<int>(() =>
        {
            Thread.Sleep(delayMs);
            return 0;
        });

        Assert.True(milliseconds > 0, "Замер должен показать ненулевое время");
        Assert.True(milliseconds >= delayMs * 0.5, "Время должно быть хотя бы примерно сопоставимо с задержкой");
    }
}