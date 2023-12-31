﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IdentityTesting.Extensions;

internal static class MockingHelper
{
    /// <summary>
    /// ASP.NET Core Identity framework's SignInManager user sign-in authentication is very tightly coupled to the IHttpContextAccessor.
    /// This method will inject a fake (mocks) IHttpContextAccess implementation so the SignInManager sign-in features can be executed
    /// in a Microsoft Test Project scenario.
    /// </summary>
    /// <param name="serviceCollection">The service collection used to build the DI service provider.</param>
    public static void AddHttpContextAccessorMock(this IServiceCollection serviceCollection)
    {
        // Mock IAuthenticationService
        var authManager = new Mock<IAuthenticationService>();
        authManager
            .Setup(s =>
                s.SignOutAsync(
                    It.IsAny<HttpContext>(),
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    It.IsAny<AuthenticationProperties>()))
            .Returns(Task.FromResult(true));

        ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();

        // Mock an IServiceProvider for the HttpContext, RequestServices
        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(sp => sp.GetService(typeof(IUrlHelperFactory))).Returns(new UrlHelperFactory());
        servicesMock.Setup(sp => sp.GetService(typeof(ITempDataDictionaryFactory))).Returns(new TempDataDictionaryFactory(tempDataProvider));
        servicesMock.Setup(sp => sp.GetService(typeof(IAuthenticationService))).Returns(authManager.Object);

        // Mock an HttpContext for the IHttpContextAccessor
        var context = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                RequestServices = servicesMock.Object
            }
        };

        // Mock IHttpContextAccessor
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor
            .Setup(req => req.HttpContext)
            .Returns(context.HttpContext);

        // Register the IHttpContextAccessor mock
        serviceCollection.AddTransient(sp => mockHttpContextAccessor.Object);
    }
}