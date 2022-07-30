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

		public DockerFtpServerFixture()
		{
			var testcontainersBuilder = GetVsftpdContainerBuilder();

			container = testcontainersBuilder.Build();
			container.StartAsync().Wait();
		}

		public void Dispose()
		{
			container?.DisposeAsync();
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

			builder = builder.WithEnvironment("FTP_USER_NAME", "bob")
				.WithEnvironment("FTP_USER_PASS", "12345")
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
				.WithEnvironment("FTP_USER", "bob")
				.WithEnvironment("FTP_PASS", "12345")
				.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(21));
			
			return builder;
		}
	}
}
