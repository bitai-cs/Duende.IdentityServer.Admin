// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
// File: https://github.com/IdentityServer/IdentityServer4/blob/main/samples/Quickstarts/3_AspNetCoreAndApis/src/IdentityServer/Quickstart/Account/ExternalController.cs

// Modified by Jan Škoruba and J. Arturo

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skoruba.Duende.IdentityServer.STS.Identity.Configuration;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Shared.Entities.Identity;

namespace Skoruba.Duende.IdentityServer.STS.Identity.Helpers
{
    public class ApplicationSignInManager<TUser> : SignInManager<TUser>
        where TUser : class
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly LdapWebApiProviderConfiguration<TUser> _ldapWebApiProvider;

        public ApplicationSignInManager(UserManager<TUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<TUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<TUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<TUser> confirmation,
            LdapWebApiProviderConfiguration<TUser> ldapWebApiProvider) : base(userManager, contextAccessor,
                claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _contextAccessor = contextAccessor;
            _ldapWebApiProvider = ldapWebApiProvider;
        }

        public override async Task SignInWithClaimsAsync(TUser user, AuthenticationProperties authenticationProperties, IEnumerable<Claim> additionalClaims)
        {
            var claims = additionalClaims.ToList();

            var externalResult = await _contextAccessor.HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (externalResult != null && externalResult.Succeeded)
            {
                var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
                if (sid != null)
                {
                    claims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
                }

                if (authenticationProperties != null)
                {
                    // if the external provider issued an id_token, we'll keep it for sign out
                    var idToken = externalResult.Properties.GetTokenValue("id_token");
                    if (idToken != null)
                    {
                        authenticationProperties.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
                    }
                }

                var authenticationMethod = claims.FirstOrDefault(x => x.Type == ClaimTypes.AuthenticationMethod);
                var idp = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.IdentityProvider);

                if (authenticationMethod != null && idp == null)
                {
                    claims.Add(new Claim(JwtClaimTypes.IdentityProvider, authenticationMethod.Value));
                }
            }

            await base.SignInWithClaimsAsync(user, authenticationProperties, claims);
        }

        /// <summary>
        /// Allow to authenticate user credentials against an LDAP Web Api.
        /// </summary>
        /// <remarks>
        /// When a user does not have a domain assigned or when the user's 
        /// domain is not related to an LDAP Web Api, the password verification
        /// operation follows the regular flow in 
        /// <see cref="SignInManager{TUser}.CheckPasswordSignInAsync(TUser, string, bool)"/>.
        /// </remarks>
        /// <param name="user">User identity. See <see cref="IUserWithDomain"/>.</param>
        /// <param name="password">User password.</param>
        /// <param name="lockoutOnFailure">Whether or not to lockout the user account when password validation fails.</param>
        /// <returns><see cref="SignInResult"/></returns>
        public override async Task<SignInResult> CheckPasswordSignInAsync(TUser user, string password, bool lockoutOnFailure)
        {
            // Try to cast the user identity to the custom IUserWithDomain interface. If the cast fails, it means that the user identity does not have the UserDomain property, so we proceed with the regular password verification flow.
            var userWithDomain = user as IUserWithDomain;
            if (userWithDomain == null)
                return await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);

            // Try to get the user domain profile based on the UserDomain property of the user identity. If there is no user domain profile for the user's domain, it means that the user's domain is not registered to access the LDAP Web Api, so we proceed with the regular password verification flow.
            var userDomainProfile = _ldapWebApiProvider.GetUserDomainProfile(userWithDomain.UserDomain);

            // The user does not have an assigned domain or the user's domain is not registered to access the LDAP Web Api.
            if (userDomainProfile == null)
                // In this case, the password verification operation follows the regular flow in SignInManager<TUser>.CheckPasswordSignInAsync(TUser, string, bool).
                return await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
            
            var error = await PreSignInCheck(user);
            if (error != null)
                return error;

            var ldapAccountCredentials = new Bitai.LDAPHelper.DTO.LDAPDomainAccountCredential
            {
                DomainName = userWithDomain.UserDomain,
                AccountName = userWithDomain.UserName.Split('\\').Last(),
                DomainAccountPassword = password
            };

            var ldapAuthenticationsWebApiClient = userDomainProfile.GetLdapAuthenticationsWebApiClient();
            var httpResponse = await ldapAuthenticationsWebApiClient.AuthenticateAsync(ldapAccountCredentials);
            if (!httpResponse.IsSuccessResponse)
            {
                #region Write event log
                string responseContent = null;
                Bitai.WebApi.Server.MiddlewareExceptionModel middlewareException = null;
                switch (httpResponse.ContentMediaType)
                {
                    case Bitai.WebApi.Common.Content_MediaType.ApplicationJson:
                        responseContent = (httpResponse as Bitai.WebApi.Client.NoSuccessResponseWithJsonStringContent).Content;
                        break;

                    case Bitai.WebApi.Common.Content_MediaType.ApplicationProblemJson:
                        middlewareException = (httpResponse as Bitai.WebApi.Client.NoSuccessResponseWithJsonExceptionContent).Content;
                        responseContent = middlewareException.ToStringReport();
                        break;

                    case Bitai.WebApi.Common.Content_MediaType.TextHtml:
                        responseContent = (httpResponse as Bitai.WebApi.Client.NoSuccessResponseWithHtmlContent).Content;
                        break;

                    case Bitai.WebApi.Common.Content_MediaType.NoContent:
                        responseContent = "(No response content)";
                        break;
                }

                Logger.LogError("Login failed: DomainName:{0}, AccountName:{1}", ldapAccountCredentials.DomainName, ldapAccountCredentials.AccountName);
                Logger.LogError($"Failed to authenticate user credentials using the LDAP Web Api. Response Code: {(int)httpResponse.HttpStatusCode} ({httpResponse.ReasonPhrase}). Content Type: {httpResponse.ContentMediaType}");
                Logger.LogError(responseContent);
                #endregion
                
                var _customResult = httpResponse.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? CustomSignInResult.NotAllowed
                    : CustomSignInResult.Failed;

                _customResult.HttpStatusCode = (int)httpResponse.HttpStatusCode;
                _customResult.HttpReasonPhrase = middlewareException != null
                    ? $"{middlewareException.Source}: {middlewareException.Message}"
                    : httpResponse.ReasonPhrase;
                _customResult.HttpContentType = httpResponse.ContentMediaType.ToString();
                _customResult.HttpResponseContent = responseContent;

                return _customResult;
            }
            else
            {
                var accountAuthenticationStatus = ldapAuthenticationsWebApiClient.GetDTOFromResponse<Bitai.LDAPHelper.DTO.LDAPDomainAccountAuthenticationResult>(httpResponse);

                if (accountAuthenticationStatus.IsAuthenticated)
                {
                    var successResult = CustomSignInResult.Success;
                    successResult.HttpStatusCode = (int)httpResponse.HttpStatusCode;
                    successResult.HttpReasonPhrase = accountAuthenticationStatus.OperationMessage;

                    return await ResetAccessFailedCountOnSuccessAsync(user, successResult);
                }

                var failedResult = CustomSignInResult.NotAllowed;
                failedResult.HttpStatusCode = (int)httpResponse.HttpStatusCode;
                failedResult.HttpReasonPhrase = accountAuthenticationStatus.OperationMessage;

                return await AccessFailedOnLdapRejectionAsync(user, lockoutOnFailure, failedResult);
            }
        }

        private async Task<SignInResult> ResetAccessFailedCountOnSuccessAsync(TUser user, CustomSignInResult successResult)
        {
            if (!UserManager.SupportsUserLockout)
                return successResult;

            var resetResult = await UserManager.ResetAccessFailedCountAsync(user);
            if (resetResult.Succeeded)
                return successResult;

            Logger.LogWarning("LDAP login succeeded, but resetting the access failed count failed for user.");
            return CustomSignInResult.Failed;
        }

        private async Task<SignInResult> AccessFailedOnLdapRejectionAsync(TUser user, bool lockoutOnFailure, CustomSignInResult failedResult)
        {
            if (!UserManager.SupportsUserLockout || !lockoutOnFailure)
                return failedResult;

            var incrementResult = await UserManager.AccessFailedAsync(user);
            if (!incrementResult.Succeeded)
            {
                Logger.LogWarning("LDAP login failed, but incrementing the access failed count failed for user.");
                return CustomSignInResult.Failed;
            }

            if (!await UserManager.IsLockedOutAsync(user))
                return failedResult;

            return CustomSignInResult.LockedOut.WithMetadataFrom(failedResult);
        }
    }
}

