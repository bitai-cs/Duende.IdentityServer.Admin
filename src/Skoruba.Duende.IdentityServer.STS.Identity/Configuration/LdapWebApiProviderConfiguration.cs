using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Skoruba.Duende.IdentityServer.Admin.EntityFramework.Shared.Entities.Identity;

namespace Skoruba.Duende.IdentityServer.STS.Identity.Configuration
{
	public class LdapWebApiProviderConfiguration<TUserIdentity> where TUserIdentity : class
	{
		public bool UseLdapWebApiProvider { get; set; }
		public LdapWebApiProfile[] LdapWebApiProfiles { get; set; }
		public UserDomainProfile[] UserDomainProfiles { get; set; }


		public LdapWebApiProviderConfiguration()
		{
			UseLdapWebApiProvider = false;
			LdapWebApiProfiles = new LdapWebApiProfile[] { };
			UserDomainProfiles = new UserDomainProfile[] { };
		}


		public void CheckConfigurationIntegrity()
		{
			if (!UseLdapWebApiProvider)
				return;

			if (UserDomainProfiles == null || UserDomainProfiles.Length == 0)
				throw new Exception($"Error in {nameof(LdapWebApiProviderConfiguration<TUserIdentity>)} configuration section. {nameof(UseLdapWebApiProvider)} is enabled, but no Domain profile is defined in {nameof(UserDomainProfiles)}.");

			if (LdapWebApiProfiles == null || LdapWebApiProfiles.Length == 0)
				throw new Exception($"Error in {nameof(LdapWebApiProviderConfiguration<TUserIdentity>)} configuration section. {nameof(UseLdapWebApiProvider)} is enabled, but no LDAP Web Api profile is defined in {nameof(LdapWebApiProfiles)}.");

			var repeatedWebApiProfileIds = LdapWebApiProfiles
				 .GroupBy(p => p.WebApiProfileId)
				 .Where(g => g.Count() > 1)
				 .Select(c => c.Key)
				 .ToArray();

			if (repeatedWebApiProfileIds.Length > 0)
				throw new Exception($"Error in {nameof(LdapWebApiProviderConfiguration<TUserIdentity>)} configuration section. Repeated {nameof(LdapWebApiProfile.WebApiProfileId)} identifier ({String.Join(", ", repeatedWebApiProfileIds)}) in {nameof(LdapWebApiProfiles)}. Take a look at the {nameof(LdapWebApiProviderConfiguration<TUserIdentity>)}.{nameof(LdapWebApiProfiles)} section.");

			var repeatedDomainIds = UserDomainProfiles
				 .GroupBy(p => p.DomainId)
				 .Where(g => g.Count() > 1)
				 .Select(c => c.Key).ToArray();

			if (repeatedDomainIds.Length > 0)
				throw new Exception($"Error in {nameof(LdapWebApiProviderConfiguration<TUserIdentity>)} configuration section. Repeated {nameof(UserDomainProfile.DomainId)} identifier ({string.Join(", ", repeatedDomainIds)}) in {nameof(UserDomainProfiles)}. Take a look at the {nameof(LdapWebApiProviderConfiguration<TUserIdentity>)}.{nameof(UserDomainProfiles)} section.");

			foreach (var udp in UserDomainProfiles)
			{
				if (!LdapWebApiProfiles.Any(f => f.WebApiProfileId == udp.WebApiProfileId))
				{
					throw new Exception($"Error in {nameof(LdapWebApiProviderConfiguration<TUserIdentity>)} configuration section. {nameof(UserDomainProfile.WebApiProfileId)} \"{udp.WebApiProfileId}\" for {nameof(UserDomainProfile.DomainId)} \"{udp.DomainId}\" does not exist in {nameof(LdapWebApiProviderConfiguration<TUserIdentity>)}.{nameof(LdapWebApiProfiles)}");
				}

				udp.LdapWebApiProfile = LdapWebApiProfiles.Where(p => p.WebApiProfileId == udp.WebApiProfileId).Single();
			}
		}

		public UserDomainProfile GetUserDomainProfile(string domainId)
		{
			return UserDomainProfiles.Where(i => i.DomainId == domainId).SingleOrDefault();
		}



		#region Inner classes
		public class LdapWebApiProfile
		{
			public string WebApiProfileId { get; set; }
			public string WebApiUrl { get; set; }
			public bool ClientCredentialRequired { get; set; }
			public Bitai.WebApi.Client.WebApiClientCredential? ClientCredential { get; set; }
			


			public LdapWebApiProfile()
			{
				WebApiProfileId = "webApiProfileId";
				WebApiUrl = "http://localhost/ldapWebApi";
				ClientCredentialRequired = false;
			}
		}

		public class UserDomainProfile
		{
			private Bitai.LDAPWebApi.Clients.LDAPAuthenticationsWebApiClient _ldapAuthenticationsWebApiClient;
			private Bitai.LDAPWebApi.Clients.LDAPUserDirectoryWebApiClient _ldapUsersDirectoryWebApiClient;



			public string DomainId { get; set; }
			public string WebApiProfileId { get; set; }
			public string LdapServerProfile { get; set; }
			public LdapWebApiProfile LdapWebApiProfile { internal set; get; }



			public UserDomainProfile()
			{
				DomainId = "DomainId";
				WebApiProfileId = "webApiProfileId";
				LdapServerProfile = "ldapServerProfile";
				LdapWebApiProfile = new LdapWebApiProfile();
			}



			public Bitai.LDAPWebApi.Clients.LDAPAuthenticationsWebApiClient GetLdapAuthenticationsWebApiClient()
			{
				if (_ldapAuthenticationsWebApiClient == null)
				{
					if (LdapWebApiProfile.ClientCredentialRequired)
						_ldapAuthenticationsWebApiClient = new Bitai.LDAPWebApi.Clients.LDAPAuthenticationsWebApiClient(LdapWebApiProfile.WebApiUrl, LdapServerProfile, false, LdapWebApiProfile.ClientCredential);
					else
						_ldapAuthenticationsWebApiClient = new Bitai.LDAPWebApi.Clients.LDAPAuthenticationsWebApiClient(LdapWebApiProfile.WebApiUrl, LdapServerProfile, false);
				}

				return _ldapAuthenticationsWebApiClient;
			}

			public Bitai.LDAPWebApi.Clients.LDAPUserDirectoryWebApiClient GetLdapUsersDirectoryWebApiClient()
			{
				if (_ldapUsersDirectoryWebApiClient == null)
				{
					if (LdapWebApiProfile.ClientCredentialRequired)
						_ldapUsersDirectoryWebApiClient = new Bitai.LDAPWebApi.Clients.LDAPUserDirectoryWebApiClient(LdapWebApiProfile.WebApiUrl, LdapServerProfile, false, LdapWebApiProfile.ClientCredential);
					else
						_ldapUsersDirectoryWebApiClient = new Bitai.LDAPWebApi.Clients.LDAPUserDirectoryWebApiClient(LdapWebApiProfile.WebApiUrl, LdapServerProfile, false);
				}

				return _ldapUsersDirectoryWebApiClient;
			}
		}
		#endregion
	}
}