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
        /// <param name="user">User identity. See <see cref="Admin.EntityFramework.Shared.Entities.Identity.UserIdentity"/>.</param>
        /// <param name="password">User password.</param>
        /// <param name="lockoutOnFailure">Whether or not to lockout the user account when password validation fails.</param>
        /// <returns><see cref="SignInResult"/></returns>
        public override async Task<SignInResult> CheckPasswordSignInAsync(TUser user, string password, bool lockoutOnFailure)
        {
            var userIdentity = user as Admin.EntityFramework.Shared.Entities.Identity.UserIdentity;

            var userDomainProfile = _ldapWebApiProvider.GetUserDomainProfile(userIdentity.UserDomain);

            // The user does not have an assigned domain or the user's domain is not registered to access the LDAP Web Api.
            if (userDomainProfile == null)
                return await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);

            var ldapAccountCredentials = new Bitai.LDAPHelper.DTO.LDAPDomainAccountCredential
            {
                DomainName = userIdentity.UserDomain,
                AccountName = userIdentity.UserName.Split('\\').Last(),
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
                Logger.LogError($"Unsuccessful response when try to authenticate user credentials by LDAP Web Api. Response code: {(int)httpResponse.HttpStatusCode} ({httpResponse.ReasonPhrase}). Content Type: {httpResponse.ContentMediaType}");
                Logger.LogError(responseContent);
                #endregion
                
                CustomSignInResult _customResult;
                
                if (httpResponse.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
                    _customResult = CustomSignInResult.NotAllowed;
                else
                    _customResult = CustomSignInResult.Failed;

                _customResult.HttpStatusCode = (int)httpResponse.HttpStatusCode;
                _customResult.HttpReasonPhrase = httpResponse.ReasonPhrase + middlewareException != null ? $"{middlewareException.Source}: {middlewareException.Message}" : string.Empty;
                _customResult.HttpContentType = httpResponse.ContentMediaType.ToString();
                _customResult.HttpResponseContent = responseContent;

                return _customResult;
            }
            else
            {
                var accountAuthenticationStatus = ldapAuthenticationsWebApiClient.GetDTOFromResponse<Bitai.LDAPHelper.DTO.LDAPDomainAccountAuthenticationResult>(httpResponse);

                CustomSignInResult _customResult;

                if (accountAuthenticationStatus.IsAuthenticated)
                    _customResult = CustomSignInResult.Success;
                else
                    _customResult = CustomSignInResult.NotAllowed;

                _customResult.HttpStatusCode = (int)httpResponse.HttpStatusCode;
                _customResult.HttpReasonPhrase = accountAuthenticationStatus.OperationMessage;

                return _customResult;
            }
        }
    }
}

