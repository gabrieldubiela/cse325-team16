using System.Collections.Generic;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VehicleRentalManager.Models;
using VehicleRentalManager.Services;
using Xunit;

namespace VehicleRentalManager.Tests;

public class ApprovalsPageTests : TestContext
{
    [Fact]
    public void AuthenticatedUser_SeesPendingAndApprovedLists()
    {
        var authStateProvider = new TestAuthStateProvider(
            isAuthenticated: true,
            userName: "Manager");

        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
        Services.AddSingleton<AuthService>();
        Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
        Services.AddAuthorization();
        Services.AddCascadingAuthenticationState();

        // Mock da interface, não da classe concreta
        var mockUserService = new Mock<IUserService>();
        mockUserService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<AppUser>
            {
                new AppUser { Id = "1", Name = "Pendente", Email = "p@end.com", IsApproved = false },
                new AppUser { Id = "2", Name = "Aprovado", Email = "a@prov.com", IsApproved = true },
            });

        Services.AddSingleton<IUserService>(mockUserService.Object);

        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Approvals>();

        Assert.Contains("Employees", cut.Markup);
        Assert.Contains("Pending Approval", cut.Markup);
        Assert.Contains("Approved Employees", cut.Markup);
        Assert.Contains("Pendente", cut.Markup);
        Assert.Contains("Aprovado", cut.Markup);
    }
}
