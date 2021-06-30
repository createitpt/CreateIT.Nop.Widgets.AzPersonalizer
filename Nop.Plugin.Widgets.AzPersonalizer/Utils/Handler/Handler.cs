using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.AzPersonalizer.Utils.Handler
{

    public abstract class Handler<T>
    {
        private IHandler<T> Next { get; set; }

        public async virtual Task<T> Handle()
        {

            T aux = default;
            if (Next != null)
            {
                aux = await Next.Handle();
            }
            return aux;
        }

        public virtual void HandleVoid(T request)
        {
            Next?.HandleVoid(request);
        }
        public IHandler<T> SetNext(IHandler<T> next)
        {
            Next = next;
            return Next;
        }
    }

}
