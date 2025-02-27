namespace Skoruba.Duende.IdentityServer.STS.Identity.Helpers
{
	public class CustomSignInResult: Microsoft.AspNetCore.Identity.SignInResult
	{
        public int HttpStatusCode { get; internal set; }
        public string HttpReasonPhrase { get; internal set; }
        public string HttpContentType { get; internal set; }
        public string HttpResponseContent { get; internal set; }
        



        public CustomSignInResult(): base() { }




        #region Static members & methods
        private static readonly CustomSignInResult _success = new CustomSignInResult { Succeeded = true };
        private static readonly CustomSignInResult _failed = new CustomSignInResult();
        private static readonly CustomSignInResult _lockedOut = new CustomSignInResult { IsLockedOut = true };
        private static readonly CustomSignInResult _notAllowed = new CustomSignInResult { IsNotAllowed = true };
        private static readonly CustomSignInResult _twoFactorRequired = new CustomSignInResult { RequiresTwoFactor = true };




        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a successful sign-in.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents a successful sign-in.</returns>
        public new static CustomSignInResult Success => _success;

        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a failed sign-in.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents a failed sign-in.</returns>
        public new static CustomSignInResult Failed => _failed;

        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a sign-in attempt that failed because
        /// the user was locked out.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents sign-in attempt that failed due to the
        /// user being locked out.</returns>
        public new static CustomSignInResult LockedOut => _lockedOut;

        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a sign-in attempt that failed because
        /// the user is not allowed to sign-in.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents sign-in attempt that failed due to the
        /// user is not allowed to sign-in.</returns>
        public new static CustomSignInResult NotAllowed => _notAllowed;

        /// <summary>
        /// Returns a <see cref="SignInResult"/> that represents a sign-in attempt that needs two-factor
        /// authentication.
        /// </summary>
        /// <returns>A <see cref="SignInResult"/> that represents sign-in attempt that needs two-factor
        /// authentication.</returns>
        public new static CustomSignInResult TwoFactorRequired => _twoFactorRequired;
        #endregion
    }
}
