using System.Linq.Expressions;

namespace CertTools.CommandBuilders;

internal class CommandBuilder<TOptionHolder>
{
    internal OptionBuilder<TOptionHolder, TOption> NewOption<TOption>(Expression<Func<TOptionHolder, TOption>> propertyExpression)
    {
        return new OptionBuilder<TOptionHolder, TOption>(this, propertyExpression);
    }
}