using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OnlineShop2.LegacyDb.Models
{
    public enum DocumentStatusLegacy
    {
        New = 0,
        Edit = 1,
        Confirm = 2,
        Remove = 3
    }
}
