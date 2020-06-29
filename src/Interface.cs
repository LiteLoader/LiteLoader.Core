using LiteLoader.Pooling;

namespace LiteLoader
{
    public static class Interface
    {
        public static ILiteLoader CoreModule { get; private set; }

        #region Initialization

        /// <summary>
        /// Begins shutdown and cleanup of LiteLoader
        /// </summary>
        public static void Shutdown()
        {
            if (CoreModule != null)
            {
                ((LiteLoader)CoreModule).Unload();
                CoreModule = null;
            }
        }

        /// <summary>
        /// Begins startup of LiteLoader
        /// </summary>
        public static void Startup(string gameAssembly)
        {
            if (CoreModule == null)
            {
                var l = new LiteLoader(gameAssembly);
                CoreModule = l;
                l.Load();
            }
        }

        #endregion

        #region Hook Execution

        /// <summary>
        /// Starts hook execution
        /// </summary>
        /// <param name="hookName">Name of the hook to call</param>
        /// <param name="arguments">Array of object parameter values used for hook execution</param>
        /// <returns>The result of the hook execution</returns>
        public static object ExecuteHook(string hookName, object[] arguments)
        {
            // TODO: Setup Hook Execution
            return null;
        }

        /// <summary>
        /// Starts hook execution with 0 arguments
        /// </summary>
        /// <param name="hookName">Name of the hook to call</param>
        /// <returns>The result of the hook execution</returns>
        public static object ExecuteHook(string hookName)
        {
            object[] args = Pool.Array<object>(0);
            object result = ExecuteHook(hookName, args);
            Pool.Free(args);
            return result;
        }

        /// <summary>
        /// Starts hook execution with 1 arguments
        /// </summary>
        /// <param name="hookName">Name of the hook to call</param>
        /// <param name="a0">Value of argument index 0</param>
        /// <returns>The result of the hook execution</returns>
        public static object ExecuteHook(string hookName, object a0)
        {
            object[] args = Pool.Array<object>(1);
            args[0] = a0;
            object result = ExecuteHook(hookName, args);
            Pool.Free(args);
            return result;
        }

        /// <summary>
        /// Starts hook execution with 2 arguments
        /// </summary>
        /// <param name="hookName">Name of the hook to call</param>
        /// <param name="a0">Value of argument index 0</param>
        /// <param name="a1">Value of argument index 1</param>
        /// <returns>The result of the hook execution</returns>
        public static object ExecuteHook(string hookName, object a0, object a1)
        {
            object[] args = Pool.Array<object>(2);
            args[0] = a0;
            args[1] = a1;
            object result = ExecuteHook(hookName, args);
            Pool.Free(args);
            return result;
        }

        /// <summary>
        /// Starts hook execution with 3 arguments
        /// </summary>
        /// <param name="hookName">Name of the hook to call</param>
        /// <param name="a0">Value of argument index 0</param>
        /// <param name="a1">Value of argument index 1</param>
        /// <param name="a2">Value of argument index 2</param>
        /// <returns>The result of the hook execution</returns>
        public static object ExecuteHook(string hookName, object a0, object a1, object a2)
        {
            object[] args = Pool.Array<object>(3);
            args[0] = a0;
            args[1] = a1;
            args[2] = a2;
            object result = ExecuteHook(hookName, args);
            Pool.Free(args);
            return result;
        }

        /// <summary>
        /// Starts hook execution with 4 arguments
        /// </summary>
        /// <param name="hookName">Name of the hook to call</param>
        /// <param name="a0">Value of argument index 0</param>
        /// <param name="a1">Value of argument index 1</param>
        /// <param name="a2">Value of argument index 2</param>
        /// <param name="a3">Value of argument index 3</param>
        /// <returns>The result of the hook execution</returns>
        public static object ExecuteHook(string hookName, object a0, object a1, object a2, object a3)
        {
            object[] args = Pool.Array<object>(4);
            args[0] = a0;
            args[1] = a1;
            args[2] = a2;
            args[3] = a3;
            object result = ExecuteHook(hookName, args);
            Pool.Free(args);
            return result;
        }

        /// <summary>
        /// Starts hook execution with 5 arguments
        /// </summary>
        /// <param name="hookName">Name of the hook to call</param>
        /// <param name="a0">Value of argument index 0</param>
        /// <param name="a1">Value of argument index 1</param>
        /// <param name="a2">Value of argument index 2</param>
        /// <param name="a3">Value of argument index 3</param>
        /// <param name="a4">Value of argument index 4</param>
        /// <returns>The result of the hook execution</returns>
        public static object ExecuteHook(string hookName, object a0, object a1, object a2, object a3, object a4)
        {
            object[] args = Pool.Array<object>(5);
            args[0] = a0;
            args[1] = a1;
            args[2] = a2;
            args[3] = a3;
            args[4] = a4;
            object result = ExecuteHook(hookName, args);
            Pool.Free(args);
            return result;
        }

        #endregion
    }
}
