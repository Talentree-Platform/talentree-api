using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditFields(DbContext? context)
        {
            if (context == null) return;

            var currentUser = GetCurrentUserId();
            var currentTime = DateTime.UtcNow;

            // ═══════════════════════════════════════════════════════════
            // Handle AuditableEntity
            // ═══════════════════════════════════════════════════════════
            var auditableEntries = context.ChangeTracker
                .Entries<AuditableEntity>();

            foreach (var entry in auditableEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = currentTime;
                        entry.Entity.CreatedBy = currentUser;
                        break;

                    case EntityState.Modified:
                        // Prevent overwriting CreatedAt and CreatedBy
                        entry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
                        entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;

                        entry.Entity.UpdatedAt = currentTime;
                        entry.Entity.UpdatedBy = currentUser;
                        break;
                }
            }

            // ═══════════════════════════════════════════════════════════
            // Handle ISoftDelete (Soft Delete)
            // ═══════════════════════════════════════════════════════════
            var softDeleteEntries = context.ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Deleted &&
                           e.Entity is ISoftDelete);

            foreach (var entry in softDeleteEntries)
            {
                // Convert hard delete to soft delete
                entry.State = EntityState.Modified;

                var softDeleteEntity = (ISoftDelete)entry.Entity;
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedAt = currentTime;
                softDeleteEntity.DeletedBy = currentUser;

                // Also update UpdatedBy if entity is AuditableEntity
                if (entry.Entity is AuditableEntity auditableEntity)
                {
                    auditableEntity.UpdatedAt = currentTime;
                    auditableEntity.UpdatedBy = currentUser;
                }
            }
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}