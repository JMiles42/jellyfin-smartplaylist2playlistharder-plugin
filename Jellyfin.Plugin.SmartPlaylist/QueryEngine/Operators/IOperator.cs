using System.Linq.Expressions;
using Jellyfin.Plugin.SmartPlaylist.Models;
using Jellyfin.Plugin.SmartPlaylist.QueryEngine.Containers;

namespace Jellyfin.Plugin.SmartPlaylist.QueryEngine.Operators;

public interface IOperator
{
	/// <summary>
	/// This method validates if the operator is able to process the provided expression.
	/// Used to confirm if the method <see cref="GetOperator"/> should return a valid Expression
	/// </summary>
	/// <typeparam name="T">The Type that contains all the data to be checking against</typeparam>
	/// <param name="plExpression">The expression to check if it can be parsed as this operator</param>
	/// <param name="sourceExpression">The left hand of the operation, eg the Operator's property</param>
	/// <param name="parameterExpression">The parameters of the T object</param>
	/// <param name="parameterPropertyType">The property type of the parameter</param>
	/// <returns>An OperatorResult that indicates any errors, or success</returns>
	EngineOperatorResult ValidateOperator<T>(SmartPlExpression   plExpression,
											 MemberExpression    sourceExpression,
											 ParameterExpression parameterExpression,
											 Type                parameterPropertyType);

	/// <summary>
	/// Creates the expression to evaluate the operator
	/// This method expects that you have validated the operator using <see cref="ValidateOperator"/>
	/// </summary>
	/// <typeparam name="T">The Type that contains all the data to be checking against</typeparam>
	/// <param name="plExpression">The expression to check if it can be parsed as this operator</param>
	/// <param name="sourceExpression">The left hand of the operation, eg the Operator's property</param>
	/// <param name="parameterExpression">The parameters of the T object</param>
	/// <param name="parameterPropertyType">The property type of the parameter</param>
	/// <returns>The built expression that calculates the plExpression</returns>
	ParsedValueExpressions GetOperator<T>(SmartPlExpression   plExpression,
									  MemberExpression    sourceExpression,
									  ParameterExpression parameterExpression,
									  Type                parameterPropertyType);
}
