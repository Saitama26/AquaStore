namespace AquaStore.Domain.Enums;

/// <summary>
/// Типы фильтров для воды
/// </summary>
public enum FilterType
{
    /// <summary>
    /// Фильтр-кувшин
    /// </summary>
    Pitcher = 0,

    /// <summary>
    /// Проточный фильтр под мойку
    /// </summary>
    UnderSink = 1,

    /// <summary>
    /// Система обратного осмоса
    /// </summary>
    ReverseOsmosis = 2,

    /// <summary>
    /// Магистральный фильтр
    /// </summary>
    MainLine = 3,

    /// <summary>
    /// Насадка на кран
    /// </summary>
    FaucetMount = 4,

    /// <summary>
    /// Сменный картридж
    /// </summary>
    ReplacementCartridge = 5,

    /// <summary>
    /// Фильтр для душа
    /// </summary>
    Shower = 6,

    /// <summary>
    /// Умягчитель воды
    /// </summary>
    WaterSoftener = 7
}

