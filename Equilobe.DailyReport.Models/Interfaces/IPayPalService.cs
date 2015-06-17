﻿using Equilobe.DailyReport.Models.Paypal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IPayPalService : IService
    {
        void GetStatus(byte[] parameters, PayPalCheckoutInfo payPalCheckoutInfo);
    }
}
