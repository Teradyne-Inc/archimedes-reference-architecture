using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#if HAS_ULTRAEDGE
using Teradyne.Igxl.Utilities;
#endif

namespace UltraEdgeTools
{
    /// <summary>
    /// This class takes care of the loading of DLLs required for UltraEdge operations.
    /// Application-level classes need to use this class to access the UltraEdge object.
    /// </summary>
    public class UEProxy
    {
#if HAS_ULTRAEDGE
        private const String UE_DLL = @"C:\Program Files (x86)\Teradyne\UltraEdge\UltraEdge.dll";
        private const String UENATIVE_DLL = @"C:\Program Files (x86)\Teradyne\UltraEdge\UltraEdgeNative.dll";

        // Handle to the UltraEdge object
        public UltraEdge UltraEdge { get; private set; }
#else
        private const String UE_DLL = @"C:\Program Files (x86)\Teradyne\UltraEdge\UltraEdge.dll";
        private const String UENATIVE_DLL = @"C:\Program Files (x86)\Teradyne\UltraEdge\UltraEdgeNative.dll";

        // Handle to the UltraEdge object - stub implementation when DLL is not available
        public object UltraEdge { get; private set; }
#endif

        // Singleton
        private static UEProxy _instance = null;
        private UEProxy()
        {
#if HAS_ULTRAEDGE
            UltraEdge = new UltraEdge();
#else
            throw new NotSupportedException("UltraEdge.dll is not available. This is reference code that requires the UltraEdge software to be installed.");
#endif
        }

        /// <summary>
        /// Factory including the AppDomain resolution for the UltraEdge DLL
        /// </summary>
        public static UEProxy GetInstance()
        {
            if (_instance == null)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                _instance = new UEProxy();
            }
            return _instance;
        }

        /// <summary>
        /// AppDomain resolution for the UltraEdge DLL
        /// </summary>
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
#if HAS_ULTRAEDGE
            Assembly a = null;
            if (args.Name.Contains("UltraEdgeNative,"))
                a = Assembly.LoadFrom(UENATIVE_DLL);
            else if (args.Name.Contains("UltraEdge,"))
                a = Assembly.LoadFrom(UE_DLL);
            return a;
#else
            return null;
#endif
        }
    }
}
