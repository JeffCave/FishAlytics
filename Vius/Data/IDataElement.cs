using System;

namespace Vius
{
	public interface IDataElement<PK>: IDataElement{
		void Load(PK primarykey);
	}
	public interface IDataElement : IDisposable
	{
		bool IsNew{ get; }
		bool IsDirty{ get; }
		bool IsValid{ get; }

		void Save();
		void Load();
		void Validate();
		void Delete();
		void Create();
	}

}

