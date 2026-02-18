using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace VehicleRentalManager.Tests;

public class AuthPageTests : TestContext
{
    [Fact]
    public void WhenStatusPendingNew_ShowsAccountCreatedCard()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        var uri = nav.GetUriWithQueryParameter("Status", "pending_new");
        nav.NavigateTo(uri);

        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Auth>();

        Assert.Contains("Account Created!", cut.Markup);
        Assert.Contains("Your account has been created successfully.", cut.Markup);
    }

    [Fact]
    public void WhenStatusPending_ShowsAwaitingApprovalCard()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        var uri = nav.GetUriWithQueryParameter("Status", "pending");
        nav.NavigateTo(uri);

        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Auth>();

        Assert.Contains("Awaiting Approval", cut.Markup);
        Assert.Contains("Your account is still pending manager approval.", cut.Markup);
    }

    [Fact]
    public void WhenErrorLoginFailed_ShowsLoginFailedAlert()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        var uri = nav.GetUriWithQueryParameter("Error", "login_failed");
        nav.NavigateTo(uri);

        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Auth>();

        Assert.Contains("Sign in", cut.Markup);
        Assert.Contains("Login failed. Please try again.", cut.Markup);
    }

    [Fact]
    public void WhenNoStatusOrError_ShowsDefaultSignIn()
    {
        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Auth>();

        Assert.Contains("Sign in", cut.Markup);
        Assert.Contains("Sign in with Google", cut.Markup);
        Assert.Contains("cse325vrm@gmail.com", cut.Markup);
    }
}
