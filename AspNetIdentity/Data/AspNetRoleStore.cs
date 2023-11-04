using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SqlMembershipEntityModel.Context;
using SqlMembershipEntityModel.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetIdentity.Data
{
    public sealed class AspNetRoleStore : AspNetStoreBase, IRoleStore<ApplicationRole>
    {
        public AspNetRoleStore(IDbContextFactory<AspNetIdentityModel> idenityModelFactory, IConfiguration configuration) : base(configuration)
        {
            IdentityModelFactory = idenityModelFactory;
        }

        public void Dispose()
        {
        }

        private IDbContextFactory<AspNetIdentityModel> IdentityModelFactory { get; }

        public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            ParameterValidation(role, cancellationToken);

            var membershipRole = new AspNetRole()
            {
                ApplicationId = role.ApplicationId,
                RoleId = role.Id,
                RoleName = role.Name,
                LoweredRoleName = role.NormalizedName,
                Description = role.Description
            };

            using var context = IdentityModelFactory.CreateDbContext();
            _ = await context
                .AspNetRoles
                .AddAsync(membershipRole, cancellationToken);

            _ = await context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            ParameterValidation(role, cancellationToken);

            using var context = IdentityModelFactory.CreateDbContext();

            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                context
                    .ChangeTracker
                    .Entries<AspNetUsersInRole>()
                    .Where(e => e.Entity.RoleId == role.Id)
                    .ToList()
                    .ForEach(entry => entry.State = EntityState.Detached);

                var userInRoles = context.AspNetUsersInRoles.Where(uir => uir.RoleId == role.Id);
                if (await userInRoles.AnyAsync(cancellationToken))
                {
                    context.AspNetUsersInRoles.RemoveRange(userInRoles);
                }

                var userRole =
                    context.ChangeTracker.Entries<AspNetRole>().SingleOrDefault(entry => entry.Entity.RoleId == role.Id)?.Entity ??
                    context.AspNetRoles.SingleOrDefault(r => r.RoleId == role.Id);

                if (userRole != null)
                {
                    _ = context.AspNetRoles.Remove(userRole);
                }

                _ = await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
            }
            return IdentityResult.Success;
        }

        public async Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(roleId))
            {
                throw new ArgumentNullException(nameof(roleId));
            }
            if (!Guid.TryParse(roleId, out var id))
            {
                throw new ArgumentException("Not a valid (GUID) id", nameof(roleId));
            }

            using var context = IdentityModelFactory.CreateDbContext();
            var role = await context
                .AspNetRoles
                .SingleOrDefaultAsync(record => record.RoleId == id, cancellationToken);

            return TransferDbToRole(role);
        }

        public async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            normalizedRoleName = normalizedRoleName.ToLower();

            using var context = IdentityModelFactory.CreateDbContext();
            var role = await context
                .AspNetRoles
                .SingleOrDefaultAsync(record =>
                    record.ApplicationId == ApplicationId &&
                    record.LoweredRoleName == normalizedRoleName,
                    cancellationToken);

            return TransferDbToRole(role);
        }

        public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            ParameterValidation(role, cancellationToken);
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            ParameterValidation(role, cancellationToken);
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            ParameterValidation(role, cancellationToken);
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            ParameterValidation(role, cancellationToken);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                throw new ArgumentException("Invalid parameter value.", nameof(normalizedName));
            }
            role.NormalizedName = normalizedName.ToLower();
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            ParameterValidation(role, cancellationToken);
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Invalid parameter value.", nameof(roleName));
            }
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            ParameterValidation(role, cancellationToken);
            using var context = IdentityModelFactory.CreateDbContext();

            // Manage entity tracking (if enabled)
            var ur = context.ChangeTracker.Entries<AspNetRole>().FirstOrDefault(entry => entry.Entity.RoleId == role.Id);
            if ((ur?.State ?? EntityState.Detached) != EntityState.Detached)
            {
                ur.State = EntityState.Detached;
            }

            // Transform data
            TransferRoleToDb(role, out var roleRecord);

            // Apply update to database
            context.AspNetRoles.Update(roleRecord);

            // Commit the updates to the database
            _ = await context.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }

        private static void ParameterValidation(ApplicationRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (role.ApplicationId == Guid.Empty)
            {
                throw new Exception($"Bad value: {nameof(role.ApplicationId)}");
            }
            if (role.Id == Guid.Empty)
            {
                throw new Exception($"Bad value: {nameof(role.Id)}");
            }
            if (string.IsNullOrWhiteSpace(role.Name))
            {
                throw new Exception($"Bad value: {nameof(role.Name)}");
            }
        }

        private static ApplicationRole TransferDbToRole(AspNetRole data)
        {
            return (data == null)
                ? null
                : new ApplicationRole()
                {
                    ApplicationId = data.ApplicationId,
                    Id = data.RoleId,
                    Name = data.RoleName,
                    NormalizedName = data.LoweredRoleName,
                    Description = data.Description
                };
        }

        private static void TransferRoleToDb(ApplicationRole role, out AspNetRole roleRecord)
        {
            roleRecord = new AspNetRole()
            {
                ApplicationId = role.ApplicationId,
                RoleId = role.Id,
                RoleName = role.Name,
                LoweredRoleName = role.NormalizedName,
                Description = role.Description
            };
        }
    }
}