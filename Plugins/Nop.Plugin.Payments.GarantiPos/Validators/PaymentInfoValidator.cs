using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Payments.GarantiPos.Models; // added

namespace Nop.Plugin.Payments.GarantiPos.Validators;

public class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
{
    public PaymentInfoValidator(ILocalizationService localization)
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty()
                .WithMessageAwait(localization.GetResourceAsync("Payment.CardNumber.Required"))
            .CreditCard()
                .WithMessageAwait(localization.GetResourceAsync("Payment.CardNumber.Wrong"));

        RuleFor(x => x.CardCode)
            .NotEmpty()
                .WithMessageAwait(localization.GetResourceAsync("Payment.CardCode.Required"))
            .Matches("^[0-9]{3,4}$")
                .WithMessageAwait(localization.GetResourceAsync("Payment.CardCode.Wrong"));

        RuleFor(x => x.CardholderName)
            .NotEmpty()
                .WithMessageAwait(localization.GetResourceAsync("Payment.CardholderName.Required"));

        RuleFor(x => x.ExpireYear)
            .NotEmpty()
                .WithMessageAwait(localization.GetResourceAsync("Payment.ExpireYear.Required"));

        RuleFor(x => x.ExpireMonth)
            .NotEmpty()
                .WithMessageAwait(localization.GetResourceAsync("Payment.ExpireMonth.Required"));
    }
}
