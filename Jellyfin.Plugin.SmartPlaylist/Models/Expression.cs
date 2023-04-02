namespace Jellyfin.Plugin.SmartPlaylist.Models;

public class Expression
{

    public string MemberName { get; set; }

    public string Operator { get; set; }

    public string TargetValue { get; set; }

    public bool InvertResult { get; set; }

    public StringComparison StringComparison { get; set; }

    public Expression(string memberName, string @operator, string targetValue, bool invertResult = false, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        MemberName = memberName;
        Operator = @operator;
        TargetValue = targetValue;
        InvertResult = invertResult;
        StringComparison = stringComparison;
    }
}
