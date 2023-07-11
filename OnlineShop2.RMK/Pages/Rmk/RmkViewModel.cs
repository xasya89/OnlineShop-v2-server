using Microsoft.Extensions.Logging;
using OnlineShop2.RMK.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.RMK.Pages.Rmk
{
    internal class RmkViewModel:ViewModelBase
    {
        public RmkViewModel(ILogger<RmkViewModel> logger)
        {
            logger.LogInformation("hello");
        }
    }
}
