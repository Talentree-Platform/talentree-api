// Talentree.Repository/Data/Interceptors/AuditInterceptor.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Talentree.Core.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Talentree.Repository.Data.Interceptors
{
    /// <summary>
    /// Automatically populates audit fields (CreatedAt, CreatedBy, etc.) for all BaseEntity instances
    /// </summary>
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

            var entries = context.ChangeTracker.Entries<BaseEntity>();
            var currentUser = GetCurrentUserId();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = currentUser;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        // Prevent overwriting CreatedAt and CreatedBy
                        entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                        entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;

                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = currentUser;
                        break;

                    case EntityState.Deleted:
                        // Soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = currentUser;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets current authenticated user ID from JWT token
        /// </summary>
        private string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return userId ?? "System";
        }
    }
}