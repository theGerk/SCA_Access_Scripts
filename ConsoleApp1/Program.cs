namespace ConsoleApp1
{
	using System;
	using System.Data;
	using System.Data.OleDb;
	using System.Text;

	class AccessDatabaseExample
	{
		static string connectionString(string db) => $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={db};Persist Security Info=False;";

		static void Main()
		{
			string startDB = "ChessCamp_Startof2025.accdb";
			string endDB = @"Camp 2025_Wk2.accdb";


			(string, string)[] tables = [("SELECT [All].[PrimaryID] AS [ID], [All].[Player ID], [ALL].[Last Name], [ALL].[First Name], [ALL].[Grade], [ALL].[SchoolCode], [ALL].[Level], [ALL].[Promoted/Demoted], [ALL].[SCATournaments], [ALL].[new], [ALL].[Time], [ALL].[1], [ALL].[2], [ALL].[3], [ALL].[4], [ALL].[5], [ALL].[6], [ALL].[7], [ALL].[JuneUnder], [ALL].[JuneOver], [ALL].[AugUnder], [ALL].[AugOver], [ALL].[Notes], [ALL].[SwimLevel], [ALL].[SwimmingComments], [ALL].[YESphoto], [ALL].[PhotoComments], [ALL].[Extras], [All].[CamperPaid], [All].[Teeshirt], [All].[Discount]\r\nFROM [ALL];", "Camper Info"), ("SELECT [Camper Info].[Player ID], [Camper Info].[Last Name], [Camper Info].[First Name], [Camp Contact Info].Address, [Camp Contact Info].City, [Camp Contact Info].State, [Camp Contact Info].ZipCode, [Camp Contact Info].HPhone, [Camp Contact Info].WPhone1, [Camp Contact Info].WPhone2, [Camp Contact Info].CPhone, [Camp Contact Info].EMail\r\nFROM [Camp Contact Info] INNER JOIN [Camper Info] ON [Camp Contact Info].[PrimaryID] = [Camper Info].[ID];", "Contact Info"), ("SELECT [Camper Info].[Player ID], [Camper Info].[Last Name], [Camper Info].[First Name], [Camp Contact Info].Address, [Camp Contact Info].City, [Camp Contact Info].State, [Camp Contact Info].ZipCode, [Camp Contact Info].HPhone, [Camper Info].SchoolCode, [Camper Info].Grade, [Camp Contact Info].EMail\r\nFROM [Camp Contact Info] INNER JOIN [Camper Info] ON [Camp Contact Info].PrimaryID = [Camper Info].ID\r\nWHERE ((([Camper Info].[Player ID])>=60528) OR (([Camper Info].[Player ID])=0))\r\nORDER BY [Camper Info].[Last Name], [Camper Info].[First Name];\r\n", "New Player")];

			using (OleDbConnection readDB = new OleDbConnection(connectionString(startDB)))
			using (OleDbConnection writeDB = new OleDbConnection(connectionString(endDB)))
			{
				readDB.Open();
				writeDB.Open();

				foreach (var (readQuery, writeTable) in tables)
				{
					using (OleDbCommand readCommand = new OleDbCommand(readQuery, readDB))
					{
						using (OleDbDataReader reader = readCommand.ExecuteReader())
						{
							StringBuilder insertCommandBuilder = new StringBuilder();
							insertCommandBuilder.Append($"INSERT INTO [{writeTable}] (");
							for (int i = 0; i < reader.FieldCount; i++)
							{
								insertCommandBuilder.Append($"[{reader.GetName(i)}]");
								if (i < reader.FieldCount - 1)
									insertCommandBuilder.Append(", ");
							}
							insertCommandBuilder.Append(") VALUES (");
							for (int i = 0; i < reader.FieldCount; i++)
							{
								insertCommandBuilder.Append($"@param{i}");
								if (i < reader.FieldCount - 1)
									insertCommandBuilder.Append(", ");
							}
							insertCommandBuilder.Append(");");
							string insertCommandText = insertCommandBuilder.ToString();
							// truncate table first
							using (OleDbCommand truncateCommand = new OleDbCommand($"DELETE FROM [{writeTable}]", writeDB))
							{
								truncateCommand.ExecuteNonQuery();
							}
							while (reader.Read())
							{
								using (OleDbCommand writeCommand = new OleDbCommand(insertCommandText, writeDB))
								{
									for (int i = 0; i < reader.FieldCount; i++)
									{
										writeCommand.Parameters.AddWithValue($"@param{i}", reader.GetValue(i) ?? DBNull.Value);
									}
									writeCommand.ExecuteNonQuery();
								}
							}
						}
					}
				}
			}
		}
	}
}
