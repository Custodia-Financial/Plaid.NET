﻿using Acklann.Plaid.Entity;
using Newtonsoft.Json;

namespace Acklann.Plaid.Management
{
	/// <summary>
	/// Represents a request for plaid's '<c>/processor/token/create</c>' endpoint. Exchange a Link <see cref="ExchangeTokenResponse.AccessToken"/> for a <see cref="ProcessorTokenResponse.ProcessorToken"/>.
	/// </summary>
	/// <seealso cref="RequestBase" />
	/// <seealso href="https://plaid.com/docs/api/processors/#processortokencreate"/>
	public class ProcessorTokenRequest : RequestBase
	{
		/// <summary>
		/// The <see cref="Account.AccountId"/> for which to generate the processor token
		/// </summary>
		[JsonProperty("account_id")]
		public string AccountId { get; set; } = string.Empty;

		/// <summary>
		/// The processor you are integrating with.
		/// </summary>
		[JsonProperty("processor")]
		public Processor Processor { get; set; }
	}
}
