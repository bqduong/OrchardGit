using Orchard;
using System;

namespace Magelia.WebStore.Contracts
{
    public interface IUserModelsStateServices : IDependency
    {
        void FlushUserContext();
        void FlushCommerceContext();
        T GetFromUserContext<T>(String type, Int32 id) where T : class, new();
        T GetFromCommerceContext<T>(String type, Int32 id) where T : class, new();
    }
}
