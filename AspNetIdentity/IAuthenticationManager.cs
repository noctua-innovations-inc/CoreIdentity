using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetIdentity;

public interface IAuthenticationManager
{
    Task<bool> ValidateUserAsync(string userName, string password);

    Task<UserLogin> AuthenticateAsync(UserLogin user, bool issueJwtToken = true);

    Task<UserLogin> SignOutAsync(UserLogin user);

    #region --[ User Management ]--

    Task<ApplicationUser> GetUserAsync(string userName);

    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);

    Task<string[]> GetRolesForUserAsync(string userName);

    Task<List<ApplicationUser>> GetAllUsersInRolesAsync(string[] roles);

    Task<bool> IsUserInRoleAsync(string userName, string role);

    Task<AspNetMembership> GetMembershipAsync(string userName);

    Task<string> ResetPasswordAsync(string userName);

    Task<bool> ChangePasswordAsync(string userName, string oldPassword, string newPassword);

    string GeneratePassword();

    #endregion --[ User Management ]--
}