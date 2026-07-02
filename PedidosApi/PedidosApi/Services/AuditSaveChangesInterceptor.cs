using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PedidosApi.Models;

namespace PedidosApi.Services
{
    public class AuditSaveChangesInterceptor : ISaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return new ValueTask<InterceptionResult<int>>(result);

            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added
                         || e.State == EntityState.Modified
                         || e.State == EntityState.Deleted)
                .Where(e => e.Entity.GetType().Name != nameof(AuditLog))
                .ToList();

            foreach (var entry in entries)
            {
                var entityName = entry.Entity.GetType().Name;
                var action = entry.State.ToString();
                string? oldValues = null;
                string? newValues = null;
                string? entityId = null;

                if (entry.State == EntityState.Modified)
                {
                    var old = new Dictionary<string, object?>();
                    var @new = new Dictionary<string, object?>();

                    foreach (var prop in entry.Properties)
                    {
                        if (prop.IsModified)
                        {
                            old[prop.Metadata.Name] = prop.OriginalValue;
                            @new[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }

                    if (old.Count > 0)
                    {
                        oldValues = JsonSerializer.Serialize(old);
                        newValues = JsonSerializer.Serialize(@new);
                    }
                }
                else if (entry.State == EntityState.Added)
                {
                    var @new = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                    {
                        @new[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    newValues = JsonSerializer.Serialize(@new);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    var old = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                    {
                        old[prop.Metadata.Name] = prop.OriginalValue;
                    }
                    oldValues = JsonSerializer.Serialize(old);
                }

                var key = entry.Metadata.FindPrimaryKey();
                if (key != null)
                {
                    var keyValues = key.Properties
                        .Select(p => entry.Property(p.Name).CurrentValue?.ToString())
                        .Where(v => v != null);
                    entityId = string.Join(",", keyValues);
                }

                var auditLog = new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId ?? "0",
                    Action = action,
                    OldValues = oldValues,
                    NewValues = newValues,
                    UserName = userName,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow
                };

                context.Set<AuditLog>().Add(auditLog);
            }

            return new ValueTask<InterceptionResult<int>>(result);
        }

        public int? Order => 0;
    }
}
