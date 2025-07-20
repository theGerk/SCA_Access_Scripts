using System.ComponentModel.DataAnnotations;
using System.Data.OleDb;
using System.Text;

namespace SCAAccessTools
{
	static class Program
	{
		static void Main(string[] args)
		{
			HttpClient client = new HttpClient();
			//var httpReq = client.GetAsync("https://script.google.com/macros/s/AKfycbw5zUjZgC_Gruqz6zgLnKgfTneQyU2TJXY039nDTarG3Bc21Z2jGOV9xG9FhBVto1iq/exec");
			var filename = "ChessCamp_Startof2025.accdb";
			File.Delete(filename);
			File.Copy("C:\\Users\\Benji\\Dropbox\\Ty\\Camp\\" + filename, filename);
			string week = "Wk3";
			using OleDbConnection conn = new OleDbConnection()
			{
				ConnectionString = @"Provider=Microsoft.ACE.OLEDB.16.0;Data source= " + filename,
			};

			conn.Open();
			var mapping = GetSCAIDToCamperInfoIDMapping(conn);

			foreach (var (item, id) in CamperChange.GetAll(week).Select(x => (x, mapping[x.ID])).ToList())
			{
				ExecuteCamperChange(item, id, conn);
			}

			foreach (var (item, id) in CamperInfoAddition.GetAll(week).Select(x => (x, mapping[x.PlayerID])).ToList())
			{
				AddCamper(item, id, conn);
			}

			//var csvResponse = httpReq.Result;
			//var newGroups = Csv.CsvReader.ReadFromStream(csvResponse.Content.ReadAsStream(), new Csv.CsvOptions { HeaderMode = Csv.HeaderMode.HeaderAbsent });
			//foreach (var row in newGroups)
			//{
			//	int id = int.Parse(row[0]);
			//	string group = row[1].Trim().ToLowerInvariant();
			//	var level = GroupToLevel(group);
			//	var levelCode = LevelCode(level);
			//	using var cmd = new OleDbCommand($"UPDATE [Camper Info] SET [Level] = '{level}', [LevelCode] = {levelCode} WHERE [Player ID] = {id}", conn);
			//	cmd.ExecuteNonQuery();
			//}
		}

		public static string GroupToLevel(string group) => group switch
		{
			"purple" => "B",
			"red" => "AB",
			"brown" => "I",
			"blue" => "A",
			"gold" => "SA",
			_ => throw new NotImplementedException($"Group {group} not supported yet")
		};

		public static void FixSession(OleDbConnection conn)
		{
			using var cmd = new OleDbCommand("UPDATE [Camper Info] SET [Session] = 1 WHERE [Session] IS NULL", conn);
			cmd.ExecuteNonQuery();
		}

		public static Dictionary<int, int> GetSCAIDToCamperInfoIDMapping(OleDbConnection dbConn)
		{
			var mapping = new Dictionary<int, int>();

			using (var cmd = new OleDbCommand("SELECT [ID], [Player ID] FROM [Camper Info]", dbConn))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					int camperInfoId = reader.GetInt32(0);
					int scaId = reader.GetInt32(1);
					mapping[scaId] = camperInfoId;
				}
			}

			return mapping;
		}

		public static void AddCamper(CamperInfoAddition addition, int id, OleDbConnection dbConn)
		{
			string sql = @$"UPDATE [Camper Info]
SET 
	[Last Name] = '{addition.LastName}',
	[First Name] = '{addition.FirstName}',
	[Grade] = {addition.Grade},
	[SchoolCode] = '{addition.SchoolCode}',
	[Level] = '{addition.Level}',
	[LevelCode] = {LevelCode(addition.Level)},
	[SCATournaments] = {addition.SCATournament},
	[new] = {addition.New},
	[1] = {addition.weeks[0]},
	[2] = {addition.weeks[1]},
	[3] = {addition.weeks[2]},
	[4] = {addition.weeks[3]},
	[5] = {addition.weeks[4]},
	[6] = {addition.weeks[5]},
	[7] = {addition.weeks[6]},
	[Session] = {(addition.weeks.Any(x => x) ? $"'{string.Join(", ", addition.weeks.Select((x, i) => x ? i + 1 : 0).Where(x => x > 0))}'" : "NULL")},
	[Notes] = {(addition.Notes == string.Empty ? "NULL" : $"'{addition.Notes}'")},
	[SwimLevel] = '{addition.SwimLevel}',
	[SwimCode] = {SwimCode(addition.SwimLevel)},
	[SwimmingComments] = {(addition.SwimComments == string.Empty ? "NULL" : $"'{addition.SwimComments}'")},
	[YESphoto] = {addition.YESphoto},
	[PhotoComments] = {(addition.PhotoComments == string.Empty ? "NULL" : $"'{addition.PhotoComments}'")},
	[Extras] = {(addition.Extras == string.Empty ? "NULL" : $"'{addition.Extras}'")}
WHERE ID = {id}";
			new OleDbCommand(sql, dbConn).ExecuteNonQuery();
		}

		static Dictionary<string, int> SwimCodes = new Dictionary<string, int>
		{
			["Unknown"] = 0,
			["Non-Swimmer"] = 1,
			["Shallow and Slide"] = 2,
			["Confident Swimmer"] = 3,
		};

		public static string NormalizeSwimLevel(string swimLevel)
		{
			// check if caps is off.
			if (swimLevel == null || swimLevel.Length == 0)
				return "Unknown";


			foreach (var kvp in SwimCodes)
			{
				if (swimLevel.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase))
					return kvp.Key;
			}

			return swimLevel;
		}

		static int LevelCode(string level) => level switch
		{
			"U" => 0,
			"B" => 1,
			"AB" => 2,
			"I" => 3,
			"A" => 4,
			"SA" => 5,
			_ => throw new NotImplementedException($"Level {level} not supported yet")
		};
		static int SwimCode(string swimLevel) => SwimCodes.TryGetValue(swimLevel, out var code) ? code : throw new NotImplementedException($"SwimCode {swimLevel} not supported yet");

		public static void ExecuteCamperChange(CamperChange change, int id, OleDbConnection dbConn)
		{
			switch (change.Table)
			{
				case "[CamperInfo]":
					{
						StringBuilder query = new($"UPDATE [Camper Info] SET {change.Field[..^1]} = '{change.New}'");
						switch (change.Field)
						{
							case "[Player ID]:":
								query.Append(", [SCATournaments] = 1");
								break;
							case "[Level]:":
								query.Append($", [LevelCode] = {LevelCode(change.New)}");
								break;
							case "[SwimLevel]:":
								query.Append($", [SwimCode] = {SwimCode(change.New)}");
								break;
							case "[1]:":
							case "[2]:":
							case "[3]:":
							case "[4]:":
							case "[5]:":
							case "[6]:":
							case "[7]:":
							case "[Grade]:":
							case "[Last Name]:":
							case "[First Name]:":
							case "[SchoolCode]:":
							case "[Notes]:":
								break;
							default:
								throw new NotImplementedException($"Field {change.Field} not implemented in ExecuteCamperChange");
						}
						query.Append($" WHERE ID = {id}");
						using (var command = new OleDbCommand(query.ToString(), dbConn))
							command.ExecuteNonQuery();

						// Update session
						if (int.TryParse(change.Field[1..^2], out var _))
						{
							using var selectCommand = new OleDbCommand($"SELECT [1], [2], [3], [4], [5], [6], [7] FROM [Camper Info] WHERE ID = {id}", dbConn);
							var reader = selectCommand.ExecuteReader();
							reader.Read();
							var weeks = new List<int>();
							for (int i = 0; i < 7; i++)
								if (reader.GetBoolean(i))
									weeks.Add(i + 1);
							using var updateCommand = new OleDbCommand($"UPDATE [Camper Info] SET [Session] = {(weeks.Any() ? $"'{string.Join(", ", weeks)}'" : "NULL")} WHERE ID = {id}", dbConn);
							updateCommand.ExecuteNonQuery();
						}
						break;
					}
				case "[NewPlayer]":
					break;
				default:
					throw new NotImplementedException($"Table {change.Table} not implemented in ExecuteCamperChange");
			}
		}
	}
}
