using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Jellyfin.Plugin.SmartPlaylist.Models.ExpressionValues;

namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class SmartPlExpression
{
    public OperandMember MemberName { get; set; }

    [JsonIgnore]
    [IgnoreDataMember]
    private string _operator;

    public string Operator {
        get => _operator;
        set {
            _operator       = value;
            OperatorAsLower = value.ToLower();
        }
    }

    [JsonIgnore]
    [IgnoreDataMember]
    public string OperatorAsLower { get; private set; }

    public ExpressionValue TargetValue { get; set; }

    public bool InvertResult { get; set; }

    public StringComparison StringComparison { get; set; }

    public MatchMode Match { get; set; }

    public string? TypeOverride { get; set; }

    [JsonIgnore]
    [IgnoreDataMember]
    public bool IsInValid { get; }

    public SmartPlExpression(OperandMember    memberName,
                             string           @operator,
                             ExpressionValue  targetValue,
                             MatchMode        match            = MatchMode.Any,
                             bool             invertResult     = false,
                             StringComparison stringComparison = StringComparison.CurrentCulture,
                             string?           typeOverride     = null
                             ) {
        MemberName       = memberName;
        Operator         = @operator;
        TargetValue      = targetValue;
        TypeOverride     = typeOverride;
        InvertResult     = invertResult;
        StringComparison = stringComparison;
        Match            = match;
        IsInValid        = MemberName == OperandMember.Invalid;
    }

    /// <inheritdoc />
    public override string ToString() => $"{MemberName} {(InvertResult? "!" : "")}'{Operator}' {TargetValue}";
}
