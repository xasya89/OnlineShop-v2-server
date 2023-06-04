using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OnlineShop2.Dao
{
    public enum SpecialTypes
    {
        [Display(Name = "")]
        None,
        [Display(Name = "Пиво")]
        Beer,
        [Display(Name = "Тара")]
        Bottle,
        [Display(Name = "Пакет")]
        Bag
    }
}
