using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentFTP.Tests.Integration
{
	public class DockerFtpServerFixture : IDisposable
	{
		internal TestcontainersContainer container;
		internal string user { get; } = "bob";
		internal string password { get; } = "12345";

		public DockerFtpServerFixture()
		{
			var testContainerKey = Environment.GetEnvironmentVariable("FluentFTP__Tests__Integration__FtpServerKey");
			var testcontainersBuilder = GetContainer(testContainerKey);

			if(testcontainersBuilder is not null)
			{
				container = testcontainersBuilder.Build();
				container.StartAsync().Wait();
			}
		}

		public void Dispose()
		{
			container?.DisposeAsync();
		}

		private ITestcontainersBuilder<TestcontainersContainer>? GetContainer(string? key)
		{
			var container = key switch
			{
				"pure-ftpd" => GetPureFtpdContainerBuilder(),
				"vsftpd" => GetVsftpdContainerBuilder(),
				_ => null
			};

			return container;
		}

		private ITestcontainersBuilder<TestcontainersContainer> GetPureFtpdContainerBuilder()
		{
			var builder = new TestcontainersBuilder<TestcontainersContainer>()
				.WithImage("stilliard/pure-ftpd")
				.WithName("pure-ftpd")
				.WithPortBinding(21);

			for (var port = 30000; port <= 30009; port++)
			{
				builder = builder.WithPortBinding(port);
			}

			builder = builder.WithEnvironment("FTP_USER_NAME", user)
				.WithEnvironment("FTP_USER_PASS", password)
				.WithEnvironment("FTP_USER_HOME", "/home/bob")
				.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(21));

			return builder;
		}

		private ITestcontainersBuilder<TestcontainersContainer> GetVsftpdContainerBuilder()
		{
			var builder = new TestcontainersBuilder<TestcontainersContainer>()
				.WithImage("fauria/vsftpd")
				.WithName("vsftpd")
				.WithPortBinding(20)
				.WithPortBinding(21);

			for (var port = 21100; port <= 21110; port++)
			{
				builder = builder.WithExposedPort(port);
				builder = builder.WithPortBinding(port);
			}

			builder = builder
				.WithEnvironment("PASV_ADDRESS", "127.0.0.1")
				.WithEnvironment("FTP_USER", user)
				.WithEnvironment("FTP_PASS", password)
				.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(21));
			
			return builder;
		}
	}
}
