using System.CommandLine;
using System.Linq.Expressions;

namespace CertTools.CommandBuilders;
internal static class OptionBuilder
{
    internal static CommandBuilder<TOptionHolder> For<TOptionHolder>() where TOptionHolder : class
    {
        return new CommandBuilder<TOptionHolder>();
    }
}
internal class OptionBuilder<TOptionHolder, TOption>(CommandBuilder<TOptionHolder> commandHandlerBuilder, Expression<Func<TOptionHolder, TOption>> propertyExpression)
{
    private readonly Option<TOption> option = new(ToKebabCase(GetPropertyName(propertyExpression)));

    private static string ToKebabCase(string optionName)
    {
        return NameExtensions.ToKebabCase("--", optionName);
    }

    private static string GetPropertyName(Expression<Func<TOptionHolder, TOption>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        throw new InvalidOperationException("The provided expression must be a simple property access (e.g., x => x.PropertyName).");
    }

    internal CommandBuilder<TOptionHolder> AddTo(Command command)
    {
        command.Add(option);
        return commandHandlerBuilder;
    }

    internal OptionBuilder<TOptionHolder, TOption> Configure(Action<Option<TOption>> value)
    {
        value.Invoke(option);
        return this;
    }
}
