using AMQSongProcessor.Utils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMQSongProcessor.Tests.Utils
{
	[TestClass]
	public sealed class ProcessUtils_Tests
	{
		public TestContext TestContext { get; set; } = null!;

		[TestMethod]
		public void FindProgram_Test()
		{
			// We should always have a dotnet program installed if this test is being run
			_ = ProcessUtils.FindProgram("dotnet");
		}

		[TestMethod]
		public void FindProgramBin_Test()
		{
			const string PROGRAM = "joemama";
			var dir = Path.Combine(Environment.CurrentDirectory, "bin");
			Directory.CreateDirectory(dir);
			var path = Path.Combine(dir, $"{PROGRAM}.exe");
			File.Create(path).Dispose();

			_ = ProcessUtils.FindProgram(PROGRAM);

			// Some cleanup, not important if it fails
			File.Delete(path);
			try
			{
				Directory.Delete(dir);
			}
			catch { }
		}

		[TestMethod]
		public void FindProgramFailure_Test()
		{
			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				_ = ProcessUtils.FindProgram("asdfasdfasdfasdfasdfasdfasdfasdf");
			});
		}

		[TestMethod]
		public async Task OnCancel_Test()
		{
			var process = ProcessUtils.FindProgram("dotnet").CreateProcess("--version");
			var source = new CancellationTokenSource();
			var invokeCount = 0;
			process.OnCancel((_, _) => ++invokeCount, source.Token);

			source.Cancel();
			source.Cancel();
			Assert.AreEqual(1, invokeCount);

			await process.RunAsync(OutputMode.Sync).ConfigureAwait(false);
		}

		[TestMethod]
		public async Task OnCancelAfterExited_Test()
		{
			var process = ProcessUtils.FindProgram("dotnet").CreateProcess("--version");
			var source = new CancellationTokenSource();
			var invokeCount = 0;
			process.OnCancel((_, _) => ++invokeCount, source.Token);

			await process.RunAsync(OutputMode.Sync).ConfigureAwait(false);
			source.Cancel();
			Assert.AreEqual(0, invokeCount);
		}

		[TestMethod]
		public async Task OnComplete_Test()
		{
			var process = ProcessUtils.FindProgram("dotnet").CreateProcess("--version");
			var invokeCount = 0;
			process.OnComplete(_ => ++invokeCount);

			await process.RunAsync(OutputMode.Sync).ConfigureAwait(false);
			Assert.AreEqual(1, invokeCount);
		}

		[TestMethod]
		public void OnXNoEventsAllowed_Test()
		{
			var process = ProcessUtils.FindProgram("dotnet").CreateProcess("--version");
			process.EnableRaisingEvents = false;
			Assert.ThrowsException<ArgumentException>(() =>
			{
				_ = process.OnCancel((_, _) => { });
			});
			Assert.ThrowsException<ArgumentException>(() =>
			{
				_ = process.OnComplete(_ => { });
			});
		}

		[TestMethod]
		public void OnXNullCallback_Test()
		{
			var process = ProcessUtils.FindProgram("dotnet").CreateProcess("--version");
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				_ = process.OnCancel(null!);
			});
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				_ = process.OnComplete(null!);
			});
		}

		[TestMethod]
		public async Task RunAsync_Test()
		{
			var process = ProcessUtils.FindProgram("dotnet").CreateProcess("--version");

			var output = new List<string>();
			process.OutputDataReceived += (s, e) =>
			{
				if (!string.IsNullOrWhiteSpace(e.Data))
				{
					output.Add(e.Data);
				}
			};

			var code = await process.RunAsync(OutputMode.Async).ConfigureAwait(false);
			Assert.AreEqual(0, code); // Zero = success
			Assert.AreEqual(1, output.Count);

			TestContext.Write(output[0]);
		}
	}
}