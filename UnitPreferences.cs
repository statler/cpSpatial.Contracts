using cpSpatial.Contract.Enums;

namespace cpSpatial.Contract
{
    public sealed record UnitPreferences(
        LengthUnit LengthUnit = LengthUnit.Metre,
        int LengthDecimals = 3,
        bool IncludeUnitSuffix = true
    );
}