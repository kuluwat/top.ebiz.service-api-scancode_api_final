
using System;
using System.Data;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace top.ebiz.service.Service.AzureAD
{
    public class GraphMicrosoftClient

    {
        private static HttpClient _HttpClient;
        private static GraphServiceClient GraphClient { get; set; }
        // Client configured with user authentication
        private static GraphServiceClient _userClient;
        private static string accessToken = "";
        private static GraphMicrosoftToken _GraphMicrosoftToken;
        private static List<GraphMicrosoftUser> userList = new List<GraphMicrosoftUser>();

        private static string ClientId = top.ebiz.helper.AppEnvironment.GetClientIDRoleRead() ?? "";
        private static string TenantId = top.ebiz.helper.AppEnvironment.GetTenantRoleRead() ?? "";
        private static string clientSecret = top.ebiz.helper.AppEnvironment.GetSecretRoleRead() ?? "";
        private static string GraphUserScopes = top.ebiz.helper.AppEnvironment.GetGraphUserScopes() ?? "";

        public static string InitializeGraphForUserAuth()
        {
            _HttpClient = new HttpClient();

            try
            {

                if (TenantId != null && ClientId != null && clientSecret != null)
                {
                    GraphClient = CreateGraphClient(TenantId, ClientId, clientSecret);
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
            return "";
        }

        public static string InitializeGraph()
        {
            return InitializeGraphForUserAuth();
        }
        public static List<GraphMicrosoftUser> GetUsers()
        {
            return userList;
        }
        private static GraphServiceClient CreateGraphClient(string tenantId, string clientId, string clientSecret)
        {
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,

            };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var scopes = new[] { "https://graph.microsoft.com/.default" };


            return new GraphServiceClient(clientSecretCredential, scopes);
        }

        private static void GetAccessToken()
        {

            if (!Guid.TryParse(TenantId, out Guid tenantGuid))
            {
                throw new ArgumentException("Invalid TenantId format.");
            }
            UriBuilder uriBuilder = new UriBuilder(GraphUserScopes);
            uriBuilder.Path = $"{tenantGuid}/oauth2/v2.0/token";
            string authString = uriBuilder.Uri.ToString();

            var content = new FormUrlEncodedContent(new[]
      {
         new KeyValuePair<string, string>("grant_type", "client_credentials"),
         new KeyValuePair<string, string>("client_id", ClientId),
         new KeyValuePair<string, string>("scope", GraphUserScopes),
         new KeyValuePair<string, string>("client_secret", clientSecret),
     });
            if (authString != null)
            {
                var result = _HttpClient.PostAsync(authString, content).Result;
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = result.Content.ReadAsStringAsync().Result;
                    GraphMicrosoftToken resultjson = JsonConvert.DeserializeObject<GraphMicrosoftToken>(resultContent);
                    _GraphMicrosoftToken = resultjson ?? null;
                    _GraphMicrosoftToken?.setExpireTime(resultjson?.expires_in);
                    accessToken = resultjson?.access_token ?? "";
                }
            }


        }
        public static string callBackNextLink(string OdataNextLink)
        {
            string nextLink = null;

            UserCollectionResponse users = GraphClient.Users.WithUrl(OdataNextLink).GetAsync((requestConfiguration) =>
            {
                requestConfiguration.Headers.Add("authorization", $"Bearer " + accessToken);
                requestConfiguration.Headers.Add("ConsistencyLevel", $"eventual");
            }).Result;
            if (users != null && users.Value != null)
            {
                // per 999
                foreach (var item in users.Value)
                {

                    userList.Add(new GraphMicrosoftUser(item));
                }
            }
            nextLink = null;
            if (users?.OdataNextLink != null)
            {
                nextLink = users?.OdataNextLink;
            }

            return nextLink;
        }
        public static void getAllUsers()
        {
            userList.Clear();
            var isExpire = _GraphMicrosoftToken?.isExpire() ?? true;
            string nextLink = null;
            try
            {
                if (isExpire)
                {
                    GetAccessToken();
                }

                UserCollectionResponse users = GraphClient.Users.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Filter = "endsWith(userPrincipalName,'@thaioilgroup.com')";
                    requestConfiguration.QueryParameters.Top = 999;
                    requestConfiguration.QueryParameters.Count = true;
                    requestConfiguration.Headers.Add("authorization", $"Bearer " + accessToken);
                    requestConfiguration.Headers.Add("ConsistencyLevel", $"eventual");
                }).Result;
                if (users != null && users.OdataCount > 0 && users.Value != null)
                {
                    // per 999
                    foreach (var item in users.Value)
                    {

                        userList.Add(new GraphMicrosoftUser(item));
                    }
                }
                if (users?.OdataNextLink != null)
                {
                    nextLink = users?.OdataNextLink;
                    while (nextLink != null)
                    {
                        nextLink = callBackNextLink(nextLink);
                    }
                }


                Console.WriteLine($"Total users: {users.Value.Count}");
            }
            catch (ServiceException ex)
            {
                nextLink = null;
                Console.WriteLine($"Error getting user details: {ex.Message}");
            }
        }
        public static async Task<GraphMicrosoftUser> GetUser(string email)
        {
            var isExpire = _GraphMicrosoftToken?.isExpire() ?? true;
            List<GraphMicrosoftUser> user = new List<GraphMicrosoftUser>();
            string nextLink = null;
            try
            {

                if (isExpire)
                {
                    GetAccessToken();
                }
                if (!(email.IndexOf("@") > -1)) { email += "@thaioilgroup.com"; }
                UserCollectionResponse users = await GraphClient.Users.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Filter = $"userPrincipalName eq '{email}'";
                    requestConfiguration.QueryParameters.Top = 999;
                    requestConfiguration.QueryParameters.Count = true;
                    requestConfiguration.Headers.Add("authorization", $"Bearer " + accessToken);
                    requestConfiguration.Headers.Add("ConsistencyLevel", $"eventual");
                });
                if (users != null && users.OdataCount > 0 && users.Value != null)
                {
                    // per 999
                    foreach (var item in users.Value)
                    {

                        user.Add(new GraphMicrosoftUser(item));
                    }
                }


            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting user details: {ex.Message}");
            }
            return user.FirstOrDefault() ?? null;
        }
        public static string ListhUserAllUsers()
        {
            getAllUsers();
            var users = userList;

            return users.ToString();

        }

    }
}
