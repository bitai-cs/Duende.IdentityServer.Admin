﻿using System.ComponentModel.DataAnnotations;
using Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Identity.Dtos.Identity.Base;
using Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Identity.Dtos.Identity.Interfaces;

namespace Skoruba.Duende.IdentityServer.Admin.BusinessLogic.Identity.Dtos.Identity
{
    public class UserClaimDto<TKey> : BaseUserClaimDto<TKey>, IUserClaimDto
    {
        [Required]
        public string ClaimType { get; set; }

        [Required]
        public string ClaimValue { get; set; }
    }
}