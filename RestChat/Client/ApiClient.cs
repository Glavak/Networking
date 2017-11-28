using Model.HandlersData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    public class ApiClient
    {
        private readonly HttpClient client = new HttpClient();

        private readonly Uri loginUrl;
        private readonly Uri logoutUrl;
        private readonly Uri usersUrl;
        private readonly Uri messagesUrl;

        private Guid token;
        private bool isLoggedIn;

        public bool IsLoggedIn => isLoggedIn;

        public ApiClient()
        {
            var baseUrl = new Uri("http://localhost:4242/");

            loginUrl = new Uri(baseUrl, "login");
            logoutUrl = new Uri(baseUrl, "logout");
            usersUrl = new Uri(baseUrl, "users");
            messagesUrl = new Uri(baseUrl, "messages");
        }

        public async Task LoginAsync(string username)
        {
            LoginRequest request = new LoginRequest
            {
                Username = username
            };

            var response = await PostAsync<LoginRequest, LoginResponse>(loginUrl, request).ConfigureAwait(false);

            this.token = response.Token;
            this.isLoggedIn = true;

            client.DefaultRequestHeaders.Add("Authorization", token.ToString());
        }

        public async Task LogoutAsync()
        {
            var response = await GetAsync<EmptyData>(logoutUrl).ConfigureAwait(false);

            this.isLoggedIn = false;

            client.DefaultRequestHeaders.Clear();
        }

        public async Task<List<UserDetailsResponse>> GetUsersAsync()
        {
            var response = await GetAsync<UserListResponse>(usersUrl).ConfigureAwait(false);

            return response.Users;
        }

        public async Task<List<PostMessageResponse>> GetMessagesAsync(int offset, int count = 10)
        {
            Uri url = new Uri(messagesUrl, $"?offset={offset}&count={count}");

            var response = await GetAsync<GetMessagesResponse>(url).ConfigureAwait(false);

            return response.Messages;
        }

        public async Task PostMessageAsync(string text)
        {
            PostMessageRequest request = new PostMessageRequest
            {
                Message = text
            };

            var response = await PostAsync<PostMessageRequest, PostMessageResponse>(messagesUrl, request).ConfigureAwait(false);
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(Uri uri, TRequest request)
        {
            string json = JsonConvert.SerializeObject(request);

            var response = await client.PostAsync(uri, new StringContent(json)).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && response.Headers.WwwAuthenticate.Count > 0)
            {
                throw new LoginTakenException();
            }
            else if(!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.StatusCode);
            }

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseObject = JsonConvert.DeserializeObject<TResponse>(responseString);

            return responseObject;
        }

        private async Task<TResponse> GetAsync<TResponse>(Uri uri)
        {
            var response = await client.GetAsync(uri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.StatusCode);
            }

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseObject = JsonConvert.DeserializeObject<TResponse>(responseString);

            return responseObject;
        }
    }
}
