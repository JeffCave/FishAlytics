using System;

namespace Vius
{
	public abstract class BaseDataElement<PK>:IDataElement<PK>
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

		public virtual void Validate(){
		}

		public abstract void Save();

		public abstract void Load();
		public abstract void Load(PK primarykey);

		public abstract void Delete();

		public abstract void Create();
	}
}

