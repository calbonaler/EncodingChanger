using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace EncodingChanger
{
	public class Model : INotifyPropertyChanged
	{
		public Model()
		{
			var encodings = Encoding.GetEncodings();
			Array.Sort(encodings, (x, y) => x.DisplayName.CompareTo(y.DisplayName));
			Encodings = new ReadOnlyCollection<EncodingInfo>(encodings);
		}

		string m_InputText;
		string m_OutputText;
		Encoding m_InputEncoding;
		Encoding m_OutputEncoding;

		public string InputText
		{
			get => m_InputText;
			set => this.SetProperty(ref m_InputText, value);
		}

		public string OutputText
		{
			get => m_OutputText;
			private set => this.SetProperty(ref m_OutputText, value);
		}

		public Encoding InputEncoding
		{
			get => m_InputEncoding;
			set => this.SetProperty(ref m_InputEncoding, value);
		}

		public Encoding OutputEncoding
		{
			get => m_OutputEncoding;
			set => this.SetProperty(ref m_OutputEncoding, value);
		}

		public ReadOnlyCollection<EncodingInfo> Encodings { get; }

		public void SwapInputOutputEncodings()
		{
			var tmp = InputEncoding;
			InputEncoding = OutputEncoding;
			OutputEncoding = tmp;
		}

		public void SetOutputTextToInputText() => InputText = OutputText;

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
			switch (e.PropertyName)
			{
				case nameof(InputText):
				case nameof(InputEncoding):
				case nameof(OutputEncoding):
					if (InputText != null && InputEncoding != null && OutputEncoding != null)
						OutputText = OutputEncoding.GetString(InputEncoding.GetBytes(InputText));
					break;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
