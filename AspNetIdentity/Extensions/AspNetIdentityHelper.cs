using AspNetIdentity.Data;
using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlMembershipEntityModel.Context;
using System.Runtime.Versioning;

namespace AspNetIdentity.Extensions;

public static class AspNetIdentityHelper
{
    public const string ConnectionStringForMembership = "ConnectionStrings:Membership";

    /// <summary>
    /// Adds the ASP.NET Identity dependencies.
    /// Attributed to Windows supported operating system, due to the use of a Microsoft Access Database.
    /// The Windows supported operating system attribute can be removed with the use of a different
    /// database technology.
    /// </summary>
    /// <param name="serviceCollection">Dependency injection service collection</param>
    [SupportedOSPlatform("Windows")]
    public static void AddAspNetIdentity(this IServiceCollection serviceCollection)
    {
        // Identity Services
        serviceCollection.AddTransient<IUserStore<ApplicationUser>, AspNetUserStore>();
        serviceCollection.AddTransient<IRoleStore<ApplicationRole>, AspNetRoleStore>();

        // Password Hasher - must be added before AddIdentity -or- AddIdentityCore
        serviceCollection.AddScoped<IPasswordHasher<ApplicationUser>, AspNetPasswordHasher>();

        // Configure DI for application services
        serviceCollection.AddScoped<IAuthenticationManager, AuthenticationManager>();

        // Add identity types
        serviceCollection
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddDefaultTokenProviders();

        // Identity Data Model
        serviceCollection.AddDbContextFactory<AspNetIdentityModel>((serviceProvider, builder) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            builder.UseJet(configuration[ConnectionStringForMembership]);
            builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
    }
}