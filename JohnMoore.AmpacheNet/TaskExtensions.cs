﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    /// <summary>
    /// http://blogs.msdn.com/b/pfxteam/archive/2011/11/10/10235834.aspx
    /// </summary>
    public static class TaskExtensions
    {
        internal struct VoidTypeStruct { }

        public static Task WithTimeout(this Task task, TimeSpan timeout)
        {
            if (task.IsCompleted || (timeout <= TimeSpan.Zero))
            {
                return task;
            }
            // tcs.Task will be returned as a proxy to the caller
            var tcs = new TaskCompletionSource<VoidTypeStruct>();

            // Set up a timer to complete after the specified timeout period
            Timer timer = new Timer(state =>
            {
                var myTcs = (TaskCompletionSource<VoidTypeStruct>)state;
                myTcs.TrySetException(new TimeoutException());
                if (task is CancelableTask)
                {
                    ((CancelableTask)task).Cancel();
                }
            }, tcs, (int)timeout.TotalMilliseconds, Timeout.Infinite);

            // Wire up the logic for what happens when source task completes
            var tuple = Tuple.Create(timer, tcs);
            task.ContinueWith((antecedent) =>
            {
                tuple.Item1.Dispose();
                MarshalTaskResults(antecedent, tuple.Item2);
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnCanceled,
            TaskScheduler.Default);

            return tcs.Task;
        }

        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            if (task.IsCompleted || (timeout <= TimeSpan.Zero))
            {
                return task;
            }
            // tcs.Task will be returned as a proxy to the caller
            var tcs = new TaskCompletionSource<TResult>();

            // Set up a timer to complete after the specified timeout period
            Timer timer = new Timer(state =>
            {
                var myTcs = (TaskCompletionSource<TResult>)state;
                myTcs.TrySetException(new TimeoutException());
                if (task is CancelableTask<TResult>)
                {
                    ((CancelableTask<TResult>)task).Cancel();
                }
            }, tcs, (int)timeout.TotalMilliseconds, Timeout.Infinite);

            // Wire up the logic for what happens when source task completes
            var tuple = Tuple.Create(timer, tcs);
            task.ContinueWith((antecedent) =>
            {
                //var tuple = (Tuple<Timer, TaskCompletionSource<TResult>>)state;
                tuple.Item1.Dispose();
                MarshalTaskResults(antecedent, tuple.Item2);
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnCanceled,
            TaskScheduler.Default);

            return tcs.Task;
        }

        internal static void MarshalTaskResults<TResult>(Task source, TaskCompletionSource<TResult> proxy)
        {
            switch (source.Status)
            {
                case TaskStatus.Faulted:
                    proxy.TrySetException(source.Exception);
                    break;
                case TaskStatus.Canceled:
                    proxy.TrySetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    Task<TResult> castedSource = source as Task<TResult>;
                    proxy.TrySetResult(
                        castedSource == null ? default(TResult) : // source is a Task
                            castedSource.Result); // source is a Task<TResult>
                    break;
            }
        }
        public static CancelableTask StartCancelable(this TaskFactory factory, Action<CancellationToken> action)
        {
            var task = new CancelableTask(action, new CancellationTokenSource(), factory.CreationOptions);
            task.Start();
            return task;
        }

        public static CancelableTask<TResult> StartCancelable<TResult>(this TaskFactory factory, Func<CancellationToken, TResult> action)
        {
            var task = new CancelableTask<TResult>(action, new CancellationTokenSource(), factory.CreationOptions);
            task.Start();
            return task;
        }
    }

    public class CancelableTask : Task
    {
        private readonly CancellationTokenSource _source;

        public CancellationToken CancelationToken { get { return _source.Token; } }

        public CancelableTask(Action<CancellationToken> action, CancellationTokenSource source, TaskCreationOptions options)
            : base(Downcast(action), source.Token, source.Token, options)
        {
            _source = source;
        }

        private static Action<object> Downcast(Action<CancellationToken> a)
        {
            return (o) => a.Invoke((CancellationToken)o);
        }

        public void Cancel()
        {
            _source.Cancel();
        }
    }

    public class CancelableTask<TResult> : Task<TResult>
    {
        private readonly CancellationTokenSource _source;

        public CancellationToken CancelationToken { get { return _source.Token; } }

        public CancelableTask(Func<CancellationToken, TResult> action, CancellationTokenSource source, TaskCreationOptions options)
            : base(Downcast(action), source.Token, source.Token, options)
        {
            _source = source;
        }

        private static Func<object, TResult> Downcast(Func<CancellationToken, TResult> a)
        {
            return (o) => a.Invoke((CancellationToken)o);
        }

        public void Cancel()
        {
            _source.Cancel();
        }
    }
}
