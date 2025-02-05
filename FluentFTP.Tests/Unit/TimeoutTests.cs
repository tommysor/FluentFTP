﻿using FluentFTP.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace FluentFTP.Tests.Unit {
	public class TimeoutTests {

		private const int timeoutMillis = 2500;

		private void ValidateTime(DateTime callStart, string methodName) {
			var maxElapsedMillis = (timeoutMillis + 500);
			if ((DateTime.Now - callStart).TotalMilliseconds > maxElapsedMillis) {
				Assert.True(false, $"ConnectTimeout is being ignored with {methodName}() method!");
			}
			else {
				Assert.True(true);
			}
		}

		[Fact]
		public void ConnectTimeout() {

			FtpClient client = new FtpClient("test.github.com", 21, "wrong", "password");
			client.DataConnectionType = FtpDataConnectionType.PASVEX;
			client.ConnectTimeout = timeoutMillis;
			var start = DateTime.Now;
			try {
				client.Connect();
				Assert.True(false, "Connect succeeded. Was supposed to time out.");
			}
			catch (TimeoutException) {
				ValidateTime(start, "Connect");
			}
		}

		[Fact]
		public async Task ConnectTimeoutAsync() {

			FtpClient client = new FtpClient("test.github.com", 21, "wrong", "password");
			client.DataConnectionType = FtpDataConnectionType.PASVEX;
			client.ConnectTimeout = timeoutMillis;
			var start = DateTime.Now;
			try {
				await client.ConnectAsync();
				Assert.True(false, "Connect succeeded. Was supposed to time out.");
			}
			catch (TimeoutException) {
				ValidateTime(start, "ConnectAsync");
			}
			catch (SocketException sockEx) {
				if (sockEx.Message?.Contains("Operation canceled", StringComparison.OrdinalIgnoreCase) == true) {
					ValidateTime(start, "ConnectAsync");
				}
				throw;
			}
		}

	}
}