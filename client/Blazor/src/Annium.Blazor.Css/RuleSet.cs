using System.Threading.Tasks;

namespace Annium.Blazor.Css
{
    public abstract class RuleSet
    {
        protected RuleSet()
        {
            Task.Run(() => Internal.StyleSheet.Instance.Render(this));
        }
    }
}