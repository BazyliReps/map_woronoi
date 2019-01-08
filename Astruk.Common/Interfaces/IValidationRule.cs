namespace Astruk.Common.Interfaces
{
	public interface IValidationRule<in TRule>
	{
		string ErrorMessage { get; }
		string ErrorKey { get; }
		bool IsCritical { get; }
		bool IsValid(TRule obj);
	}
}