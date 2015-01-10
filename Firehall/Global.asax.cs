using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Security.Principal;

namespace Firehall
{
	public partial class Global : System.Web.HttpApplication
	{
		
		protected virtual void Application_Start (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Session_Start (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_BeginRequest (Object sender, EventArgs e)
		{
			Context.Response.Expires = -1;
		}
		
		/// <summary>
		/// Applications the authenticate request.
		/// </summary>
		/// <remarks>>
		/// http://tech.pro/tutorial/1216/implementing-custom-authentication-for-aspnet
		/// </remarks>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected virtual void Application_AuthenticateRequest (Object sender, EventArgs e)
		{
		}

		protected virtual void Application_EndRequest (Object sender, EventArgs e)
		{
		}

		protected virtual void Application_Error (Object sender, EventArgs e)
		{
			Console.WriteLine("Application_Error");
			// Get the exception object.
			Exception exc = Server.GetLastError();

			// Handle HTTP errors
			if (exc.GetType() == typeof(HttpException)) {
				//Redirect HTTP errors to HttpError page
				Server.Transfer("HttpErrorPage.aspx");
				return;
			}

			// For other kinds of errors give the user some information
			// but stay on the default page
			Response.Write("<h2>Global Page Error</h2>\n");
			Response.Write("<p>" + exc.Message + "</p>\n");

			Response.Write("<p>Return to the <a href='"+System.Web.VirtualPathUtility.ToAbsolute("~/")+"'>Default Page</a></p>\n");
				
			// Log the exception and notify system operators
			var stack = "Unhandled Exception:\n" +
			          exc.InnerException.Message + "\n" +
			          exc.InnerException.StackTrace;
			System.Console.WriteLine(stack);
			Response.Write("<pre>" + stack + "</pre>");

			// Clear the error from the server
			Server.ClearError();
		}
		
		protected virtual void Session_End (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_End (Object sender, EventArgs e)
		{
		}
	}
}

