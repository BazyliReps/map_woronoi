using System;
using System.Collections.Generic;
using System.Linq;
using Astruk.Common.Interfaces;

namespace Astruk.Common.Utility
{
	public static class ValidatorOf<T>
	{
		public static ValidatorBuilder Validate()
		{
			return new ValidatorBuilder();
		}

		public class ValidatorBuilder
		{
			private readonly List<IValidationRule<T>> validationRules;

			public ValidatorBuilder()
			{
				validationRules = new List<IValidationRule<T>>();
			}

			public ValidatorBuilder WithRule<TRule>() where TRule : IValidationRule<T>, new()
			{
				validationRules.Add(new TRule());
				return this;
			}

			public ValidatorBuilder WithParam(object param)
			{
				var rule = (IValidationRuleWithParameters<T>) validationRules.Last();

				var type = rule.ParameterTypes[rule.ParameterList.Count];
				if (!type.IsInstanceOfType(param))
					throw new ArgumentException($"{param} is not of expected type {type}", nameof(param));
				rule.ParameterList.Add(param);
				return this;
			}

			public ValidationResult<T> ForObject(T validatedObject)
			{
				var result = new ValidationResult<T>();
				foreach (var rule in validationRules)
				{
					if (rule is IValidationRuleWithParameters<T> ruleWithParameters &&
					    ruleWithParameters.ParameterList.Count != ruleWithParameters.ParameterTypes.Count)
						throw new ArgumentException(
							$"Wrong amount of arguments supplied for {nameof(rule)}, should be {ruleWithParameters.ParameterTypes.Count} and is {ruleWithParameters.ParameterList.Count}");

					if (rule.IsValid(validatedObject)) continue;
					result.FailedValidations.Add(rule);
					if (rule.IsCritical) break;
				}

				return result;
			}
		}
	}
}