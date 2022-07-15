using Newtonsoft.Json;

namespace Acklann.Plaid.Management
{
	/// <summary>
	/// Represents a request for plaid's '/processor/token/create' endpoint. Exchange a Link access_token for an account_id and access_token.
	/// </summary>
	/// <seealso cref="Acklann.Plaid.SerializableContent" />
	public class ProcessorTokenRequest : AuthorizedRequestBase
	{
		/// <summary>
		/// Gets or sets the account id.
		/// </summary>
		/// <value>The account id.</value>
		[JsonProperty("account_id")]
		public string AccountId { get; set; }

		/// <summary>
		/// Gets or sets the processor.
		/// </summary>
		/// <value>The processor.</value>
		[JsonProperty("processor")]
		public string Processor { get; set; }
	}
}
