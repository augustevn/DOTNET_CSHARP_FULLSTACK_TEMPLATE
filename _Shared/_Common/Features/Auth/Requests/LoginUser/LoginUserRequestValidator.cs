using FluentValidation;

namespace _Common.Features.Auth.Requests.LoginUser;

public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        RuleFor(request => request.Email)
            .EmailAddress()
            .NotEmpty().WithMessage("Email should not be empty");

        RuleFor(request => request.Password)
            .MinimumLength(8)
            .NotEmpty();
    }
}