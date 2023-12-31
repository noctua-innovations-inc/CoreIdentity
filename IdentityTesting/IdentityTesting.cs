using AspNetIdentity;
using AspNetIdentity.Data;
using AspNetIdentity.Extensions;
using AspNetIdentity.Models;
using FluentAssertions;
using IdentityTesting.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SqlMembershipEntityModel.Context;

namespace IdentityTesting;

/// <summary>
/// This is not intended to illustrate Test Driven Development (TDD) and/or
/// unit testing.  It is intended to be a more agile code execution framework
/// than would be offered by a Console application with the intent of proving
/// the subject matter's primary concept (Customizing ASP.NET Core Identity).
/// </summary>
public sealed class IdentityTesting
{
    private readonly string TestPassword = "123456";
    private readonly string TestPasswordHash = "LgDTVa7Es6Zy+mDYo3vIRfolX9w=";
    private readonly string TestSecuritySalt = "ogf3xWsRWElEvQLGZ8tXow==";

    #region --[ CTOR ]--

    public IdentityTesting()
    {
        Services.AddAspNetIdentity();
        Services.AddHttpContextAccessorMock();
        Services.AddConfiguration();
        Services.AddLogging();
        Services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        LazyServiceProvider = new(() => Services.BuildServiceProvider());
    }

    #endregion --[ CTOR ]--

    #region --[ DI Services ]--

    private ServiceCollection Services { get; } = new();
    private Lazy<ServiceProvider> LazyServiceProvider { get; }
    private ServiceProvider ServiceProvider => LazyServiceProvider.Value;

    private IAuthenticationManager AuthenticationManager => ServiceProvider.GetRequiredService<IAuthenticationManager>();
    private IConfiguration Configuration => ServiceProvider.GetRequiredService<IConfiguration>();
    private IDbContextFactory<AspNetIdentityModel> DbContextFactory => ServiceProvider.GetRequiredService<IDbContextFactory<AspNetIdentityModel>>();

    #endregion --[ DI Services ]--

    /// <summary>
    /// Sanity test: Ensure our database connectivity is working at the most basic level.
    ///              Database connectivity could fail through mistakes in the ORM definition.
    /// </summary>
    [Fact]
    public void DbContext_Connectivity_Successful()
    {
        #region --[ Arrange ]--

        // Create database context
        using var context = DbContextFactory.CreateDbContext();

        #endregion --[ Arrange ]--

        #region --[ Act ]--

        // Force entity collection evaluations
        var applications = context.AspNetApplications.ToList();
        var memberships = context.AspNetMemberships.ToList();
        var profiles = context.AspNetProfiles.ToList();
        var roles = context.AspNetRoles.ToList();
        var schemaVersions = context.AspNetSchemaVersions.ToList();
        var users = context.AspNetUsers.ToList();
        var usersInRoles = context.AspNetUsersInRoles.ToList();

        #endregion --[ Act ]--

        #region --[ Assert ]--

        // Assumes that at least one user account has been created
        applications.Should().NotBeNullOrEmpty();
        memberships.Should().NotBeNullOrEmpty();
        schemaVersions.Should().NotBeNullOrEmpty();
        users.Should().NotBeNullOrEmpty();

        // May not have defined any roles, but there should be at least an empty dataset
        profiles.Should().NotBeNull();
        roles.Should().NotBeNull();
        usersInRoles.Should().NotBeNull();

        #endregion --[ Assert ]--
    }

    /// <summary>
    /// Validate that the user login happy path works as expected.
    /// This is intended to provide that the end-user's credentials are
    /// successfully validated with our ASP.NET Core Identity Customizations
    /// and nothing more.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Authenticate_User_Successfully()
    {
        #region --[ Arrange ]--

        var userLogin = new UserLogin()
        {
            UserName = "Olivia",
            Password = "SecretPassword!"
        };

        var jwt = new JsonWebToken(Configuration);

        #endregion --[ Arrange ]--

        #region --[ Act ]--

        // Validate user credentials
        var isValidUserCredentials = await AuthenticationManager.ValidateUserAsync(userLogin.UserName, userLogin.Password);

        // Authenticate user successfully
        var authenticatedUser = await AuthenticationManager.AuthenticateAsync(userLogin);
        var token = authenticatedUser.Token;

        // Sign out authenticated user
        var signedOutUser = await AuthenticationManager.SignOutAsync(authenticatedUser);

        #endregion --[ Act ]--

        #region --[ Assert ]--

        isValidUserCredentials.Should().BeTrue(because: "Valid user-name and password were provided.");

        // Successful authentication results in the data transfer object (DTO) being returned with a JWT value.
        authenticatedUser.Should().NotBeNull();
        token.Should().NotBeNullOrEmpty(because: "Successful authentication results in the creation of a JWT.");

        // Successful user sign out results in the data transfer object (DTO) being returned without a JWT value.
        signedOutUser.Should().NotBeNull();
        signedOutUser.Token.Should().BeNull(because: "Successful sign-out results in the JWT being erased.");

        jwt.ValidateToken(token, out var validatedToken).Should().BeTrue(because: "JWT was created by authentication.");

        validatedToken.Should().NotBeNull(because: "Security token was created through JWT validation.");

        #endregion --[ Assert ]--
    }

    [Fact]
    public void Identity_PasswordHashingValidation_Successful()
    {
        // Arrange - the data
        // Action - encode a clear text password into a hashed password
        var hashPassword = Security.EncodePassword(TestPassword, TestSecuritySalt);

        // Assert - that the clear text password was successfully hashed
        hashPassword.Should().BeEquivalentTo(TestPasswordHash, because: "We successfully hashed the password.");
    }
}