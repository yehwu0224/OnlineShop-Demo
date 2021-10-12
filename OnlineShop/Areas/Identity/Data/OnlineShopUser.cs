using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OnlineShop.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the OnlineShopUser class
    public class OnlineShopUser : IdentityUser
    {
        [PersonalData]
        public string Name { get; set; }
        [PersonalData]
        public DateTime DOB { get; set; }

        public GenderType Gender { get; set; }
        
    }

    public enum GenderType
    {
        Male, Female, Unknown
    }
}
