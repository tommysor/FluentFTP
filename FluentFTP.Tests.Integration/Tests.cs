using FluentFTP.Tests.Integration.Skippable;
using System.Text;

namespace FluentFTP.Tests.Integration
{
	/// <summary>
	/// All tests must use [SkippableFact] or [SkippableTheory] (as opposed to [Fact] and [Theory]) to allow
	/// "all tests" to run successfully on a developer machine without Docker (skipping all integration tests).
	/// 
	/// Making tests skippable is mostly copied from https://github.com/xunit/samples.xunit/tree/main/DynamicSkipExample
	/// </summary>
	public class Tests : IClassFixture<DockerFtpServerFixture>
    {
		private readonly DockerFtpServerFixture _fixture;
		private readonly string _host = "localhost";
		private readonly string _user;
		private readonly string _password;

		public Tests(DockerFtpServerFixture fixture)
		{
			_fixture = fixture;
			_user = _fixture.user;
			_password = _fixture.password;
		}

		private FtpClient GetConnectedClient()
		{
			var client = new FtpClient(_host, _user, _password);
			client.Connect();
			return client;
		}

		#region Connect
		[SkippableFact]
        public async Task ConnectAsync()
        {
			using var ftpClient = new FtpClient(_host, _user, _password);
			await ftpClient.ConnectAsync();
			// Connect without error => pass
			Assert.True(true);
        }
		
		[SkippableFact]
		public void Connect()
		{
			using var ftpClient = new FtpClient(_host, _user, _password);
			ftpClient.Connect();
			// Connect without error => pass
			Assert.True(true);
		}

		[SkippableFact]
		public void AutoConnect()
		{
			using var ftpClient = new FtpClient(_host, _user, _password);
			var profile = ftpClient.AutoConnect();
			Assert.NotNull(profile);
		}

		[SkippableFact]
		public async Task AutoConnectAsync()
		{
			using var ftpClient = new FtpClient(_host, _user, _password);
			var profile = await ftpClient.AutoConnectAsync();
			Assert.NotNull(profile);
		}

		[SkippableFact]
		public void AutoDetect()
		{
			using var ftpClient = new FtpClient(_host, _user, _password);
			var profiles = ftpClient.AutoDetect();
			Assert.NotEmpty(profiles);
		}

		[SkippableFact]
		public async Task AutoDetectAsync()
		{
			using var ftpClient = new FtpClient(_host, _user, _password);
			var profiles = await ftpClient.AutoDetectAsync(false);
			Assert.NotEmpty(profiles);
		}
		#endregion

		#region UploadDownload
		[SkippableFact]
		public async Task UploadDownloadBytesAsync()
		{
			const string content = "Hello World!";
			var bytes = Encoding.UTF8.GetBytes(content);

			using var ftpClient = GetConnectedClient();

			const string path = "/UploadDownloadBytesAsync/helloworld.txt";
			var uploadStatus = await ftpClient.UploadAsync(bytes, path, createRemoteDir: true);
			Assert.Equal(FtpStatus.Success, uploadStatus);

			var outBytes = await ftpClient.DownloadAsync(path, CancellationToken.None);
			Assert.NotNull(outBytes);

			var outContent = Encoding.UTF8.GetString(outBytes);
			Assert.Equal(content, outContent);
		}

		[SkippableFact]
		public void UploadDownloadBytes()
		{
			const string content = "Hello World!";
			var bytes = Encoding.UTF8.GetBytes(content);

			using var ftpClient = GetConnectedClient();

			const string path = "/UploadDownloadBytes/helloworld.txt";
			var uploadStatus = ftpClient.Upload(bytes, path, createRemoteDir: true);
			Assert.Equal(FtpStatus.Success, uploadStatus);

			var success = ftpClient.Download(out var outBytes, path);
			Assert.True(success);
			
			var outContent = Encoding.UTF8.GetString(outBytes);
			Assert.Equal(content, outContent);
		}

		[SkippableFact]
		public async Task UploadDownloadStreamAsync()
		{
			const string content = "Hello World!";
			using var stream = new MemoryStream();
			using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
			await streamWriter.WriteAsync(content);
			await streamWriter.FlushAsync();
			stream.Position = 0;

			const string path = "/UploadDownloadStreamAsync/helloworld.txt";
			using var ftpClient = GetConnectedClient();
			var uploadStatus = await ftpClient.UploadAsync(stream, path, createRemoteDir: true);
			Assert.Equal(FtpStatus.Success, uploadStatus);

			using var outStream = new MemoryStream();
			var success = await ftpClient.DownloadAsync(outStream, path);
			Assert.True(success);

			outStream.Position = 0;
			using var streamReader = new StreamReader(outStream, Encoding.UTF8);
			var outContent = await streamReader.ReadToEndAsync();
			Assert.Equal(content, outContent);
		}

		[SkippableFact]
		public void UploadDownloadStream()
		{
			const string content = "Hello World!";
			using var stream = new MemoryStream();
			using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
			streamWriter.Write(content);
			streamWriter.Flush();
			stream.Position = 0;

			const string path = "/UploadDownloadStreamAsync/helloworld.txt";
			using var ftpClient = GetConnectedClient();
			var uploadStatus = ftpClient.Upload(stream, path, createRemoteDir: true);
			Assert.Equal(FtpStatus.Success, uploadStatus); 

			using var outStream = new MemoryStream();
			var success = ftpClient.Download(outStream, path);
			Assert.True(success);

			outStream.Position = 0;
			using var streamReader = new StreamReader(outStream, Encoding.UTF8);
			var outContent = streamReader.ReadToEnd();
			Assert.Equal(content, outContent);
		}

		#endregion

		#region GetListing
		[SkippableFact]
		public async Task GetListingAsync()
		{
			using var client = GetConnectedClient();
			var bytes = Encoding.UTF8.GetBytes("a");
			const string directory = "/GetListingAsync/";
			const string fileNameInRoot = "GetListingAsync.txt";
			const string fileNameInDirectory = "GetListingAsyncInDirectory.txt";
			await client.UploadAsync(bytes, fileNameInRoot);
			await client.UploadAsync(bytes, directory + fileNameInDirectory, createRemoteDir: true);

			var listRoot = await client.GetListingAsync();
			Assert.Contains(listRoot, f => f.Name == fileNameInRoot);

			var listDirectory = await client.GetListingAsync(directory);
			Assert.Contains(listDirectory, f => f.Name == fileNameInDirectory);
			
			await client.SetWorkingDirectoryAsync(directory);
			var listCurrentDirectory = await client.GetListingAsync();
			Assert.Contains(listCurrentDirectory, f => f.Name == fileNameInDirectory);
		}

		[SkippableFact]
		public void GetListing()
		{
			using var client = GetConnectedClient();
			var bytes = Encoding.UTF8.GetBytes("a");
			const string directory = "/GetListing/";
			const string fileNameInRoot = "GetListing.txt";
			const string fileNameInDirectory = "GetListingInDirectory.txt";
			client.Upload(bytes, fileNameInRoot);
			client.Upload(bytes, directory + fileNameInDirectory, createRemoteDir: true);

			var listRoot = client.GetListing();
			Assert.Contains(listRoot, f => f.Name == fileNameInRoot);

			var listDirectory = client.GetListing(directory);
			Assert.Contains(listDirectory, f => f.Name == fileNameInDirectory);

			client.SetWorkingDirectory(directory);
			var listCurrentDirectory = client.GetListing();
			Assert.Contains(listCurrentDirectory, f => f.Name == fileNameInDirectory);
		}
		#endregion
	}
}