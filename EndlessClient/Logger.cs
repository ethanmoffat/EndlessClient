// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using EOLib;

namespace EndlessClient
{
	public class ConfigStringLoadException : Exception
	{
		public string WhichString { get; private set; }
		public ConfigStringLoadException(string whichConfigString)
		{
			WhichString = whichConfigString;
		}
	}

	public class Logger : IDisposable
	{
		private const int FLUSH_TIME_MS = 30000;
		private const int FLUSH_LINES_WRITTEN = 50;
		private const int SPLIT_FILE_BYTE_LENGTH = 100000;

		private static Logger m_instance;
		private static Logger Instance
		{
			get
			{
				if(m_instance != null && m_instance.m_isOpen)
					return m_instance;
				m_instance = new Logger();
				if (!m_instance.m_isOpen)
					m_instance = null;
				return m_instance;
			}
		}

		private static readonly object locker = new object();

		private StreamWriter m_fileStream;
		private Timer m_flushTimer;
		private bool m_isOpen, m_isEnabled;
		private int m_writesSinceFlush;

		private Logger()
		{
			lock (locker)
			{
				try
				{
					string[] dirs = Constants.LogFilePath.Split(new[] {'/', '\\'});
					string workingDir = "";
					for (int i = 0; i < dirs.Length - 1; ++i)
					{
						workingDir += dirs[i];
						if (!Directory.Exists(workingDir))
							Directory.CreateDirectory(workingDir);
					}

					m_fileStream = new StreamWriter(Constants.LogFilePath, true);

					m_flushTimer = new Timer(o =>
					{
						lock (locker)
						{
							if (m_writesSinceFlush > 0)
							{
								Interlocked.Exchange(ref m_writesSinceFlush, 0);
								m_fileStream.Close();

								//check if the file should be split (based on length)
								bool append = true;
								FileInfo fi = new FileInfo(Constants.LogFilePath);
								if (fi.Length > SPLIT_FILE_BYTE_LENGTH)
								{
									append = false;
									File.Copy(Constants.LogFilePath, string.Format(Constants.LogFileFmt, DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss")));
								}

								m_fileStream = new StreamWriter(Constants.LogFilePath, append);
							}
						}
					}, null, FLUSH_TIME_MS, FLUSH_TIME_MS);
					
						m_isOpen = true;
					m_isEnabled = true;
				}
				catch
				{
					m_isOpen = false;
				}

				m_isEnabled = OldWorld.Instance.EnableLog;

				if (m_isEnabled && m_isOpen)
				{
					m_fileStream.WriteLine("------------------------------------------------------------------");
					m_fileStream.WriteLine("Log opened at {0}", _dateTimeString());
					m_fileStream.WriteLine("Process ID - Native Thread ID - Managed Thread ID - DateTime - Info");
					m_fileStream.WriteLine("------------------------------------------------------------------");
				}
			}
		}

		public void Dispose()
		{
			lock (locker)
			{
				if (m_isOpen && m_fileStream != null)
				{
					try
					{
						m_fileStream.WriteLine("------------------------------------------------------------------");
						m_fileStream.WriteLine("Log Closed");
						m_fileStream.WriteLine("------------------------------------------------------------------");
					}
					catch
					{
						m_isOpen = false;
						m_isEnabled = false;
					}
					m_flushTimer.Dispose();
					m_flushTimer = null;
					m_fileStream.Close();
					m_fileStream = null;
				}
			}
		}

		private void _write(string logStr)
		{
			lock (locker)
			{
				m_fileStream.WriteLine(logStr);
				Interlocked.Increment(ref m_writesSinceFlush);
				if (m_writesSinceFlush > FLUSH_LINES_WRITTEN)
				{
					m_fileStream.Close();
					m_fileStream = new StreamWriter(Constants.LogFilePath, true);
					Interlocked.Exchange(ref m_writesSinceFlush, 0);
				}
			}
		}

		private string _dateTimeString()
		{
			return DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss:fff");
		}

		//public interface is here: usage is Logger.Log(), without access to the instance
		public static void Log(string format, params object[] parameters)
		{
			Logger localInst = Instance;
			if (localInst != null && localInst.m_isOpen && localInst.m_isEnabled)
			{
				int threadID = Thread.CurrentThread.ManagedThreadId;
				uint nativeID = GetCurrentThreadID();
				int processID = System.Diagnostics.Process.GetCurrentProcess().Id;
				string front = string.Format("{0,-5} {3,-5} {1,5} {2,-25} ", processID, threadID, localInst._dateTimeString(), nativeID);
				front += format;
				localInst._write(string.Format(front, parameters));
			}
		}

		public static void Close()
		{
			Logger localInst = Instance;
			if (localInst != null && localInst.m_isOpen && localInst.m_isEnabled)
			{
				localInst.Dispose();
			}
		}

		[DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId")]
		private static extern uint GetCurrentThreadID();
	}
}
