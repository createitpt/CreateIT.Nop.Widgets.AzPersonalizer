namespace Nop.Plugin.Widgets.AzPersonalizer.Utils.Handler
{

    using System.Threading.Tasks;

    public abstract class Handler<T>
    {
        private IHandler<T> Next { get; set; }

        public async virtual Task<T> HandleAsync ()
        {
            T aux = default;
            if (Next != null)
            {
                aux = await Next.HandleAsync();
            }

            return aux;
        }

        public IHandler<T> SetNext (IHandler<T> next)
        {
            Next = next;
            return Next;
        }
    }
}
