using System.Runtime.Serialization;
using System.Windows.Input;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

using Splat;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class AddViewModel : ReactiveValidationObject<AddViewModel>, IRoutableViewModel
	{
		private readonly IScreen _HostScreen;
		private string _Directory;
		private int _Id;
		private string _IdText;

		public ICommand Add { get; }

		[DataMember]
		public string Directory
		{
			get => _Directory;
			set => this.RaiseAndSetIfChanged(ref _Directory, value);
		}

		public IScreen HostScreen => _HostScreen ?? Locator.Current.GetService<IScreen>();

		[DataMember]
		public int Id
		{
			get => _Id;
			set => this.RaiseAndSetIfChanged(ref _Id, value);
		}

		public string IdText
		{
			get => _IdText;
			set => this.RaiseAndSetIfChanged(ref _IdText, value);
		}

		public string UrlPathSegment => "/add";

		public AddViewModel(IScreen screen = null)
		{
			_HostScreen = screen;

			this.ValidationRule(
				x => x.Directory,
				System.IO.Directory.Exists,
				"Nonexistent directory.");

			this.ValidationRule(
				x => x.Id,
				x => x > 0,
				"Id is less than 1.");

			this.ValidationRule(
				x => x.IdText,
				x => int.TryParse(x, out _),
				"Not a number.");

			Add = ReactiveCommand.CreateFromTask(async () =>
			{
				var result = Id;
			}, this.IsValid());
		}
	}
}