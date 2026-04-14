namespace Identity.Clients;

using Identity.Interfaces;
using System.Net.Http.Json;


public class GitHubIdentityClient(IHttpClientFactory httpClientFactory) : IGitHubIdentityClient
{
    public async Task<List<GitHubUserItem>> SearchUsersAsync(string query)
    {
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ReviewProxy");
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10"); // Match original logic

        var response = await client.GetAsync($"https://api.github.com/search/users?q={query}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<GitHubSearchResponseInternal>();
            return content?.Items ?? [];
        }

        return [];
    }

    private class GitHubSearchResponseInternal
    {
        public List<GitHubUserItem> Items { get; set; } = [];
    }
}
