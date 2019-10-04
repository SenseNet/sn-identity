using SenseNet.Search;

namespace SenseNet.Identity.Experimental
{
    internal class IdentityQueries : ISafeQueryHolder
    {
        /// <summary>Returns the following query: +TypeIs:User +Email:@0</summary>
        public static string UsersByEmail => "+TypeIs:User +Email:@0";
    }
}
