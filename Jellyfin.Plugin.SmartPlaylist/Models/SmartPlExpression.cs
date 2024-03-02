namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class SmartPlExpression
{

	public static SmartPlExpression Empty = new(OperandMember.Name,
												"IfYouSeeThisSomethingHasGoneWrong",
												NullExpressionValue.Instance);

	[JsonIgnore]
	[IgnoreDataMember]
	private string _operator;

	public OperandMember MemberName { get; set; }

	public string Operator
	{
		get => _operator;
		set
		{
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
							 string?          typeOverride     = null)
	{
		//Make nullable happy
		_operator       = string.Empty;
		OperatorAsLower = string.Empty;
		//Make nullable happy

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
