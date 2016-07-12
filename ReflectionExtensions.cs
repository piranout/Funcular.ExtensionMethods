using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Funcular.ExtensionMethods
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// For .NET 4.0 and below. In .NET 4.5 and later, use the 
        /// CallerNameAttribute instead. . Note: In release mode, unit 
        /// tests will fail because of test instrumentation.  
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethodName()
        {
            string ret = null;
            var stackTrace = new StackTrace(); // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames(); // get method calls (frames)
            int i = 0;
            // write call stack method names
            if (stackFrames != null)
                foreach (var stackFrame in stackFrames)
                {
                    i++;
                    if (i >= 2)
                    {
                        ret = stackFrame.GetMethod().Name;
                        break;
                    }
                }
            return ret;
        }

        /// <summary>
        /// For .NET 4.0 and below. In .NET 4.5 and later, use the 
        /// CallerNameAttribute instead. Note: In release mode, unit 
        /// tests will fail because of test instrumentation.  
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCallingMethodName()
        {
            string ret = null;
            var stackTrace = new StackTrace(); // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames(); // get method calls (frames)
            int i = 0;
            if (stackFrames != null)
                foreach (var stackFrame in stackFrames)
                {
                    i++;
                    if (i > 2)
                    {
                        ret = stackFrame.GetMethod().Name;
                        break;
                    }
                }
            return ret;
        }
    }
}