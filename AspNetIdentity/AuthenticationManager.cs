using AspNetIdentity.Data;
using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SqlMembershipEntityModel.Context;
using SqlMembershipEntityModel.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetIdentity
{
    public class AuthenticationManager : IAuthenticationManager
    {
        public AuthenticationManager
        (
            IConfiguration configuration,
            IDbContextFactory<AspNetIdentityModel> identityModelFactory,
            SignInManager<ApplicationUser> signInManager
        )
        {
            Configuration = configuration;
            IdentityModelFactory = identityModelFactory;
            SignInManager = signInManager;
        }

        private IConfiguration Configuration { get; }
        private IDbContextFactory<AspNetIdentityModel> IdentityModelFactory { get; }
        private SignInManager<ApplicationUser> SignInManager { get; }
        private UserManager<ApplicationUser> UserManager => SignInManager.UserManager;


        public async Task<bool> ValidateUserAsync(string userName, string password)
        {
            var applicationUser = await UserManager.FindByNameAsync(userName);
            if (applicationUser == null)
            {
                return false;
            }

            return await UserManager.CheckPasswordAsync(applicationUser, password);
        }

        public async Task<UserLogin> AuthenticateAsync(UserLogin user, bool issueJwtToken = true)
        {
            var applicationUser = await UserManager.FindByNameAsync(user.UserName);

            if (applicationUser == null)
            {
                return null;
            }

            if (!applicationUser.IsApproved)
            {
                return null;
            }

            // This method does a lot...
            //  * Checks whether sign-in is allowed.
            //  * Checks that the password is correct.
            //  * Sets up relevant two-factor authentication cookie.
            //  * Performs sign-in process which concludes with the creation of a
            //    ClaimsPrincipal cookie which is persisted.
            var signInResult = await SignInManager.PasswordSignInAsync(applicationUser, user.Password, true, true);

            applicationUser.LastActivityDate = DateTime.UtcNow;
            switch (signInResult)
            {
                case var value when value == SignInResult.Success:
                    applicationUser.AccessFailedCount = 0;
                    applicationUser.LastLoginDate = DateTime.UtcNow;
                    applicationUser.LastLockoutDate = SqlDateTime.MinValue.Value;
                    break;

                case var value when value == SignInResult.Failed:
                    applicationUser.AccessFailedCount += 1;
                    break;

                case var value when value == SignInResult.LockedOut:
                    break;

                case var value when value == SignInResult.NotAllowed
                    || value == SignInResult.TwoFactorRequired:
                    break;
            }

            var updateResult = await UserManager.UpdateAsync(applicationUser);
            if (!updateResult.Succeeded)
            {
                // Catastrophic error.
                return null;
            }

            if (signInResult != SignInResult.Success)
            {
                return null;
            }

            user.UserName = applicationUser.UserName;
            user.Password = null;

            if (issueJwtToken)
            {
                var facility = new JsonWebToken(Configuration)
                {
                    JwtAudience = Configuration[JsonWebToken.JwtAudienceEntry]
                };

                var roles = await UserManager.GetRolesAsync(applicationUser);

                user.Token = facility
                    .SetUserName(user)
                    .SetUserRoles(roles)
                    .GenerateToken();
            }

            return user;
        }

        public async Task<UserLogin> SignOutAsync(UserLogin user)
        {
            user.Token = null;
            return await Task.FromResult(user);
        }

        #region --[ User Management ]--

        public async Task<ApplicationUser> GetUserAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }
            return await UserManager.FindByNameAsync(userName);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await UserManager.UpdateAsync(user);
        }

        public async Task<string[]> GetRolesForUserAsync(string userName)
        {
            var applicationUser = await GetUserAsync(userName);
            if (applicationUser == null)
            {
                return Array.Empty<string>();
            }

            return (await UserManager.GetRolesAsync(applicationUser)).ToArray();
        }

        public async Task<List<ApplicationUser>> GetAllUsersInRolesAsync(string[] roles)
        {
            var users = new List<ApplicationUser>();
            if ((roles?.Length ?? 0) == 0)
            {
                return users;
            }

            foreach (var role in roles)
            {
                var userList = await UserManager.GetUsersInRoleAsync(role);
                foreach (var user in userList)
                {
                    if (users.Find(u => u.Id == user.Id) == null)
                    {
                        users.Add(user);
                    }
                }
            }

            return users;
        }

        public async Task<bool> IsUserInRoleAsync(string userName, string role)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(role))
            {
                return false;
            }

            var user = await GetUserAsync(userName);
            if (user == null)
            {
                return false;
            }

            return await UserManager.IsInRoleAsync(user, role);
        }

        public async Task<AspNetMembership> GetMembershipAsync(string userName)
        {
            var user = await GetUserAsync(userName);
            if (user == null)
            {
                return null;
            }

            using var context = IdentityModelFactory.CreateDbContext();

            return await context
                .AspNetMemberships
                .FirstOrDefaultAsync(member => member.UserId == user.Id);
        }

        public async Task<string> ResetPasswordAsync(string userName)
        {
            var user = await GetUserAsync(userName);
            if (user == null)
            {
                return null;
            }

            var passwordToken = await UserManager.GeneratePasswordResetTokenAsync(user);
            var password = GeneratePassword();

            var identity = await UserManager.ResetPasswordAsync(user, passwordToken, password);

            return (identity.Succeeded) ? password : null;
        }

        public async Task<bool> ChangePasswordAsync(string userName, string oldPassword, string newPassword)
        {
            var user = await GetUserAsync(userName);
            if (user == null)
            {
                return false;
            }

            var result = await UserManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result.Succeeded;
        }

        public string GeneratePassword()
        {
            var options = UserManager.Options.Password;

            var length = options.RequiredLength;
            var nonAlphanumeric = options.RequireNonAlphanumeric;
            var digit = options.RequireDigit;
            var lowercase = options.RequireLowercase;
            var uppercase = options.RequireUppercase;

            var password = new StringBuilder();
            var random = new Random();

            while (password.Length < length)
            {
                char c = (char)random.Next(32, 126);

                password.Append(c);

                if (char.IsDigit(c))
                {
                    digit = false;
                }
                else if (char.IsLower(c))
                {
                    lowercase = false;
                }
                else if (char.IsUpper(c))
                {
                    uppercase = false;
                }
                else if (!char.IsLetterOrDigit(c))
                {
                    nonAlphanumeric = false;
                }
            }

            if (nonAlphanumeric)
            {
                password.Append((char)random.Next(33, 48));
            }

            if (digit)
            {
                password.Append((char)random.Next(48, 58));
            }

            if (uppercase)
            {
                password.Append((char)random.Next(65, 91));
            }

            if (lowercase)
            {
                password.Append((char)random.Next(97, 123));
            }

            return password.ToString();
        }

        #endregion --[ User Management ]--
    }
}
