using ASI.Basecode.Data;
using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ASI.Basecode.WebApp
{
    // Other services configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configures the other services.
        /// </summary>
        private void ConfigureOtherServices()
        {
            // Framework
            this._services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            this._services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Common
            this._services.AddScoped<TokenProvider>();
            this._services.TryAddSingleton<TokenProviderOptionsFactory>();
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUnitOfWork>(provider =>
            {
                var context = provider.GetRequiredService<AsiBasecodeDBContext>();
                return new UnitOfWork<AsiBasecodeDBContext>(context, context);
            });

            // Services
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUserService, UserService>();
            this._services.AddScoped<IAdminService, AdminService>();
            this._services.AddScoped<IOrganizationService, OrganizationService>();
            this._services.AddScoped<IOrgBooksService, OrgBooksService>();
            this._services.AddScoped<IBorrowTicketService, BorrowTicketService>();
            this._services.AddScoped<IBookReviewService, BookReviewService>();
            this._services.AddScoped<IBookCheckoutService, BookCheckoutService>();
            this._services.AddScoped<IWishlistService, WishlistService>();
            this._services.AddScoped<IBookRecommendationService, BookRecommendationService>();
            this._services.AddScoped<IEmailService, EmailService>();
            this._services.AddScoped<IOTPService, OTPService>();
            this._services.AddScoped<IPasswordResetService, PasswordResetService>();

            // Repositories
            this._services.AddScoped<IUserRepository, UserRepository>();
            this._services.AddScoped<IAdminRepository, AdminRepository>();
            this._services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            this._services.AddScoped<IOrgBooksRepository, OrgBooksRepository>();
            this._services.AddScoped<IPublisherRepository, PublisherRepository>();
            this._services.AddScoped<IBorrowTicketRepository, BorrowTicketRepository>();
            this._services.AddScoped<IBorrowingPolicyRepository, BorrowingPolicyRepository>();
            this._services.AddScoped<IFinePolicyRepository, FinePolicyRepository>();
            this._services.AddScoped<IRenewalPolicyRepository, RenewalPolicyRepository>();
            this._services.AddScoped<IClaimPolicyRepository, ClaimPolicyRepository>();
            this._services.AddScoped<IBookReviewRepository, BookReviewRepository>();
            this._services.AddScoped<IBookCheckoutRepository, BookCheckoutRepository>();
            this._services.AddScoped<IWishlistRepository, WishlistRepository>();
            this._services.AddScoped<IPendingUserRegistrationRepository, PendingUserRegistrationRepository>();
            this._services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

            // Manager Class
            this._services.AddScoped<SignInManager>();

            this._services.AddHttpClient();
        }
    }
}
