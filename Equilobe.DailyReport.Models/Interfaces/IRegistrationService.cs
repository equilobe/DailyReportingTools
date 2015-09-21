using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IRegistrationService : IService
    {
        SimpleResult RegisterUser(RegisterModel model);

        SimpleResult ConfirmEmail(EmailConfirmation emailConfirmation);

        SimpleResult Login(LoginModel model);

        void Logout();

        bool IsTrialAvailableForInstance(long instanceId);

        void ValidateJiraDetails(RegisterModel model);

        SimpleResult SendResetPasswordEmail(string email);

        SimpleResult ValidateResetPasswordToken(EmailConfirmation emailConfirmation);

        SimpleResult ResetPassword(ResetPasswordModel passwordModel);
    }
}
