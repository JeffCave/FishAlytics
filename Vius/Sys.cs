using System;
using System.Web;

namespace Vius
{
public class Sys
{
	private static object locker = new object();
	
	private static QuickDict config;

	private static DateTime LastLoad = DateTime.MinValue;

	/// ----------------------------------------------------------------------------
	private Sys()
	{
	}

	/// ----------------------------------------------------------------------------
	~Sys()
	{
	}

	/// --- IsAged -----------------------------------------------------------------
	/// <summary>Is the data aged</summary>
	public static bool IsAged
	{
		get
		{
			System.TimeSpan diffTime = DateTime.Now.Subtract(LastLoad);
			return (LifeSpan < diffTime);
		}
	}

	/// --- LifeSpan ---------------------------------------------------------------
	/// <summary>The lifespan the data is valid for.</summary>
	public static TimeSpan LifeSpan
	{
		get
		{
			int hours;
			try {
				hours = int.Parse(config["sys.param.lifespan"]);
			}
			catch {
				hours = 1;
			}
			return new TimeSpan(hours, 0, 0);
		}
	}

	/// --- ExpireNow --------------------------------------------------------------
	/// <summary>
	/// </summary>
	public static void ExpireNow()
	{
		LastLoad = DateTime.MinValue;
	}

	/// ----------------------------------------------------------------------------
	public static QuickDict Settings
	{
		get
		{
			Load();
			//send the data up
			return config;
		}
	}

	/// ----------------------------------------------------------------------------
	/// <summary></summary>
	public static System.Collections.Specialized. Entities
	{
		get
		{
				return Capabilities.AllCapabilities;
		}
	}


	/// ----------------------------------------------------------------------------
	/// <summary></summary>
	public static List<MenuItem> SiteMap
	{
		get
		{
			Load();
			return sitemap;
		}
	}
	
	/// ----------------------------------------------------------------------------
	/// <summary></summary>
	public static void Load()
	{
		if (!IsAged) {
			return;
		}
		LastLoad = DateTime.Now;

		lock (locker) {
			Database.UseCommand(LoadSettings);
			Database.UseCommand(LoadEntityMasks);
			Database.UseCommand(LoadSiteMap);
		}
	}

	/// ----------------------------------------------------------------------------
	/// <summary>Loads the web settings.</summary>
	private static Database.CommandUsage LoadSettings = cmd =>
	{
		string key;
		string val;
		cmd.CommandText =
			"select *    \n" +
			"from   tWebParams \n";
		using (NpgsqlDataReader rs = cmd.ExecuteReader()) {
			if (null == config) {
				config = new QuickDict();
			}
			config.Clear();
			while (rs.Read()) {
				key = rs["name"].ToString().Trim();
				val = rs["val"].ToString().Trim();
				if (!string.IsNullOrEmpty(key)) {
					config[key] = val;
				}
			}
		}
	};
	

	/// <summary>Loads the entity masks.</summary>
	private static Database.CommandUsage LoadEntityMasks = (cmd) =>
	{
		cmd.CommandText =
			"select entity_mask::bit(64)::bigint as mask, \n" +
			"       name, \n" +
			"       description \n" +
			"from   tAuthEntities \n" +
			"order by entity_mask \n"
			;
		using (NpgsqlDataReader rs = cmd.ExecuteReader()) {

			if (null == entities) {
				entities = new EntityList();
			}
			entities.Clear();
			string key;
			Int64 mask;
			while (rs.Read()) {
				key = rs["name"].ToString().ToLower();
				mask = Int64.Parse(rs["mask"].ToString());
				if (!string.IsNullOrEmpty(key)) {
					entities[key] = mask;
				}
			}
		}

	};

	private static Database.CommandUsage LoadSiteMap = (cmd) =>
	{
		cmd.CommandText =
			"select *    \n" +
			"from   tWebPages \n";
		if (null == sitemap) {
			sitemap = new List<MenuItem>();
		}
		sitemap.Clear();
		using (NpgsqlDataReader rs = cmd.ExecuteReader()) {
			/// TODO: need to actually read the stuff
		}
	};


}


}
