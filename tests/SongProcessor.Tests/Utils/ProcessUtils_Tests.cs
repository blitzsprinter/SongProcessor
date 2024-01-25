using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SongProcessor.Utils;

namespace SongProcessor.Tests.Utils;

[TestClass]
public sealed class ProcessUtils_Tests
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public void FindProgram_Test()
		// We should always have a dotnet program installed if this test is being run
		=> _ = ProcessUtils.FindProgram("dotnet");

	[TestMethod]
	public void FindProgramBin_Test()
	{
		const string PROGRAM = "joemama";
		var dir = Path.Combine(Directory.GetCurrentDirectory(), "bin");
		Directory.CreateDirectory(dir);
		var path = Path.Combine(dir, ProcessUtils.GetProgramName(PROGRAM));
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
		Action failure = () => _ = ProcessUtils.FindProgram("asdfasdfasdfasdfasdfasdf");
		failure.Should().Throw<InvalidOperationException>();
	}

	[TestMethod]
	public async Task OnComplete_Test()
	{
		var process = ProcessUtils.FindProgram("dotnet").CreateProcess("--version");
		var invokeCount = 0;
		process.OnComplete(_ => ++invokeCount);

		await process.RunAsync(OutputMode.Sync).ConfigureAwait(false);
		invokeCount.Should().Be(1);
	}

	[TestMethod]
	public void OnXNoEventsAllowed_Test()
	{
		var process = ProcessUtils.FindProgram("dotnet").CreateProcess("--version");
		process.EnableRaisingEvents = false;
		Action onCompleted = () => _ = process.OnComplete(_ => { });
		onCompleted.Should().Throw<ArgumentException>();
	}

	[TestMethod]
	public void OnXNullCallback_Test()
	{
		var process = ProcessUtils.FindProgram("dotnet").CreateProcess("--version");
		Action onCompleted = () => _ = process.OnComplete(null!);
		onCompleted.Should().Throw<ArgumentNullException>();
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
		code.Should().Be(0); // Zero = success
		output.Should().ContainSingle();

		TestContext.Write(output[0]);
	}
}