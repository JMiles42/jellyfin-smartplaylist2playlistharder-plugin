namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class SmartPlExpression
{
    public string MemberName { get; set; }

    private string @operator;

    public string Operator {
        get => @operator;
        set {
            @operator       = value;
            OperatorAsLower = value.ToLower();
        }
    }

    public  string OperatorAsLower { get; private set; }

    public string TargetValue { get; set; }

    public bool InvertResult { get; set; }

    public StringComparison StringComparison { get; set; }

    public SmartPlExpression(string memberName, string @operator, string targetValue, bool invertResult = false, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        MemberName = memberName;
        Operator = @operator;
        TargetValue = targetValue;
        InvertResult = invertResult;
        StringComparison = stringComparison;
    }

    /// <inheritdoc />
    public override string ToString() {
        return $"{MemberName} {(InvertResult ? "!" : "")}'{Operator}' {TargetValue}";
    }
}
