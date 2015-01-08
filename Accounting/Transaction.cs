using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Accounting
{
	[Serializable]
	public class Transaction: ISerializable
	{
		public int Id;
		public DateTime Date;
		public string Comment;

		public Vius.Fixed Amount{
			get{
				return 0;
			}
		}


		public Transaction ()
		{
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("id", Id);
			info.AddValue("date", Date);
			info.AddValue("memo", Comment);
		}

	}
}

