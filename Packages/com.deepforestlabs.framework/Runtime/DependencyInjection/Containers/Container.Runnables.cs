#nullable enable
using System.Threading;
using DeepForestLabs.BuildSystems;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Common;
using UnityEngine;

namespace DeepForestLabs
{
    internal sealed partial class Container
    {
        public async UniTask Run(IRunnable runnable, CancellationToken token)
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                // Enter Log
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0}", InternalUtils.FormatTypeName(runnable.GetType())), ContainerLogFlag.Running);

                // Run it
                await runnable.Run(scope.Token);

                // Exit Log
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(runnable.GetType())), ContainerLogFlag.Running);
            }
        }

        public async UniTask Run<TRunnable>(CancellationToken token)
            where TRunnable : class, IRunnable
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0}", InternalUtils.FormatTypeName(typeof(TRunnable))), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable>(scope.Token);

                await runnable.Run(scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(typeof(TRunnable))), ContainerLogFlag.Running);
            }
        }

        public async UniTask Run<TRunnable, TArgs>(TArgs args, CancellationToken token) 
            where TRunnable : class, IRunnable
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs).Name, ToArgsString(args)), ContainerLogFlag.Running);

                TRunnable runnable = await Create<TRunnable, TArgs>(args, scope.Token);

                await runnable.Run(scope.Token);
                
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(typeof(TRunnable))), ContainerLogFlag.Running);
            }
        }

        public async UniTask Run<TRunnable, TArgs1, TArgs2>(TArgs1 args1, TArgs2 args2, CancellationToken token) 
            where TRunnable : class, IRunnable
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs2).Name, ToArgsString(args2)), ContainerLogFlag.Running);

                TRunnable runnable = await Create<TRunnable, TArgs1, TArgs2>(args1, args2, scope.Token);

                await runnable.Run(scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(typeof(TRunnable))), ContainerLogFlag.Running);
            }
        }

        public async UniTask Run<TRunnable, TArgs1, TArgs2, TArgs3>(TArgs1 args1, TArgs2 args2, TArgs3 args3, 
            CancellationToken token) 
            where TRunnable : class, IRunnable
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs2).Name, ToArgsString(args2)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs3).Name, ToArgsString(args3)), ContainerLogFlag.Running);

                TRunnable runnable = await Create<TRunnable, TArgs1, TArgs2, TArgs3>(args1, args2, args3, scope.Token);

                await runnable.Run(scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(typeof(TRunnable))), ContainerLogFlag.Running);
            }
        }

        public async UniTask<TResult> Run<TResult>(IRunnableResult<TResult> runnable, CancellationToken token)
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0}", InternalUtils.FormatTypeName(runnable.GetType())), ContainerLogFlag.Running);

                TResult result = await runnable.Run(scope.Token);
                
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0} with", InternalUtils.FormatTypeName(runnable.GetType())) +
                    ZString.Format("\n{0}:\n{1}", InternalUtils.FormatTypeName(typeof(TResult)), ToArgsString(result)), ContainerLogFlag.Running);

                return result;
            }
        }

        public async UniTask<TResult> Run<TRunnable, TResult>(CancellationToken token)
            where TRunnable : class, IRunnableResult<TResult>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0}", InternalUtils.FormatTypeName(typeof(TRunnable))), ContainerLogFlag.Running);

                TRunnable runnable = await Create<TRunnable>(scope.Token);

                TResult result = await runnable.Run(scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0} with", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("\n{0}:\n{1}", InternalUtils.FormatTypeName(typeof(TResult)), ToArgsString(result)), ContainerLogFlag.Running);

                return result;
            }
        }

        public async UniTask<TResult> Run<TRunnable, TArgs, TResult>(TArgs args, CancellationToken token) 
            where TRunnable : class, IRunnableResult<TResult>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs).Name, ToArgsString(args)), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable, TArgs>(args, scope.Token);

                TResult result = await runnable.Run(scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0} with", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("\n{0}:\n{1}", InternalUtils.FormatTypeName(typeof(TResult)), ToArgsString(result)), ContainerLogFlag.Running);
                
                return result;
            }
        }

        public async UniTask<TResult> Run<TRunnable, TArgs1, TArgs2, TResult>(TArgs1 args1, TArgs2 args2, 
            CancellationToken token) 
            where TRunnable : class, IRunnableResult<TResult>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs2).Name, ToArgsString(args2)), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable, TArgs1, TArgs2>(args1, args2, scope.Token);

                TResult result = await runnable.Run(scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0} with", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("\n{0}:\n{1}", InternalUtils.FormatTypeName(typeof(TResult)), ToArgsString(result)), ContainerLogFlag.Running);
                
                return result;
            }
        }

        public async UniTask<TResult> Run<TRunnable, TArgs1, TArgs2, TArgs3, TResult>(TArgs1 args1, TArgs2 args2, 
            TArgs3 args3, CancellationToken token) 
            where TRunnable : class, IRunnableResult<TResult>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs2).Name, ToArgsString(args2)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs3).Name, ToArgsString(args3)), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable, TArgs1, TArgs2, TArgs3>(args1, args2, args3, scope.Token);

                TResult result = await runnable.Run(scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0} with", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("\n{0}:\n{1}", InternalUtils.FormatTypeName(typeof(TResult)), ToArgsString(result)), ContainerLogFlag.Running);
                
                return result;
            }
        }

        public async UniTask RunWith<TArgs>(IRunnableWith<TArgs> runnable, TArgs args, CancellationToken token)
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(runnable.GetType())) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs).Name, ToArgsString(args)), ContainerLogFlag.Running);

                await runnable.Run(args, scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(runnable.GetType())), ContainerLogFlag.Running);
            }
        }

        public async UniTask RunWith<TRunnable, TArgs>(TArgs args, CancellationToken token)
            where TRunnable : class, IRunnableWith<TArgs>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs).Name, ToArgsString(args)), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable>(scope.Token);

                await runnable.Run(args, scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(typeof(TRunnable))), ContainerLogFlag.Running);
            }
        }
        
        public async UniTask RunWith<TArgs1, TArgs2>(IRunnableWith<TArgs1, TArgs2> runnable, TArgs1 args1, TArgs2 args2, CancellationToken token)
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(runnable.GetType())) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs2).Name, ToArgsString(args2)), ContainerLogFlag.Running);

                await runnable.Run(args1, args2, scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(runnable.GetType())), ContainerLogFlag.Running);
            }
        }

        public async UniTask RunWith<TRunnable, TArgs1, TArgs2>(TArgs1 args1, TArgs2 args2, CancellationToken token)
            where TRunnable : class, IRunnableWith<TArgs1, TArgs2>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs2).Name, ToArgsString(args2)), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable>(scope.Token);

                await runnable.Run(args1, args2, scope.Token);
                
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(typeof(TRunnable))), ContainerLogFlag.Running);
            }
        }
        
        public async UniTask RunWith<TArgs1, TArgs2, TArgs3>(IRunnableWith<TArgs1, TArgs2, TArgs3> runnable, 
            TArgs1 args1, TArgs2 args2, TArgs3 args3, CancellationToken token)
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(runnable.GetType())) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs2).Name, ToArgsString(args2)), ContainerLogFlag.Running);

                await runnable.Run(args1, args2, args3, scope.Token);
              
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(runnable.GetType())), ContainerLogFlag.Running);
            }
        }
        
        public async UniTask RunWith<TRunnable, TArgs1, TArgs2, TArgs3>(TArgs1 args1, TArgs2 args2, TArgs3 args3, CancellationToken token)
            where TRunnable : class, IRunnableWith<TArgs1, TArgs2, TArgs3>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs2).Name, ToArgsString(args2)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs3).Name, ToArgsString(args3)), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable>(scope.Token);

                await runnable.Run(args1, args2, args3, scope.Token);
                
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0}", InternalUtils.FormatTypeName(typeof(TRunnable))), ContainerLogFlag.Running);
            }
        }

        public async UniTask<TResult> RunWith<TRunnable, TArgs, TResult>(TArgs args, CancellationToken token)
            where TRunnable : class, IRunnableWithResult<TArgs, TResult>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs).Name, ToArgsString(args)), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable>(scope.Token);

                TResult result = await runnable.Run(args, scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0} with", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("\n{0}:\n{1}", InternalUtils.FormatTypeName(typeof(TResult)), ToArgsString(result)), ContainerLogFlag.Running);
                
                return result;
            }
        }

        public async UniTask<TResult> RunWith<TRunnable, TArgs1, TArgs2, TResult>(TArgs1 args1, TArgs2 args2,
            CancellationToken token)
            where TRunnable : class, IRunnableWithResult<TArgs1, TArgs2, TResult>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs2).Name, ToArgsString(args2)), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable>(scope.Token);

                TResult result = await runnable.Run(args1, args2, scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0} with", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("\n{0}:\n{1}", InternalUtils.FormatTypeName(typeof(TResult)), ToArgsString(result)), ContainerLogFlag.Running);

                return result;
            }
        }

        public async UniTask<TResult> RunWith<TRunnable, TArgs1, TArgs2, TArgs3, TResult>(TArgs1 args1, TArgs2 args2, TArgs3 args3,
            CancellationToken token) where TRunnable : class, IRunnableWithResult<TArgs1, TArgs2, TArgs3, TResult>
        {
            using (CancelOnDisposeTokenSource scope = new(token, Scope))
            {
                InternalUtils.VerboseLog(_name,
                    ZString.Format("Entering {0} with \n", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("{0}:\n{1}\n", typeof(TArgs1).Name, ToArgsString(args1)) +
                    ZString.Format("{0}:\n{1}", typeof(TArgs2).Name, ToArgsString(args2)), ContainerLogFlag.Running);
                
                TRunnable runnable = await Create<TRunnable>(scope.Token);

                TResult result = await runnable.Run(args1, args2, args3, scope.Token);

                InternalUtils.VerboseLog(_name,
                    ZString.Format("Exiting {0} with", InternalUtils.FormatTypeName(typeof(TRunnable))) +
                    ZString.Format("\n{0}:\n{1}", InternalUtils.FormatTypeName(typeof(TResult)), ToArgsString(result)), ContainerLogFlag.Running);

                return result;
            }
        }

#if RELEASE_BUILD
        private string ToArgsString(object? obj) => string.Empty;
#else
        
        private string ToArgsString(object? obj)
        {
            if (obj == null)
            {
                return "null";
            }
            string result = string.Empty;
            if (DeepForestLabs.Main.ToStringOverride != null)
            {
                result = DeepForestLabs.Main.ToStringOverride(this, obj);
            }
            if (string.IsNullOrWhiteSpace(result))
            {
                result = JsonUtility.ToJson(obj, true);
            }
            if (string.IsNullOrWhiteSpace(result))
            {
                result = obj.ToString();
            }
            return result;
        }
#endif
    }
}
#nullable disable