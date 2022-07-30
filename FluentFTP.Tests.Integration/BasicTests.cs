namespace FluentFTP.Tests.Integration
{
    public class BasicTests : IClassFixture<DockerFtpServerFixture>
    {
		private readonly DockerFtpServerFixture _fixture;
		private readonly string _host = "localhost";
		private readonly string _user;
		private readonly string _password;

		public BasicTests(DockerFtpServerFixture fixture)
		{
			_fixture = fixture;
			_user = _fixture.user;
			_password = _fixture.password;
		}

		[Fact]
        public async Task ConnectAndList()
        {
			using var ftpClient = new FtpClient(_host, _user, _password);
			await ftpClient.ConnectAsync();
			var list = ftpClient.GetListing();
			Assert.Empty(list);
        }

		[Fact]
		public async Task Upload()
		{
			using var stream = new MemoryStream();
			using var writer = new StreamWriter(stream);
			writer.WriteLine("Hello World!");
			stream.Flush();
			stream.Position = 0;

			using var ftpClient = new FtpClient(_host, _user, _password);
			await ftpClient.ConnectAsync();

			await ftpClient.UploadAsync(stream, "HelloWorld.txt");

			var list = ftpClient.GetListing();
			Assert.Single(list);
			var fileName = list.Where(x => x.Name == "HelloWorld.txt");
			Assert.Single(fileName);
		}
	}
}