using AspNetIdentity.Extensions;
using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SqlMembershipEntityModel.Context;
using SqlMembershipEntityModel.Models;
using System;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetIdentity.Data
{
    // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-custom-storage-providers?view=aspnetcore-5.0
    public partial class AspNetUserStore : AspNetStoreBase, IUserStore<ApplicationUser>
    {
        public AspNetUserStore(IDbContextFactory<AspNetIdentityModel> idenityModelFactory, IConfiguration configuration)
            : base(configuration)
        {
            IdentityModelFactory = idenityModelFactory;
        }

        ~AspNetUserStore()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private IDbContextFactory<AspNetIdentityModel> IdentityModelFactory { get; }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);
            using var context = IdentityModelFactory.CreateDbContext();
            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                TransferUserToDb(user, out var userRecord, out var membershipRecord);

                userRecord.LastActivityDate = DateTime.UtcNow;

                _ = await context.AspNetUsers.AddAsync(userRecord, cancellationToken);
                _ = await context.AspNetMemberships.AddAsync(membershipRecord, cancellationToken);

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

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);
            using var context = IdentityModelFactory.CreateDbContext();
            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var membershipAccount =
                    context.ChangeTracker.Entries<AspNetMembership>().FirstOrDefault(e => e.Entity.UserId == user.Id)?.Entity ??
                    context.AspNetMemberships.SingleOrDefault(m => m.UserId == user.Id);

                if (membershipAccount != null)
                {
                    context.AspNetMemberships.Remove(membershipAccount);
                }

                context
                    .ChangeTracker
                    .Entries<AspNetUsersInRole>()
                    .Where(e => e.Entity.UserId == user.Id)
                    .ToList()
                    .ForEach(entry => entry.State = EntityState.Detached);

                var userInRoles = context
                    .AspNetUsersInRoles
                    .Where(uir => uir.UserId == user.Id);

                if (userInRoles.Any())
                {
                    context
                        .AspNetUsersInRoles
                        .RemoveRange(userInRoles);
                }

                context
                    .ChangeTracker
                    .Entries<AspNetProfile>()
                    .Where(e => e.Entity.UserId == user.Id)
                    .ToList()
                    .ForEach(entry => entry.State = EntityState.Detached);

                var profile = context.AspNetProfiles.SingleOrDefault(p => p.UserId == user.Id);
                if (profile != null)
                {
                    context.AspNetProfiles.Remove(profile);
                }

                var userAccount =
                    context.ChangeTracker.Entries<AspNetUser>().FirstOrDefault(e => e.Entity.UserId == user.Id)?.Entity ??
                    context.AspNetUsers.SingleOrDefault(m => m.UserId == user.Id);

                if (userAccount != null)
                {
                    context
                        .AspNetUsers
                        .Remove(userAccount);
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

        public Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }
            if (!Guid.TryParse(userId, out var id))
            {
                throw new ArgumentException("Not a valid (GUID) id", nameof(userId));
            }

            using var context = IdentityModelFactory.CreateDbContext();
            var user = context
                .AspNetMemberships
                .Include(m => m.User)
                .SingleOrDefault(m => m.UserId == id);

            return Task.FromResult(TransferDbToUser(user));
        }

        public Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(normalizedUserName))
            {
                throw new ArgumentNullException(nameof(normalizedUserName));
            }

            normalizedUserName = normalizedUserName.ToLower();

            using var context = IdentityModelFactory.CreateDbContext();
            var user = context
                .AspNetUsers
                .Include(u => u.AspNetMembership)
                .SingleOrDefault(record =>
                    record.ApplicationId == ApplicationId &&
                    record.LoweredUserName == normalizedUserName);

            return Task.FromResult(TransferDbToUser(user?.AspNetMembership));
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);
            user.NormalizedUserName = normalizedName ?? throw new ArgumentNullException(nameof(normalizedName));
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);
            user.UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            // Validate data
            ParameterValidation(user, cancellationToken);

            // Begin database transaction
            using var context = IdentityModelFactory.CreateDbContext();
            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Parse Application User into database records
                TransferUserToDb(user, out var userRecord, out var membershipRecord);

                // Manage entity tracking (if enabled)
                var ur = context
                    .ChangeTracker
                    .Entries<AspNetUser>()
                    .FirstOrDefault(e => e.Entity.UserId == userRecord.UserId);

                if ((ur?.State ?? EntityState.Detached) != EntityState.Detached)
                {
                    ur.State = EntityState.Detached;
                }

                // Apply update to database
                userRecord.LastActivityDate = DateTime.UtcNow;
                _ = context.AspNetUsers.Update(userRecord);

                // Manage entity tracking (if enabled)
                var mr = context
                    .ChangeTracker
                    .Entries<AspNetMembership>()
                    .FirstOrDefault(e => e.Entity.UserId == membershipRecord.UserId);

                if (mr != null)
                {
                    mr.State = EntityState.Detached;
                }

                // Apply update to database
                _ = context.AspNetMemberships.Update(membershipRecord);

                // Commit the updates to the database
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

        private static void ParameterValidation(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (user.ApplicationId == Guid.Empty)
            {
                throw new Exception($"Bad value: {nameof(user.ApplicationId)}");
            }
            if (user.Id == Guid.Empty)
            {
                throw new Exception($"Bad value: {nameof(user.Id)}");
            }
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                throw new Exception($"Bad value: {nameof(user.UserName)}");
            }
        }

        private static ApplicationUser TransferDbToUser(AspNetMembership data)
        {
            if (data == null)
            {
                return null;
            }
            else
            {
                var user = new ApplicationUser()
                {
                    ApplicationId = data.ApplicationId,
                    Id = data.UserId,
                    UserName = data.User.UserName,
                    LastActivityDate = data.User.LastActivityDate,

                    PasswordHash = data.Password,
                    PasswordFormat = data.PasswordFormat,
                    SecurityStamp = data.PasswordSalt,
                    Email = data.Email,
                    NormalizedEmail = data.LoweredEmail,
                    // PasswordQuestion = null,
                    // PasswordAnswer = null,
                    IsApproved = data.IsApproved,
                    LockoutEnabled = data.IsLockedOut,
                    LockoutEnd = null,
                    CreateDate = data.CreateDate,
                    LastLoginDate = data.LastLoginDate,
                    LastPasswordChangedDate = data.LastPasswordChangedDate,
                    LastLockoutDate = data.LastLockoutDate,
                    AccessFailedCount = data.FailedPasswordAttemptCount,
                    Comment = data.Comment,
                };
                return user;
            }
        }

        private static void TransferUserToDb(ApplicationUser user, out AspNetUser userRecord, out AspNetMembership membershipRecord)
        {
            userRecord = new AspNetUser()
            {
                ApplicationId = user.ApplicationId,
                UserId = user.Id,
                UserName = user.UserName,
                LoweredUserName = user.NormalizedUserName,
                MobileAlias = null,
                IsAnonymous = false,
                LastActivityDate = DateTime.UtcNow,
            };

            membershipRecord = new AspNetMembership()
            {
                ApplicationId = user.ApplicationId,
                UserId = user.Id,
                Password = user.PasswordHash ?? string.Empty,
                PasswordFormat = user.PasswordFormat,
                PasswordSalt = user.SecurityStamp ?? Security.CreateSalt(),
                MobilePIN = null,
                Email = user.Email,
                LoweredEmail = user.NormalizedEmail,
                PasswordQuestion = null,
                PasswordAnswer = null,
                IsApproved = user.IsApproved,
                IsLockedOut = user.LockoutEnabled,
                CreateDate = user.CreateDate,
                LastLoginDate = user.LastLoginDate,
                LastPasswordChangedDate = user.LastPasswordChangedDate,
                LastLockoutDate = user.LastLockoutDate,
                FailedPasswordAttemptCount = user.AccessFailedCount,
                FailedPasswordAttemptWindowStart = SqlDateTime.MinValue.Value,
                FailedPasswordAnswerAttemptCount = 0,
                FailedPasswordAnswerAttemptWindowStart = SqlDateTime.MinValue.Value,
                Comment = user.Comment
            };
        }
    }
}