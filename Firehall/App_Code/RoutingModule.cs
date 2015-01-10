using System;
using System.Web;

namespace Firehall
{
	/// <summary>
	/// Routing module.
	/// </summary>
	/// <remarks>
	/// <pre>
	/// http://www.codeproject.com/Articles/335968/Implementing-HTTPHandler-and-HTTPModule-in-ASP-NET
	/// http://msdn.microsoft.com/en-us/library/cc668201%28v=vs.140%29.aspx#adding_routes_to_a_web_forms_application
	/// </pre>
	/// </remarks>
	public class RoutingModule : IHttpModule
	{
		private static class CollectionKeys{
			public static string OrigUrl = "RoutingModule.OrigUrl";
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Firehall.RoutingModule"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Firehall.RoutingModule"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Firehall.RoutingModule"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="Firehall.RoutingModule"/> so the garbage
		/// collector can reclaim the memory that the <see cref="Firehall.RoutingModule"/> was occupying.</remarks>
		public void Dispose()
		{

		}

		/// <summary>
		/// Init the specified context.
		/// </summary>
		/// <param name="context">Context.</param>
		public void Init(HttpApplication context)
		{
			context.BeginRequest += new EventHandler(context_BeginRequest);
			context.PreRequestHandlerExecute += new EventHandler(context_PreRequestHandlerExecute);
			context.EndRequest += new EventHandler(context_EndRequest);
			context.AuthorizeRequest += new EventHandler(context_AuthorizeRequest);
		}

		/// <summary>
		/// Authorize portion of the request.
		/// </summary>
		/// <param name="sender">HttpApplication object</param>
		/// <param name="e">Standard Event arguments</param>
		void context_AuthorizeRequest(object sender, EventArgs e)
		{
			//We change uri for invoking correct handler
			HttpContext context = ((HttpApplication)sender).Context;
			var uri = context.Request.Url;

			if (uri.AbsolutePath.EndsWith(".html"))
			{
				string url = uri.ToString().Replace(".html", ".aspx");
				context.RewritePath(url);
			}
		}

		/// <summary>
		/// Contexts the pre request handler execute.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void context_PreRequestHandlerExecute(object sender, EventArgs e)
		{
			//We set back the original url on browser
			HttpContext context = ((HttpApplication)sender).Context;

			if (context.Items[CollectionKeys.OrigUrl] != null)
			{
				context.RewritePath((string)context.Items[CollectionKeys.OrigUrl]);
			}
		}

		/// <summary>
		/// Contexts the end request.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void context_EndRequest(object sender, EventArgs e)
		{
			//We processed the request
		}

		/// <summary>
		/// Contexts the begin request.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void context_BeginRequest(object sender, EventArgs e)
		{
			//We received a request, so we save the original URL here
			HttpContext context = ((HttpApplication)sender).Context;
			context.Items[CollectionKeys.OrigUrl] = context.Request.RawUrl;
		}
	}
}

