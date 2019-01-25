using System;
using System.Collections.Generic;

namespace Astruk.Common.Interfaces
{
	public interface IValidationRuleWithParameters<in TRule> : IValidationRule<TRule>
	{
		List<object> ParameterList { get; }
		List<Type> ParameterTypes { get; }
	}
}
