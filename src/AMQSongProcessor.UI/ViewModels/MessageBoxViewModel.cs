using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

using Avalonia.Controls;

using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	public class MessageBoxViewModel : ReactiveObject
	{
		private const string CONFIRM = "Confirm";
		private const string OK = "OK";

		private string? _ButtonText;
		private object? _CurrentOption;
		private IEnumerable<object>? _Options;
		private string? _Text;
		private string? _Title;

		public string? ButtonText
		{
			get => _ButtonText;
			set => this.RaiseAndSetIfChanged(ref _ButtonText, value);
		}

		public ReactiveCommand<Window, Unit> CloseCommand { get; }

		public object? CurrentOption
		{
			get => _CurrentOption;
			set => this.RaiseAndSetIfChanged(ref _CurrentOption, value);
		}

		public IEnumerable<object>? Options
		{
			get => _Options;
			set
			{
				this.RaiseAndSetIfChanged(ref _Options, value);
				this.RaiseAndSetIfChanged(ref _CurrentOption, null);
				ButtonText = Options == null ? OK : CONFIRM;
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

		public MessageBoxViewModel()
		{
			var canClose = this.WhenAnyValue(
				x => x.CurrentOption,
				x => x.Options,
				(current, all) => new
				{
					Current = current,
					All = all,
				}).Select(x => x.All == null || x.Current != default);
			CloseCommand = ReactiveCommand.Create<Window>(window =>
			{
				window.Close(CurrentOption);
			}, canClose);
		}
	}
}