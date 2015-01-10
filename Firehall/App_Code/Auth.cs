using System;
using System.Web.Security;
using System.Web;

namespace Firehall.Web
{
	public static class Auth
	{
		private static HttpContext Context{
			get{
				return HttpContext.Current;
			}
		}
		public static void SignIn(string email, string redirect){
			FormsAuthentication.SetAuthCookie(email, true);
		}
		public static void SignIn(string email, bool redirect = false){
			FormsAuthentication.SetAuthCookie(email, true);
		}
		public static void SignOut(){
			var cookies = HttpContext.Current.Response.Cookies;

			HttpContext.Current.User = null;

			// clear authentication cookie
//			FormsAuthentication.SignOut();
//			FormsAuthentication.SetAuthCookie("", true);
//			//HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
			HttpCookie cookie = FormsAuthentication.GetAuthCookie("",false);
			cookie.Value = "";
			//cookie.Expires = DateTime.MinValue;
			cookies.Add(cookie);

			// clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
//			Context.Session.Abandon();
//			HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
//			cookie2.Expires = DateTime.Now.AddYears(-1);
//			HttpContext.Current.Response.Cookies.Add(cookie2);

//			FormsAuthentication.RedirectToLoginPage();		
		}
	}
}

