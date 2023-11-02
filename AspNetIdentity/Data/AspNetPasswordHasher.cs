using AspNetIdentity.Extensions;
using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;

namespace AspNetIdentity.Data
{
    public class AspNetPasswordHasher : PasswordHasher<ApplicationUser>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Reserved for future use")]
        private readonly PasswordHasherCompatibilityMode _compatibilityMode;

        public AspNetPasswordHasher(IOptions<PasswordHasherOptions> optionsAccessor = null) : base(optionsAccessor)
        {
            _compatibilityMode = optionsAccessor?.Value?.CompatibilityMode ?? PasswordHasherCompatibilityMode.IdentityV3;
        }

        // The HashPassword() function is called when a new user registers,
        // and the password needs hashing before it's stored in the database.
        // It's also called after an old v2 format password hash is verified, and needs rehashing.
        public override string HashPassword(ApplicationUser user, string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            user.SecurityStamp = Security.CreateSalt();
            user.PasswordHash = Security.EncodePassword(password, user.SecurityStamp);
            return user.PasswordHash;
        }

        /// <summary>
        /// Returns a <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.
        /// </summary>
        /// <param name="user">The user whose password should be verified.</param>
        /// <param name="hashedPassword">The hash value for a user's stored password.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>A <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.</returns>
        /// <remarks>Implementations of this method should be time consistent.</remarks>
        public override PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }
            if (providedPassword == null)
            {
                throw new ArgumentNullException(nameof(providedPassword));
            }

            byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            // read the format marker from the hashed password
            if (decodedHashedPassword.Length == 0)
            {
                return PasswordVerificationResult.Failed;
            }

            switch (decodedHashedPassword[0])
            {
                case 0x00:
                    if (Security.VerifyHashedPasswordV2(decodedHashedPassword, providedPassword))
                    {
                        // Use PasswordVerificationResult.SuccessRehashNeeded to indicate that the
                        // provided password was correct, but the stored hash should be updated.
                        return (user.MustChangePassword)
                            ? PasswordVerificationResult.SuccessRehashNeeded
                            : PasswordVerificationResult.Success;
                    }
                    else
                    {
                        return PasswordVerificationResult.Failed;
                    }
                default:
                    {
                        return PasswordVerificationResult.Failed;
                    }
            }
        }
    }
}