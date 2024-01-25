using ReactiveUI;

using SongProcessor.UI.Views;

using System.Reactive;
using System.Reactive.Linq;

namespace SongProcessor.UI.ViewModels;

public sealed class MessageBoxViewModel<T> : ReactiveObject
{
	private string? _ButtonText = "Ok";
	private bool _CanResize;
	private T? _CurrentOption;
	private bool _HasOptions;
	private int _Height = UIUtils.MESSAGE_BOX_HEIGHT;
	private IEnumerable<T>? _Options;
	private string? _Text;
	private string? _Title;
	private int _Width = UIUtils.MESSAGE_BOX_WIDTH;

	public string? ButtonText
	{
		get => _ButtonText;
		set => this.RaiseAndSetIfChanged(ref _ButtonText, value);
	}
	public bool CanResize
	{
		get => _CanResize;
		set => this.RaiseAndSetIfChanged(ref _CanResize, value);
	}
	public T? CurrentOption
	{
		get => _CurrentOption;
		set => this.RaiseAndSetIfChanged(ref _CurrentOption, value);
	}
	public bool HasOptions
	{
		get => _HasOptions;
		private set => this.RaiseAndSetIfChanged(ref _HasOptions, value);
	}
	public int Height
	{
		get => _Height;
		set => this.RaiseAndSetIfChanged(ref _Height, value);
	}
	public IEnumerable<T>? Options
	{
		get => _Options;
		set
		{
			this.RaiseAndSetIfChanged(ref _Options, value);
			CurrentOption = default!;
			HasOptions = value?.Any() ?? false;
			ButtonText = HasOptions ? "Confirm" : "Ok";
		}
	}
	public string? Text
	{
		get => _Text;
		set => this.RaiseAndSetIfChanged(ref _Text, value);
	}
	public string? Title
	{
		get => _Title;
		set => this.RaiseAndSetIfChanged(ref _Title, value);
	}
	public int Width
	{
		get => _Width;
		set => this.RaiseAndSetIfChanged(ref _Width, value);
	}

	#region Commands
	public ReactiveCommand<MessageBox, Unit> Escape { get; }
	public ReactiveCommand<MessageBox, Unit> Ok { get; }
	#endregion Commands

	public MessageBoxViewModel()
	{
		Escape = ReactiveCommand.Create<MessageBox>(x => x.Close());

		var canClose = this.WhenAnyValue(
			x => x.CurrentOption!,
			x => x.Options,
			(current, all) => new
			{
				Current = current,
				All = all,
			})
			.Select(x => x.All is null || !Equals(x.Current, default));
		Ok = ReactiveCommand.Create<MessageBox>(
			x => x.Close(CurrentOption),
			canClose
		);
	}
}