using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class SmartPlExpression
{
    public OperandMember MemberName { get; set; }

    [JsonIgnore]
    [IgnoreDataMember]
    private string @operator;

    public string Operator {
        get => @operator;
        set {
            @operator       = value;
            OperatorAsLower = value.ToLower();
        }
    }

    [JsonIgnore]
    [IgnoreDataMember]
    public string OperatorAsLower { get; private set; }

    public string TargetValue { get; set; }

    public bool InvertResult { get; set; }

    public StringComparison StringComparison { get; set; }

    [JsonIgnore]
    [IgnoreDataMember]
    public bool IsInValid { get; }

    public SmartPlExpression(OperandMember memberName, string @operator, string targetValue, bool invertResult = false, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        MemberName       = memberName;
        Operator         = @operator;
        TargetValue      = targetValue;
        InvertResult     = invertResult;
        StringComparison = stringComparison;
        IsInValid          = MemberName == OperandMember.Invalid;
    }

    /// <inheritdoc />
    public override string ToString() => $"{MemberName} {(InvertResult ? "!" : "")}'{Operator}' {TargetValue}";
}
