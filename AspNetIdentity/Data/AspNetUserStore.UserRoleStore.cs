using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SqlMembershipEntityModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetIdentity.Data
{
    public partial class AspNetUserStore : IUserRoleStore<ApplicationUser>
    {
        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            UserRoleParameterValidation(user, roleName, cancellationToken);

            var isInRole = await IsInRoleAsync(user, roleName, cancellationToken);
            if (isInRole)
            {
                return;
            }

            var rollMapping = new AspNetUsersInRole()
            {
                RoleId = await GetRoleIdByRoleNameAsync(roleName),
                UserId = user.Id
            };

            using var context = IdentityModelFactory.CreateDbContext();

            _ = await context.AspNetUsersInRoles.AddAsync(rollMapping, cancellationToken);
            _ = await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);

            using var context = IdentityModelFactory.CreateDbContext();
            var roles = await context
                .AspNetUsersInRoles
                .Include(record => record.Role)
                .Where(record => record.UserId == user.Id)
                .Select(record => record.Role.RoleName)
                .ToListAsync(cancellationToken);

            return roles;
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Invalid parameter value.", nameof(roleName));
            }

            using var context = IdentityModelFactory.CreateDbContext();

            var users = await context
                .AspNetUsersInRoles
                .Include(record => record.User)
                .ThenInclude(record => record.AspNetMembership)
                .ThenInclude(record => record.User)
                .Where(record => record.Role.RoleName == roleName)

                // Filter out legacy corruption (i.e. User must exist in the aspnet_Membership table)
                .Where(record => record.User.AspNetMembership.User != null)

                // Required for cyclical query
                .AsTracking(QueryTrackingBehavior.TrackAll)

                .Select(record => TransferDbToUser(record.User.AspNetMembership))
                .ToListAsync(cancellationToken);

            return users;
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            UserRoleParameterValidation(user, roleName, cancellationToken);

            using var context = IdentityModelFactory.CreateDbContext();

            var hasRole = await context
                .AspNetUsersInRoles
                .Include(record => record.Role)
                .Where(record => record.UserId == user.Id)
                .AnyAsync(record => record.Role.RoleName == roleName, cancellationToken);

            return hasRole;
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            UserRoleParameterValidation(user, roleName, cancellationToken);
            var isInRole = await IsInRoleAsync(user, roleName, cancellationToken);
            if (!isInRole)
            {
                return;
            }

            var roleId = await GetRoleIdByRoleNameAsync(roleName);

            using var context = IdentityModelFactory.CreateDbContext();

            var rollMapping =
                context
                    .ChangeTracker
                    .Entries<AspNetUsersInRole>()
                    .FirstOrDefault(e => e.Entity.UserId == user.Id && e.Entity.RoleId == roleId)?.Entity ??
                context
                    .AspNetUsersInRoles
                    .SingleOrDefault(m => m.UserId == user.Id && m.RoleId == roleId);

            _ = context.AspNetUsersInRoles.Remove(rollMapping);

            _ = await context.SaveChangesAsync(cancellationToken);
        }

        private async Task<Guid> GetRoleIdByRoleNameAsync(string roleName)
        {
            using var context = IdentityModelFactory.CreateDbContext();

            var role = await context
                .AspNetRoles
                .SingleAsync(record =>
                    record.ApplicationId == ApplicationId &&
                    record.LoweredRoleName == roleName.ToLowerInvariant());

            return role.RoleId;
        }

        private static void UserRoleParameterValidation(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
        {
            ParameterValidation(user, cancellationToken);
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Invalid parameter value.", nameof(roleName));
            }
        }
    }
}