using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

using Avalonia.Controls;

using ReactiveUI;

namespace AMQSongProcessor.UI.ViewModels
{
	public class MessageBoxViewModel<T> : ReactiveObject
	{
		private string? _ButtonText = "Ok";
		private bool _CanResize;
		private T _CurrentOption = default!;
		private int _Height = Constants.MESSAGE_BOX_HEIGHT;
		private IEnumerable<T>? _Options;
		private string? _Text;
		private string? _Title;
		private int _Width = Constants.MESSAGE_BOX_WIDTH;

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
		[property: MaybeNull]
		public T CurrentOption
		{
			get => _CurrentOption;
			set => this.RaiseAndSetIfChanged(ref _CurrentOption, value);
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
				ButtonText = Options == null ? "Ok" : "Confirm";
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
		public ReactiveCommand<Window, Unit> CloseCommand { get; }
		#endregion Commands

		public MessageBoxViewModel()
		{
			var canClose = this.WhenAnyValue(
				x => x.CurrentOption!,
				x => x.Options,
				(current, all) => new
				{
					Current = current,
					All = all,
				}).Select(x => x.All == null || !Equals(x.Current, default));
			CloseCommand = ReactiveCommand.Create<Window>(window =>
			{
				window.Close(CurrentOption);
			}, canClose);
		}
	}
}