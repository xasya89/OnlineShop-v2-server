using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Dao.Extensions
{
    public static class EnumHelper
    {
        public static string? GetDisplayName(this Enum value)
        {
            var attribute = value.GetType().GetMember(value.ToString()).First().GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name;
        }
    }
}
