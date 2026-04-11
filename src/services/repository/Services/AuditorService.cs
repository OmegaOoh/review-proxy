using Repository.Data;
using Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using ReviewProxy.Contracts;
using System.Net.Http.Json;

namespace Repository.Services;

public class AuditorService(
    RepoDbContext dbContext,
    IRepositoryEventPublisher eventPublisher,
    IHttpClientFactory httpClientFactory) : IAuditorService
{
    public async Task AddAuditorsAsync(Guid repoId, List<Guid> userId)
    {
        var repo = await dbContext.Repositories.FindAsync(repoId);
        if (repo == null) return;

        foreach (var id in userId)
        {
            var idStr = id.ToString();
            // Do not add owner to auditors list
            if (repo.OwnerId != idStr && !repo.AuditorsIds.Contains(idStr))
            {
                repo.AuditorsIds.Add(idStr);
            }
        }

        var saved = await dbContext.SaveChangesAsync();
        if (saved > 0)
        {
            await eventPublisher.PublishAuditorListAsync(repoId);
        }
    }

    public async Task<List<Guid>> GetAuditorsAsync(Guid repoId)
    {
        var repo = await dbContext.Repositories.FindAsync(repoId);
        if (repo == null) return [];

        var auditors = repo.AuditorsIds.Select(Guid.Parse).ToList();
        if (Guid.TryParse(repo.OwnerId, out var ownerId) && !auditors.Contains(ownerId))
        {
            auditors.Add(ownerId);
        }

        return auditors;
    }

    public async Task<List<object>> GetAuditorsDetailsAsync(Guid repoId, string? authorizationHeader)
    {
        var ids = await GetAuditorsAsync(repoId);
        if (ids.Count == 0) return [];

        var client = httpClientFactory.CreateClient("identity");
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        try
        {
            var response = await client.PostAsJsonAsync("/api/identities/batch", ids);
            if (response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadFromJsonAsync<List<object>>();
                return details ?? [];
            }
        }
        catch
        {
            // Fallback to empty if batch fails
        }

        return [];
    }

    public async Task RemoveAuditorsAsync(Guid repoId, List<Guid> userId)
    {
        var repo = await dbContext.Repositories.FindAsync(repoId);
        if (repo == null) return;

        foreach (var id in userId)
        {
            var idStr = id.ToString();
            // Owner is not eligible to remove themselves
            if (repo.OwnerId == idStr) continue;

            repo.AuditorsIds.Remove(idStr);
        }

        var saved = await dbContext.SaveChangesAsync();
        if (saved > 0)
        {
            await eventPublisher.PublishAuditorListAsync(repoId);
        }
    }
}
