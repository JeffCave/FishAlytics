using System;

namespace Vius
{
	public abstract class BaseDataElement:IDataElement
	{
		public BaseDataElement ()
		{
		}

		public abstract bool IsNew{
			get;
		}

		public abstract bool IsDirty{
			get;
		}

		public bool IsValid {
			get {
				try {
					Validate();
					return true;
				} catch {
					return false;
				}
			}
		}

		public abstract void Dispose ();

		public abstract void Validate();
	}
}

