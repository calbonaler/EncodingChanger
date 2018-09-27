using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EncodingChanger
{
	public class MainWindowViewModel : IDisposable, INotifyPropertyChanged
	{
		public MainWindowViewModel()
		{
			m_Model = new Model();
			m_Model.PropertyChanged += OnModelPropertyChanged;
			Encodings = new ReadOnlyObservableCollection<EncodingViewModel>(new ObservableCollection<EncodingViewModel>(m_Model.Encodings.Select(x => new EncodingViewModel(x))));
			m_CodePageEncodingMap = new ReadOnlyDictionary<int, EncodingViewModel>(Encodings.ToDictionary(x => x.CodePage));
			SwapInputOutputEncodingCommand = new DelegateCommand(m_Model.SwapInputOutputEncodings);
			SetOutputTextToInputTextCommand = new DelegateCommand(m_Model.SetOutputTextToInputText);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;
			m_Model.PropertyChanged -= OnModelPropertyChanged;
		}

		readonly Model m_Model;
		readonly ReadOnlyDictionary<int, EncodingViewModel> m_CodePageEncodingMap;

		public string InputText
		{
			get => m_Model.InputText;
			set => m_Model.InputText = value;
		}
		public string OutputText => m_Model.OutputText;
		public EncodingViewModel InputEncoding
		{
			get => m_Model.InputEncoding == null ? null : m_CodePageEncodingMap[m_Model.InputEncoding.CodePage];
			set => m_Model.InputEncoding = value.GetEncoding();
		}
		public EncodingViewModel OutputEncoding
		{
			get => m_Model.OutputEncoding == null ? null : m_CodePageEncodingMap[m_Model.OutputEncoding.CodePage];
			set => m_Model.OutputEncoding = value.GetEncoding();
		}
		public ReadOnlyObservableCollection<EncodingViewModel> Encodings { get; }

		public DelegateCommand SwapInputOutputEncodingCommand { get; }
		public DelegateCommand SetOutputTextToInputTextCommand { get; }

		void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(m_Model.InputText):
				case nameof(m_Model.OutputText):
				case nameof(m_Model.InputEncoding):
				case nameof(m_Model.OutputEncoding):
					OnPropertyChanged(e);
					break;
			}
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
