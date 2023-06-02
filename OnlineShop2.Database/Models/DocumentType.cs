using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public enum DocumentType
    {
        [Display(Name = "Приход")]
        Arrival,
        [Display(Name = "Списание")]
        Writeof,
        [Display(Name = "Изменение цены товара")]
        PriceChange
    }
}
