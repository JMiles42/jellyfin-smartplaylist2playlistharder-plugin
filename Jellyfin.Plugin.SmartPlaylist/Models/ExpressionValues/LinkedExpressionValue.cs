namespace Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

public record LinkedExpressionValue : ExpressionValue
{
    public static LinkedExpressionValue Create(string name) => new()
    {
        Value = ExpressionValue.Create(name),
        VarName = name.Replace("$(","").Replace(")", ""),
        VarSource = name,
    };

    public ExpressionValue Value { get; private set; } = NullExpressionValue.Instance;

    public string VarName { get; init; } = string.Empty;
    public string VarSource { get; init; } = string.Empty;

    public override object SingleValue => Value.SingleValue;

    public override bool IsSingleValue => Value.IsSingleValue;

    public override IEnumerable<object> GetValues()
    {
        return Value.GetValues();
    }

    public void LinkValue(ExpressionValue value) => Value = value;
}
