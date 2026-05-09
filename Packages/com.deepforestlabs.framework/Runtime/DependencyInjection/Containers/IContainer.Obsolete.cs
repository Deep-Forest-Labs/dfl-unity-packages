#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs
{
    public partial interface IContainer
    {
        /// <summary>
        /// For legacy work not yet ported.
        /// </summary>
        /// <param name="instance"></param>
        //[Obsolete("//TODO [2.5.+] - Create JIRA -determine if we need/want this as it bypasses transients and scopes", false)]
        void Inject(object instance);
    }
}
#nullable disable