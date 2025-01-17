﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Acklann.Plaid
{
	/// <summary>
	/// Provides methods for sending request to and receiving data from Plaid banking API.
	/// </summary>
	public class PlaidClient
	{
		private static readonly HttpClient Client = new HttpClient();
		/// <summary>
		/// Initializes a new instance of the <see cref="PlaidClient"/> class.
		/// </summary>
		public PlaidClient() : this(null, null, null, Plaid.Environment.Production)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlaidClient"/> class.
		/// </summary>
		/// <param name="environment">The environment.</param>
		public PlaidClient(Plaid.Environment environment) : this(null, null, null, environment)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlaidClient"/> class.
		/// </summary>
		/// <param name="options">The options.</param>
		/// <param name="logger">The logger.</param>
		public PlaidClient(PlaidOption options, ILogger logger) :
			this(options?.ClientId, options?.Secrets, options?.AccessToken, options.EnvironmentName, options?.Version, logger)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlaidClient" /> class.
		/// </summary>
		/// <param name="clientId">The client identifier.</param>
		/// <param name="secret">The secret.</param>
		/// <param name="accessToken">The access token.</param>
		/// <param name="environment">The environment.</param>
		/// <param name="apiVersion">The Plaid API version.</param>
		/// <param name="logger">The logger.</param>
		public PlaidClient(string clientId,
						   string secret,
						   string accessToken,
						   Plaid.Environment environment = Plaid.Environment.Production,
						   string apiVersion = "2020-09-14",
						   ILogger logger = default)
		{
			_secret = secret;
			_clientId = clientId;
			_accessToken = accessToken;
			_environment = environment;
			_apiVersion = apiVersion;
			_logger = logger;

			string subDomain = _environment switch
			{
				Environment.Production => "production",
				Environment.Development => "development",
				Environment.Sandbox => "sandbox",
				_ => throw new System.NotImplementedException()
			};

			_baseUrl = $"https://{subDomain}.plaid.com";
		}

		/* Transactions */

		/// <summary>
		/// Fetches the transactions asynchronous.
		/// </summary>
		/// <param name="request">The request.</param>
		public Task<Transactions.GetTransactionsResponse> FetchTransactionsAsync(Transactions.GetTransactionsRequest request)
		{
			return PostAsync<Transactions.GetTransactionsResponse>("/transactions/get", request);
		}

		/// <summary>
		///  Initiates an on-demand extraction to fetch the newest transactions for an <see cref="Entity.Item"/>.
		/// </summary>
		/// <param name="request">The request.</param>
		public Task<Transactions.RefreshTransactionResponse> RefreshTransactionsAsync(Transactions.RefreshTransactionRequest request)
		{
			return PostAsync<Transactions.RefreshTransactionResponse>("/transactions/refresh", request);
		}

		/* Item Management */

		/// <summary>
		/// Retrieves information about the status of an <see cref="Entity.Item"/>. Endpoint '/item/get'.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Management.GetItemResponse&gt;.</returns>
		public Task<Item.GetItemResponse> FetchItemAsync(Item.GetItemRequest request)
		{
			return PostAsync<Item.GetItemResponse>("/item/get", request);
		}

		/// <summary>
		/// Delete an <see cref="Entity.Item"/>. Once deleted, the access_token associated with the <see cref="Entity.Item"/> is no longer valid and cannot be used to access any data that was associated with the <see cref="Entity.Item"/>.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Management.DeleteItemResponse&gt;.</returns>
		public Task<Item.RemoveItemResponse> DeleteItemAsync(Item.RemoveItemRequest request)
		{
			return PostAsync<Item.RemoveItemResponse>("/item/remove", request);
		}

		/// <summary>
		/// Updates the webhook associated with an <see cref="Entity.Item"/>. This request triggers a WEBHOOK_UPDATE_ACKNOWLEDGED webhook.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Management.UpdateWebhookResponse&gt;.</returns>
		public Task<Item.UpdateWebhookResponse> UpdateWebhookAsync(Item.UpdateWebhookRequest request)
		{
			return PostAsync<Item.UpdateWebhookResponse>("/item/webhook/update", request);
		}

		/* Token */

		// TODO: /link/token/get
		// TODO: /item/access_token/invalidate

		/// <summary>
		/// Exchanges a Link public_token for an API access_token.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Management.ExchangeTokenResponse&gt;.</returns>
		public Task<Management.CreatePublicTokenResponse> CreatePublicTokenAsync(Management.CreatePublicTokenRequest request)
		{
			return PostAsync<Management.CreatePublicTokenResponse>("/item/public_token/create", request);
		}

		/// <summary>
		/// Creates a Link link_token.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public Task<Management.CreateLinkTokenResponse> CreateLinkToken(Management.CreateLinkTokenRequest request)
		{
			return PostAsync<Management.CreateLinkTokenResponse>("/link/token/create", request);
		}

		/// <summary>
		/// Exchanges a Link public_token for an API access_token.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Management.ExchangeTokenResponse&gt;.</returns>
		public Task<Management.ExchangeTokenResponse> ExchangeTokenAsync(Management.ExchangeTokenRequest request)
		{
			return PostAsync<Management.ExchangeTokenResponse>("/item/public_token/exchange", request);
		}

		/// <summary>
		/// Rotates the access_token associated with an <see cref="Entity.Item"/>. The endpoint returns a new access_token and immediately invalidates the previous access_token.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Management.RotateAccessTokenResponse&gt;.</returns>
		public Task<Management.RotateAccessTokenResponse> RotateAccessTokenAsync(Management.RotateAccessTokenRequest request)
		{
			return PostAsync<Management.RotateAccessTokenResponse>("/item/access_token/invalidate", request);
		}

		/// <summary>
		/// Updates an access_token from the legacy version of Plaid’s API, you can use method to generate an access_token for the Item that works with the current API.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Management.UpdateAccessTokenVersionResponse&gt;.</returns>
		public Task<Management.UpdateAccessTokenVersionResponse> UpdateAccessTokenVersion(Management.UpdateAccessTokenVersionRequest request)
		{
			return PostAsync<Management.UpdateAccessTokenVersionResponse>("/item/access_token/update_version", request);
		}

		/* Institutions */

		/// <summary>
		/// Retrieves the details on all financial institutions currently supported by Plaid.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Institution.SearchResponse&gt;.</returns>
		public Task<Institution.SearchResponse> FetchAllInstitutionsAsync(Institution.SearchAllRequest request)
		{
			return PostAsync<Institution.SearchResponse>("/institutions/get", request);
		}

		/// <summary>
		/// Retrieves the institutions that match the query parameters.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Institution.SearchResponse&gt;.</returns>
		public Task<Institution.SearchResponse> FetchInstitutionsAsync(Institution.SearchRequest request)
		{
			return PostAsync<Institution.SearchResponse>("/institutions/search", request);
		}

		/// <summary>
		/// Retrieves the institutions that match the id.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Institution.SearchByIdResponse&gt;.</returns>
		public Task<Institution.SearchByIdResponse> FetchInstitutionByIdAsync(Institution.SearchByIdRequest request)
		{
			return PostAsync<Institution.SearchByIdResponse>("/institutions/get_by_id", request);
		}

		/* Income */

		/// <summary>
		/// Retrieves information pertaining to a <see cref="Entity.Item"/>’s income. In addition to the annual income, detailed information will be provided for each contributing income stream (or job).
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Income.GetIncomeResponse&gt;.</returns>
		public Task<Income.GetIncomeResponse> FetchUserIncomeAsync(Income.GetIncomeRequest request)
		{
			return PostAsync<Income.GetIncomeResponse>("/income/get", request);
		}

		/* Investments */

		/// <summary>
		/// Retrieves information pertaining to a <see cref="Entity.Item"/>'s investment holdings.
		/// </summary>
		public Task<Investments.GetInvestmentHoldingsResponse> FetchInvestmentHoldingsAsync(Investments.GetInvestmentHoldingsRequest request)
		{
			return PostAsync<Investments.GetInvestmentHoldingsResponse>("/investments/holdings/get", request);
		}

		/// <summary>
		/// Retrieves information pertaining to a <see cref="Entity.Item"/>'s investment transactions.
		/// </summary>
		public Task<Investments.GetInvestmentTransactionsResponse> FetchInvestmentTransactionsAsync(Investments.GetInvestmentTransactionsRequest request)
		{
			return PostAsync<Investments.GetInvestmentTransactionsResponse>("/investments/transactions/get", request);
		}

		/* Auth */

		/// <summary>
		/// Retrieves the bank account and routing numbers associated with an <see cref="Entity.Item"/>’s checking and savings accounts, along with high-level account data and balances.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Auth.GetAccountInfoResponse&gt;.</returns>
		public Task<Auth.GetAccountInfoResponse> FetchAccountInfoAsync(Auth.GetAccountInfoRequest request)
		{
			return PostAsync<Auth.GetAccountInfoResponse>("/auth/get", request);
		}

		/* Balance */

		/// <summary>
		/// Retrieve high-level information about all accounts associated with an <see cref="Entity.Item"/>.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Balance.GetAccountResponse&gt;.</returns>
		public Task<Accounts.GetAccountResponse> FetchAccountAsync(Accounts.GetAccountRequest request)
		{
			return PostAsync<Accounts.GetAccountResponse>("/accounts/get", request);
		}

		/// <summary>
		///  Retrieves the real-time balance for each of an <see cref="Entity.Item"/>’s accounts.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Balance.GetBalanceResponse&gt;.</returns>
		public Task<Balance.GetBalanceResponse> FetchAccountBalanceAsync(Balance.GetBalanceRequest request)
		{
			return PostAsync<Balance.GetBalanceResponse>("/accounts/balance/get", request);
		}

		/* Categories */

		/// <summary>
		///  Retrieves detailed information on categories returned by Plaid.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Category.GetCategoriesResponse&gt;.</returns>
		public Task<Category.GetCategoriesResponse> FetchCategoriesAsync(Category.GetCategoriesRequest request)
		{
			return PostAsync<Category.GetCategoriesResponse>("/categories/get", request);
		}

		/* Identity */

		/// <summary>
		/// Retrieves various account holder information on file with the financial institution, including names, emails, phone numbers, and addresses.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Identity.GetUserIdentityResponse&gt;.</returns>
		public Task<Identity.GetUserIdentityResponse> FetchUserIdentityAsync(Identity.GetUserIdentityRequest request)
		{
			return PostAsync<Identity.GetUserIdentityResponse>("/identity/get", request);
		}

		/// <summary>
		/// Gets a libabilities response
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Liabilities.GetLiabilitiesResponse&gt;.</returns>
		public Task<Liabilities.GetLiabilitiesResponse> FetchLiabilitiesAsync(Liabilities.GetLiabilitiesRequest request)
		{
			return PostAsync<Liabilities.GetLiabilitiesResponse>("/liabilities/get", request);
		}

		/* ***** */

		/* Stripe */

		/// <summary>
		///  Exchanges a Link access_token for an Stripe API stripe_bank_account_token.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Management.StripeTokenResponse&gt;.</returns>
		public Task<Management.StripeTokenResponse> FetchStripeTokenAsync(Management.StripeTokenRequest request)
		{
			return PostAsync<Management.StripeTokenResponse>("/processor/stripe/bank_account_token/create", request);
		}

		/* ***** */

		/// <summary>
		///  Exchanges a Link access_token for an Stripe API stripe_bank_account_token.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>Task&lt;Management.StripeTokenResponse&gt;.</returns>
		public Task<Management.ProcessorTokenResponse> CreateProcessorToken(Management.ProcessorTokenRequest request)
		{
			return PostAsync<Management.ProcessorTokenResponse>("/processor/token/create", request);
		}

		internal async Task<TResponse> PostAsync<TResponse>(string path, SerializableContent request) where TResponse : ResponseBase, new()
		{
			SetCredentials(request);

			string endpoint = GetEndpoint(path);
			string requestData = request.ToJson();

			HttpContent body = new StringContent(requestData, Encoding.UTF8, "application/json");
			body.Headers.Add("Plaid-Version", _apiVersion);

			WriteToDebugger(requestData, $"POST: '{endpoint}'");
			_logger?.LogTrace("Sent http request. POST: {0}\r\n{1}", endpoint, requestData);

			using (HttpResponseMessage response = await Client.PostAsync(endpoint, body))
			{
#if DEBUG
				requestData = await response.Content.ReadAsStringAsync();
				WriteToDebugger(requestData, $"RESPONSE ({response.StatusCode})");
#endif
				_logger?.LogTrace("Received response ({1}) from 'POST: {0}'", endpoint, (int)response.StatusCode);

				return await CreateResponse<TResponse>(response);
			}
		}

		#region Backing Members

		private string GetEndpoint(string path)
		{
			return string.Concat(_baseUrl, path);
		}

		private readonly string _baseUrl;
		private readonly ILogger _logger;
		private readonly Plaid.Environment _environment;
		private readonly HttpClient _httpClient;
		private readonly string _clientId, _secret, _accessToken, _apiVersion;

		private readonly JsonSerializer _serializer = new JsonSerializer
		{
			NullValueHandling = NullValueHandling.Ignore,
			Converters = { new Exceptions.EnumMemberEnumConverter() }
		};

		private async Task<TResponse> CreateResponse<TResponse>(HttpResponseMessage response) where TResponse : ResponseBase, new()
		{
			using (Stream stream = await response.Content.ReadAsStreamAsync())
			using (var reader = new StreamReader(stream))
			using (var jsonReader = new JsonTextReader(reader))
			{
				if (response.IsSuccessStatusCode)
				{
					TResponse result = _serializer.Deserialize<TResponse>(jsonReader);
					result.StatusCode = response.StatusCode;
					return result;
				}
				else
				{
					return new TResponse
					{
						StatusCode = response.StatusCode,
						Exception = _serializer.Deserialize<Exceptions.PlaidException>(jsonReader)
					};
				}
			}
		}

		private void SetCredentials(object request)
		{
			if (request is AuthorizedRequestBase areq)
			{
				if (string.IsNullOrEmpty(areq.Secret)) areq.Secret = _secret;
				if (string.IsNullOrEmpty(areq.ClientId)) areq.ClientId = _clientId;
				if (string.IsNullOrEmpty(areq.AccessToken)) areq.AccessToken = _accessToken;
			}
			else if (request is RequestBase req)
			{
				if (string.IsNullOrEmpty(req.Secret)) req.Secret = _secret;
				if (string.IsNullOrEmpty(req.ClientId)) req.ClientId = _clientId;
			}
		}

		private void WriteToDebugger(string message, string title)
		{
#if DEBUG
			string line = string.Concat(System.Linq.Enumerable.Repeat('-', 100));
			int n = (title.Length > line.Length) ? line.Length : (line.Length - title.Length + 2);
			string format(string x) => line.Substring(0, n).Insert(5, $" {x} ");

			System.Diagnostics.Debug.WriteLine(format(title));
			System.Diagnostics.Debug.WriteLine(message);
#endif
		}

		#endregion Backing Members
	}
}
