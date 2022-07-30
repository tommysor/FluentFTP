namespace FluentFTP.Tests.Integration
{
    public class BasicTests : IClassFixture<DockerFtpServerFixture>
    {
		private readonly DockerFtpServerFixture _fixture;

		public BasicTests(DockerFtpServerFixture fixture)
		{
			_fixture = fixture;
		}

		[Fact]
        public async Task ConnectAndList()
        {
			using var ftpClient = new FtpClient("localhost", "bob", "12345");
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

			using var ftpClient = new FtpClient("localhost", "bob", "12345");
			await ftpClient.ConnectAsync();

			await ftpClient.UploadAsync(stream, "HelloWorld.txt");

			var list = ftpClient.GetListing();
			Assert.Single(list);
			var fileName = list.Where(x => x.Name == "HelloWorld.txt");
			Assert.Single(fileName);
		}
	}
}