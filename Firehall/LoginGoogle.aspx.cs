namespace Firehall
{
	using System;
	using System.Configuration;
	using System.Net;
	using System.Web;
	using System.Web.UI;
	using System.Collections.Generic;
	using System.IO;
	using System.Security.Claims;
	using Newtonsoft.Json;
	using System.Web.Helpers;

	/// <summary>
	/// </summary>
	/// <remarks>
	/// https://developers.google.com/accounts/docs/OpenIDConnect
	/// https://developers.google.com/+/web/signin/server-side-flow
	/// https://developer.atlassian.com/static/connect/docs/concepts/understanding-jwt.html
	/// http://leastprivilege.com/2014/06/10/writing-an-openid-connect-web-client-from-scratch/
	/// </remarks>
	public partial class LoginGoogle : Firehall.Page
	{
		protected int Stage = 0;

		private static Dictionary<string,string> pGoogleTargetParams;
		private Dictionary<string,string> GoogleTargetParams{
			get{
				if(pGoogleTargetParams == null){
					pGoogleTargetParams = new Dictionary<string,string> {
						{ "client_id","239959269801-rc9sbujsr5gv4gm43ecsavjk6s149ug7.apps.googleusercontent.com" },
						{ "response_type","code" },
						{ "scope","openid email" },
						{ "redirect_uri",Request.Url.AbsoluteUri.Split("?".ToCharArray())[0] },
						{ "state","security_token="+this.StateToken+"&url=/" },

						//{"login_hint","jefferey.cave@gmail.com"},
						{ "login_hint","" }
					};
				}
				return pGoogleTargetParams;
			}
		}

		/// <summary>
		/// Gets the google target.
		/// </summary>
		/// <value>The google target.</value>
		protected string GoogleTarget { 
			get {
				var targ = "";
				foreach (KeyValuePair<string,string> val in GoogleTargetParams) {
					/*targ += string.Format("{0}={1}&"
					                      , System.Web.HttpUtility.UrlEncode(val.Key)
					                      , System.Web.HttpUtility.UrlEncode(val.Value)
					                      );*/
					var encoded = string.Format(
						"{0}={1}&"
						, HttpUtility.UrlEncode(val.Key)
						, HttpUtility.UrlEncode(val.Value)
						);
					targ += encoded;
				}
				return "https://accounts.google.com/o/oauth2/auth?" + targ;
			}
		}

		/// <summary>
		/// Gets or sets the user email.
		/// </summary>
		/// <value>The user email.</value>
		protected string UserEmail{
			get{
				string email ="";
				try{
					email = Session["useremail"].ToString();
					if (email == null) {
						email = "";
					}
				} catch {
					email = "";
				}
				return email;
			}
			set{
				Session["useremail"] = value;
			}
		}

		/// <summary>
		/// Gets the state token.
		/// </summary>
		/// <value>The state token.</value>
		private string StateToken{
			get{
				if (Session["GoogleAuthState"] == null) {
					Session["GoogleAuthState"] = Convert
						.ToBase64String(Guid.NewGuid().ToByteArray())
						.Replace("=","")
						.Replace("+","-")
						.Replace("/","_")
						;
				}
				return Session["GoogleAuthState"].ToString();
			}
		}

		protected void Page_Load(object sender, EventArgs e) {
			string strState = Request.QueryString["state"];
			if (string.IsNullOrEmpty(strState)) {
				Response.Redirect(this.GoogleTarget);
			}
			else{
				var state = HttpUtility.ParseQueryString(strState);
				var token = state["security_token"];
				var url = state["url"];
				if (token == this.StateToken) {
					string code = Request.QueryString["code"];
					GoogleTokens result = GetGoogleTokens(code);

					code = result.access_token;
					code = result.id_token;
					code = result.expires_in;
					code = result.token_type;

					var val = result.ParsedIdToken;
					var claims = Json.Decode<Dictionary<string,object>>(val["claims"]);

					Console.Write("Authenticated: " + claims["email"].ToString() + "\n");
					this.UserEmail = claims["email"].ToString();

					var claimList = new List<Claim>();
					foreach (var claim in claims) {
						Claim c = new Claim(claim.Key, claim.Value.ToString());
						claimList.Add(c);
					}
					//iss = issuer
					//sub = subject (identifies the unique google user)
					//azp = user id?
					//aud = audience (should match our token)
					//email = email
					//at_hash = 
					//email_verified = 

					var id = new ClaimsIdentity(claimList,"iss","email",null);
					//var principal = new ClaimsPrincipal(id);
					//HttpContext.Current.User = principal;
					//Response.Redirect("~/default.aspx");

					//System.Web.Security.FormsAuthentication.SetAuthCookie(claimList["email"].ToString());
					System.Web.Security.FormsAuthentication.RedirectFromLoginPage(id.Name,true);
				}
			}
		}

		protected GoogleTokens GetGoogleTokens(string code){
			var content = 
				"code=" + code +
				"&client_id=" + this.GoogleTargetParams["client_id"] + 
				"&client_secret=QyYKQRBx7HuKI-q11oJnkK-d" +
				"&redirect_uri=" + this.GoogleTargetParams["redirect_uri"] +
				"&grant_type=authorization_code"
				;
			var byteContent = System.Text.Encoding.UTF8.GetBytes(content);

			WebRequest request = WebRequest.CreateHttp("https://www.googleapis.com/oauth2/v3/token");
			//WebRequest request = WebRequest.CreateHttp("https://www.google.com/");
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = byteContent.Length;
			var stm = request.GetRequestStream();
			stm.Write(byteContent,0,byteContent.Length);
			stm.Close();

			var response = request.GetResponse();
			stm = response.GetResponseStream();
			var reader = new StreamReader(stm);
			var json = reader.ReadToEnd();
			reader.BaseStream.Close();
			reader.Close();

			var result = System.Web.Helpers.Json.Decode<GoogleTokens>(json);

			return result;
		}

		protected class GoogleTokens{
			public string access_token;
			public string id_token;
			public string expires_in;
			public string token_type;

			/// <summary>
			/// Gets the parsed identifier token.
			/// </summary>
			/// <value>The parsed identifier token.</value>
			public Vius.QuickDict ParsedIdToken{
				get{
					string jwtToken = id_token;
					string[] segments = jwtToken.Split(".".ToCharArray());
					string base64EncodedHeader = SanitizeBase64Url(segments[0]);
					string base64EncodedClaims = SanitizeBase64Url(segments[1]);
					string signature = SanitizeBase64Url(segments[2]);

					string header = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedHeader));
					string claims = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedClaims));

					var rtn = new Vius.QuickDict();
					rtn.Add("signature", signature);
					rtn.Add("header", header);
					rtn.Add("claims", claims);
					return rtn;
				}
			}

			private string SanitizeBase64Url(string input){
				var output = input;
				output = output
					.Replace('-', '+') // 62nd char of encoding
					.Replace('_', '/') // 63rd char of encoding
					;
				switch (output.Length % 4) // Pad with trailing '='s
				{
					case 0: break; // No pad chars in this case
					case 2: output += "=="; break; // Two pad chars
					case 3: output += "="; break; // One pad char
					default: throw new System.Exception("Illegal base64url string!");
				}
				return output;
			}
		}
	}
}