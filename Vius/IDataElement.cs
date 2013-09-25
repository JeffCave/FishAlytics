using System;

namespace Vius
{
	public interface IDataElement : IDisposable
	{
		bool IsNew{ get; }
		bool IsDirty{ get; }
		bool IsValid{ get; }
	}
}

