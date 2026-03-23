using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models.Options
{
    public class BootstrapAdminOptionsValidator : IValidateOptions<BootstrapAdminOptions>
    {
        private static readonly EmailAddressAttribute EmailValidator = new();

        public ValidateOptionsResult Validate(string? name, BootstrapAdminOptions options)
        {
            if (!options.Enabled)
            {
                return ValidateOptionsResult.Success;
            }

            if (string.IsNullOrWhiteSpace(options.Email))
            {
                return ValidateOptionsResult.Fail("BootstrapAdmin:Email è obbligatoria quando BootstrapAdmin:Enabled è true.");
            }

            if (!EmailValidator.IsValid(options.Email))
            {
                return ValidateOptionsResult.Fail("BootstrapAdmin:Email deve essere un indirizzo email valido quando BootstrapAdmin:Enabled è true.");
            }

            if (string.IsNullOrWhiteSpace(options.Password))
            {
                return ValidateOptionsResult.Fail("BootstrapAdmin:Password è obbligatoria quando BootstrapAdmin:Enabled è true.");
            }

            if (options.Password.Length < 12)
            {
                return ValidateOptionsResult.Fail("BootstrapAdmin:Password deve essere lunga almeno 12 caratteri quando BootstrapAdmin:Enabled è true.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
