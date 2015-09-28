using System;
using System.Threading;

namespace EOBot
{
	internal abstract class BotBase : IBot
	{
		private Thread _workerThread;
		private AutoResetEvent _terminationEvent;
		private CancellationTokenSource _cancelTokenSource;
		private bool _initialized;

		//unneeded for now
		///// <summary>
		///// Get whether or not the worker thread was requested to terminate via a call to Terminate()
		///// </summary>
		//public bool TerminationRequested { get { return _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested; } }

		/// <summary>
		/// Invoked once work has completed executing.
		/// </summary>
		public event Action WorkCompleted;

		protected BotBase()
		{
			_terminationEvent = new AutoResetEvent(false);
			_cancelTokenSource = new CancellationTokenSource();
		}

		public virtual void Initialize()
		{
			_initialized = true;
		}

		/// <summary>
		/// Run the bot
		/// </summary>
		/// <param name="waitForTermination">True to keep running until Terminate() is called, false otherwise</param>
		public void Run(bool waitForTermination)
		{
			if (!_initialized)
				throw new InvalidOperationException("Must call Initialize() before calling Run()");

			var doWorkAndWait = new ThreadStart(DoWorkAndWaitForTermination);
			var doWork = new ThreadStart(DoWorkOnly);

			_workerThread = new Thread(waitForTermination ? doWorkAndWait : doWork);
			_workerThread.Start();
		}

		/// <summary>
		/// Abstract worker method. Override with custom work logic for the bot to execute
		/// </summary>
		/// <param name="ct">A cancellation token that will be signalled when Terminate() is called</param>
		protected abstract void DoWork(CancellationToken ct);

		private void DoWorkOnly()
		{
			DoWork(_cancelTokenSource.Token);
			FireWorkCompleted();
		}

		private void DoWorkAndWaitForTermination()
		{
			DoWork(_cancelTokenSource.Token);
			_terminationEvent.WaitOne();
			FireWorkCompleted();
		}

		/// <summary>
		/// Terminate the bot. Ends execution as soon as is convenient.
		/// </summary>
		public void Terminate()
		{
			_cancelTokenSource.Cancel();
			_terminationEvent.Set();
		}

		private void FireWorkCompleted()
		{
			if (WorkCompleted != null)
				WorkCompleted();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		~BotBase()
		{
			Dispose(false);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Terminate();
				if (_workerThread != null)
					_workerThread.Join();

				if (_terminationEvent != null)
				{
					_terminationEvent.Dispose();
					_terminationEvent = null;
				}

				if (_cancelTokenSource != null)
				{
					_cancelTokenSource.Dispose();
					_cancelTokenSource = null;
				}
			}
		}
	}
}
