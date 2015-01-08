using System;

namespace Vius
{
	public struct Fixed
	{
		internal int val;
		internal int precision;

		public Fixed (double val)
			:this()
		{
			this.val = (int)Math.Round(val*(10^precision));
		}

		public Fixed (long val):this((double)val){}
		public Fixed (int val):this((double)val){}

		public Fixed (Fixed val, short scale = 4)
			:this()
		{
			this.val = val.val;
			this.precision = scale;
		}

		public static implicit operator int(Fixed d)
		{
		    return d.val;
		}
		public static implicit operator double(Fixed d)
		{
		    return d.val;
		}
		public static implicit operator Fixed(double d)
		{
		    return new Fixed(d);
		}
	}
}

