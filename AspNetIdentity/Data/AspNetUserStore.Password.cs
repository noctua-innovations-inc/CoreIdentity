using AspNetIdentity.Extensions;
using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetIdentity.Data
{
    public partial class AspNetUserStore : IUserPasswordStore<ApplicationUser>
    {
        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);

            var saltText = user.SecurityStamp;
            var hashText = user.PasswordHash;

            var result = Security.CreatePasswordHash(0x00, saltText, hashText);
            return Task.FromResult(result);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);
            using var context = IdentityModelFactory.CreateDbContext();
            var aspnetUser = context.AspNetUsers.SingleOrDefault(record => record.UserId == user.Id);
            if (aspnetUser == null)
            {
                return Task.FromResult(false);
            }
            else
            {
                var aspnetMember = context.AspNetMemberships.SingleOrDefault(record => record.UserId == user.Id);
                return Task.FromResult(aspnetMember == null);
            }
        }

        public async Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            ParameterValidation(user, cancellationToken);
            using var context = IdentityModelFactory.CreateDbContext();

            var membershipAccount =
                (
                    context.ChangeTracker.Entries<AspNetMembership>().FirstOrDefault(e => e.Entity.UserId == user.Id)?.Entity ??
                    context.AspNetMemberships.SingleOrDefault(m => m.UserId == user.Id)
                )
                ?? throw new Exception("");

            membershipAccount.LastPasswordChangedDate = DateTime.UtcNow;
            membershipAccount.IsLockedOut = false;
            membershipAccount.Password = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            membershipAccount.PasswordSalt = user.SecurityStamp;

            _ = context.AspNetMemberships.Update(membershipAccount);
            _ = await context.SaveChangesAsync(cancellationToken);
        }
    }
}