using System.ComponentModel;
using System.Text;

namespace EncodingChanger
{
	public sealed class EncodingViewModel : INotifyPropertyChanged
	{
		public EncodingViewModel(EncodingInfo info) => m_Info = info;

		readonly EncodingInfo m_Info;

		public string Name => m_Info.Name;
		public string DisplayName => m_Info.DisplayName;
		public int CodePage => m_Info.CodePage;

		public Encoding GetEncoding() => m_Info.GetEncoding();

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { }
			remove { }
		}
	}
}
