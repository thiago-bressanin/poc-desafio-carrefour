namespace CashFlow.BuildingBlocks.Domain;

public class BusinessRuleException : Exception
{
    public string Code { get; }

    public BusinessRuleException(string message, string code = "BUSINESS_RULE_VIOLATION") 
        : base(message)
    {
        Code = code;
    }
}
