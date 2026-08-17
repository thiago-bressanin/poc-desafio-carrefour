using FluentValidation;

namespace CashFlow.Transactions.Application.Commands.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("O identificador do lojista (MerchantId) é obrigatório.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("O tipo de lançamento é obrigatório.")
            .Must(type => type.Equals("Credit", StringComparison.OrdinalIgnoreCase) || 
                          type.Equals("Debit", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Credito", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Debito", StringComparison.OrdinalIgnoreCase))
            .WithMessage("O tipo de lançamento deve ser 'Credit' (Crédito) ou 'Debit' (Débito).");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("O valor do lançamento deve ser maior que zero (0.00).");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("A data do lançamento é obrigatória.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(250).WithMessage("A descrição não pode ter mais de 250 caracteres.");
    }
}
