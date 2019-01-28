using System;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace GameCore {
	public interface ILogger {
		void WriteLine(string str);
	}

	public static class Logger {
		private class CSharpLogger : ILogger {
			public void WriteLine(string str) {
				Console.WriteLine(str);
			}
		}
#if UNITY_EDITOR
		private class UnityLogger : ILogger {
			public void WriteLine(string str) {
				Debug.Log(str);
			}
		}
#endif
		private static ILogger _logger;

		static Logger() {
			_logger = new CSharpLogger();
#if UNITY_EDITOR
			_logger = new UnityLogger();
#endif
		}
		
		public static void WriteLine(string str) {
			_logger.WriteLine(str);
		}
	}
}
