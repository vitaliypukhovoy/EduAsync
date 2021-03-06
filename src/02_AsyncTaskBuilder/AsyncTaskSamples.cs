﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace _01_AsyncTaskBuilder
{
    [TestFixture]
    public class AsyncTaskSamples
    {
        [Test]
        public void RunAsyncTask()
        {
            AsyncTaskMethodBuilder.Inspector.Clear();
            AsyncTask();
            Thread.Sleep(42);
            AsyncTaskMethodBuilder.Inspector.Print();

            Assert.AreEqual(
                new string[]
                {
                    ".ctor", // AsyncTaskMethodBuilder.ctor
                    "Start", // AsyncTaskMethodBuilder.Start
                    // Here the method builder calls the MoveNext on the state machine,
                    // and the state machine runs the code from the method body.
                    "AsyncTask_Start",
                    // The state machine notifies the builder that we're done,
                    // because method is finished synchronously
                    "SetResult",

                    // The generated method access 'Task' property to await for
                    "Task",
                },
                AsyncTaskMethodBuilder.Inspector.InvokedMembers);
        }

        [Test]
        public void RunAsyncTaskWithAwait()
        {
            AsyncTaskMethodBuilder.Inspector.Clear();
            AsyncTaskWithAwait();
            Thread.Sleep(42);
            AsyncTaskMethodBuilder.Inspector.Print();

            Assert.AreEqual(
                new string[]
                {
                    ".ctor", // AsyncTaskMethodBuilder.ctor
                    "Start", // AsyncTaskMethodBuilder.Start
                    // Here the method builder calls the MoveNext on the state machine
                    "AsyncTask_Start",
                    // Task.Yield is finished, and the state machine calls Builder.AwaitUnsafeOnCompleted
                    "AwaitUnsafeOnCompleted",

                    // The builder moves the state machine
                    "AsyncTask_AfterAwait",

                    // The state machine notifies the builder that we're done
                    "SetResult",

                    // The generated method access 'Task' property to await for
                    "Task",
                },
                AsyncTaskMethodBuilder.Inspector.InvokedMembers);
        }

        [Test]
        public void RunAsyncTaskThatFails()
        {
            AsyncTaskMethodBuilder.Inspector.Clear();
            AsyncTaskThatThrows();
            Thread.Sleep(42);
            AsyncTaskMethodBuilder.Inspector.Print();

            Assert.AreEqual(
                new string[]
                {
                    ".ctor", // AsyncTaskMethodBuilder.ctor
                    "Start", // AsyncTaskMethodBuilder.Start
                    // Here the method builder calls the MoveNext on the state machine
                    "AsyncTask_Start",
                    
                    // The state machine calls SetException on the builder instance
                    "SetException",

                    // The generated method access 'Task' property to await for
                    "Task",
                },
                AsyncTaskMethodBuilder.Inspector.InvokedMembers);
        }

        [Test]
        public void RunAsyncTaskWithCancellation()
        {
            AsyncTaskMethodBuilder.Inspector.Clear();
            AsyncTaskWithCancellation();
            Thread.Sleep(42);
            AsyncTaskMethodBuilder.Inspector.Print();

            Assert.AreEqual(
                new string[]
                {
                    ".ctor", // AsyncTaskMethodBuilder.ctor
                    "Start", // AsyncTaskMethodBuilder.Start
                    // Here the method builder calls the MoveNext on the state machine
                    "AsyncTask_Start",
                    
                    // The state machine calls SetException on the builder instance with TaskCanceledException
                    "SetException",

                    // The generated method access 'Task' property to await for
                    "Task",
                },
                AsyncTaskMethodBuilder.Inspector.InvokedMembers);

            Assert.IsInstanceOf<TaskCanceledException>(AsyncTaskMethodBuilder.LastInstance.Exception);
        }

        public async Task AsyncTask()
        {
            AsyncTaskMethodBuilder.Inspector.Record("AsyncTask_Start");
        }

        public async Task AsyncTaskWithAwait()
        {
            AsyncTaskMethodBuilder.Inspector.Record("AsyncTask_Start");
            await Task.Yield();
            AsyncTaskMethodBuilder.Inspector.Record("AsyncTask_AfterAwait");
        }

        public async Task AsyncTaskThatThrows()
        {
            AsyncTaskMethodBuilder.Inspector.Record("AsyncTask_Start");
            throw new InvalidOperationException();
        }

        public async Task AsyncTaskWithCancellation()
        {
            AsyncTaskMethodBuilder.Inspector.Record("AsyncTask_Start");

            var cts = new CancellationTokenSource();
            cts.Cancel();

            await Task.Delay(42, cts.Token);
        }
    }
}
