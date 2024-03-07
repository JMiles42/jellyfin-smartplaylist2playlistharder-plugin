namespace Jellyfin.Plugin.SmartPlaylist.Infrastructure.QueryEngine.RuleFixers;

public class PremiereDateExpressionFixer : IExpressionFixer
{

    /// <inheritdoc />
    public void FixExpression(SmartPlExpression expression)
    {
        if (!expression.TargetValue.IsSingleValue)
        {
            return;
        }

        DateTime rst;

        switch (expression.TargetValue)
        {
            case ExpressionValue<DateTime> ev:
                {
                    rst = ev.Value;

                    break;
                }

            case ExpressionValueList<DateTime> ev:
                {
                    rst = ev.Value[0];

                    break;
                }
            case ExpressionValue<DateOnly> ev:
                {
                    rst = ev.Value.ToDateTime(TimeOnly.MinValue);

                    break;
                }

            case ExpressionValueList<DateOnly> ev:
                {
                    rst = ev.Value[0].ToDateTime(TimeOnly.MinValue);

                    break;
                }

            case ExpressionValue<string> ev:
                {
                    rst = DateTime.Parse(ev.Value);

                    break;
                }

            case ExpressionValueList<string> { IsSingleValue: true, } ev:
                {
                    rst = DateTime.Parse(ev.Value[0]);

                    break;
                }

            default: return;
        }

        expression.TargetValue = rst.ToString(CultureInfo.InvariantCulture).ToExpressionValue();
    }
}
