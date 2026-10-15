// тесты Тактарова Артемия для второй лабораторной работы

using DomainClassLibrary;
using Lab1.Domain.Services;
using Lab1.Infrastructure;

public class SecondLabTest
{
    private readonly WpEngine _engine = new();

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

    [Fact]
    public void GetPreset_UnknownType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => WpPresets.GetPreset("unknown"));
    }

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

    [Fact]
    public void CalculateWp_CodeWithOnlyWhitespace_BehavesLikeEmpty()
    {
        string code = "   \n\t   ";
        string post = "y < 10";

        var result = _engine.CalculateWp(code, post);

        Assert.Single(result.Steps);
        Assert.Equal(post, result.FinalFormula);
    }

    [Fact]
    public void CalculateWp_NullCode_ThrowsException()
    {
        Assert.Throws<NullReferenceException>(() => _engine.CalculateWp(null!, "x > 0"));
    }

    [Fact]
    public void CalculateWp_NullPostCondition_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.CalculateWp("x := 1;", null!));
    }

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

    // Вспомогательный метод для проверки мультимножества (одинаковые элементы с одинаковой частотой)
    private static void AssertMultisetEqual(int[] expected, int[] actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        var expectedSorted = expected.OrderBy(x => x).ToArray();
        var actualSorted = actual.OrderBy(x => x).ToArray();
        Assert.Equal(expectedSorted, actualSorted);
    }

    [Theory]
    // Используем русские имена, как в SortAlgorithmFactory
    [InlineData(new[] { 3, 1, 2 }, "Подсчётом")]
    [InlineData(new[] { 5, 5, 2, 2, 9 }, "Быстрая")]
    [InlineData(new[] { -1, -5, 0, 3 }, "Слиянием")]
    public void Sort_ValidInput_ReturnsSortedArrayAndPreservesMultiset(
        int[] input, string algorithmName)
    {
        var (result, milliseconds) = SortingService.Sort(input, algorithmName);

        Assert.True(milliseconds >= 0, "Время должно быть неотрицательным");

        // Проверка упорядоченности
        for (int i = 1; i < result.Length; i++)
        {
            Assert.True(result[i - 1] <= result[i], "Массив не отсортирован по неубыванию");
        }

        // Проверка сохранения элементов (мультимножество)
        AssertMultisetEqual(input, result);
    }

    [Fact]
    public void Sort_NullArray_ThrowsArgumentExceptionViaGuard()
    {
        int[] input = null;
        string algorithmName = "Быстрая";

        var ex = Assert.Throws<ArgumentException>(() =>
            SortingService.Sort(input, algorithmName));

        Assert.Contains("не должен быть null или пустым", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sort_EmptyArray_ThrowsArgumentExceptionViaGuard()
    {
        int[] input = System.Array.Empty<int>();
        string algorithmName = "Слиянием";

        var ex = Assert.Throws<ArgumentException>(() =>
            SortingService.Sort(input, algorithmName));

        Assert.Contains("не должен быть null или пустым", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sort_UnknownAlgorithm_ThrowsArgumentException()
    {
        int[] input = new[] { 3, 1, 2 };
        string algorithmName = "NonExistentSort";

        // Важно: ловим именно ArgumentException, потому что фабрика его выбрасывает
        var ex = Assert.Throws<ArgumentException>(() =>
            SortingService.Sort(input, algorithmName));

        Assert.NotNull(ex);
        Assert.Contains("Неизвестный алгоритм", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }

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

    [Fact]
    public void Measure_MeasuresNonZeroTimeForSlowAction()
    {
        var delayMs = 100;

        // Явно указываем тип <int>, а лямбда теперь возвращает число
        var (milliseconds, _) = TimeMeasurement.Measure<int>(() =>
        {
            Thread.Sleep(delayMs);
            return 0; // Возвращаем фиктивное значение, чтобы удовлетворить Func<T>
        });

        Assert.True(milliseconds > 0, "Замер должен показать ненулевое время");
        Assert.True(milliseconds >= delayMs * 0.5, "Время должно быть хотя бы примерно сопоставимо с задержкой");
    }
}