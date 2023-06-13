using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Dao
{
    public enum TypeSell
    {
        [Display(Name = "Приход")]
        Sell = 0,
        [Display(Name = "Возврат")]
        Return = 1
    }
}
