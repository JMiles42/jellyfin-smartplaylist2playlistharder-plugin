using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.CustomOperators;

public interface IEngineOperator
{
	bool IsOperatorFor<T>(SmartPlExpression expression, ParameterExpression parameterExpression, Type propertyType);

	bool GetOperatorFor<T>(SmartPlExpression expression, MemberExpression sourceExpression, ParameterExpression parameterExpression, Type propertyType, out Expression resultExpression);
}
