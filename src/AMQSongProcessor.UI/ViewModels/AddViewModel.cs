using System.Runtime.Serialization;
using System.Windows.Input;

using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

using Splat;

namespace AMQSongProcessor.UI.ViewModels
{
	[DataContract]
	public class AddViewModel : ReactiveObject, IRoutableViewModel, IValidatableViewModel
	{
		private readonly IScreen _HostScreen;
		private string _Directory;
		private int _Id;

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

		public string UrlPathSegment => "/add";
		public ValidationContext ValidationContext { get; } = new ValidationContext();

		public AddViewModel(IScreen screen = null)
		{
			_HostScreen = screen;

			this.ValidationRule(
				x => x.Directory,
				System.IO.Directory.Exists,
				"Directory must exist.");
			this.ValidationRule(
				x => x.Id,
				x => x > 0,
				"Id must be greater than 0.");

			Add = ReactiveCommand.CreateFromTask(async () =>
			{
				var result = Id;
			}, this.IsValid());
		}
	}
}