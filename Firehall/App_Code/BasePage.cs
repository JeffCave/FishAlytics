using System;

namespace Firehall
{
	/// <summary>
	/// Page.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public class Page:Vius.Web.Page
	{
		public Page ()
		{
			ClientScripts();
		}

		public void ClientScripts()
		{
			ClientScript.RegisterClientScriptInclude("objectwatch","/jslib/ObjectWatch.js");
		}
	}
}

