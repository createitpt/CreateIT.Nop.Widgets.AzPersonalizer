namespace Nop.Plugin.Widgets.AzPersonalizer.Utils.Handler
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Methods for building the chain of command.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHandler<T>
    {
        /// <summary>
        ///     Solves the chain of responsibility, then returns the result.
        /// </summary>
        /// <returns></returns>
        Task<T> HandleAsync ();

        /// <summary>
        /// Solves the chain of responsibility, doesnt return anything.
        /// </summary>
        /// <param name="request"></param>
        // void HandleVoid (T request);

        /// <summary>
        ///     Chains the next handler of the chain of responsibility.
        /// </summary>
        /// <param name="nextHandler"></param>
        /// <returns></returns>
        IHandler<T> SetNext (IHandler<T> nextHandler);
    }
}
