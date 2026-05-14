namespace Skoruba.Duende.IdentityServer.STS.Identity.Helpers
{
	public class CustomSignInResult: Microsoft.AspNetCore.Identity.SignInResult
	{
        public int HttpStatusCode { get; internal set; }
        public string HttpReasonPhrase { get; internal set; }
        public string HttpContentType { get; internal set; }
        public string HttpResponseContent { get; internal set; }
        



        public CustomSignInResult(): base() { }

        public CustomSignInResult WithMetadataFrom(CustomSignInResult other)
        {
            HttpStatusCode = other.HttpStatusCode;
            HttpReasonPhrase = other.HttpReasonPhrase;
            HttpContentType = other.HttpContentType;
            HttpResponseContent = other.HttpResponseContent;

            return this;
        }




        #region Static members & methods
        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a successful sign-in.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents a successful sign-in.</returns>
        public new static CustomSignInResult Success => new CustomSignInResult { Succeeded = true };

        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a failed sign-in.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents a failed sign-in.</returns>
        public new static CustomSignInResult Failed => new CustomSignInResult();

        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a sign-in attempt that failed because
        /// the user was locked out.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents sign-in attempt that failed due to the
        /// user being locked out.</returns>
        public new static CustomSignInResult LockedOut => new CustomSignInResult { IsLockedOut = true };

        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a sign-in attempt that failed because
        /// the user is not allowed to sign-in.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents sign-in attempt that failed due to the
        /// user is not allowed to sign-in.</returns>
        public new static CustomSignInResult NotAllowed => new CustomSignInResult { IsNotAllowed = true };

        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a sign-in attempt that needs two-factor
        /// authentication.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents sign-in attempt that needs two-factor
        /// authentication.</returns>
        public new static CustomSignInResult TwoFactorRequired => new CustomSignInResult { RequiresTwoFactor = true };
        #endregion
    }
}
