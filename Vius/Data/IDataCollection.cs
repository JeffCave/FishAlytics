using System;
using System.Collections.Generic;

namespace Vius.Data
{
	public interface IDataCollection<Element> :IList<Element>
			where Element:IDataElement
	{
	}
}

