using Newtonsoft.Json;

namespace Acklann.Plaid.Management
{
	/// <summary>
	/// Represents a response from plaid's '/processor/token/create' endpoint. Create a processor_token.
	/// </summary>
	/// <seealso cref="Acklann.Plaid.ResponseBase" />
	public class ProcessorTokenResponse : ResponseBase
	{
		/// <summary>
		/// Gets or sets the processor token.
		/// </summary>
		/// <value>The processor token.</value>
		[JsonProperty("processor_token")]
		public string ProcessorToken { get; set; }

	}
}
