using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCAAccessTools
{
	record CamperInfoAddition(int PlayerID, string LastName, string FirstName, int Grade, string SchoolCode, string Level, bool Promoted, bool SCATournament, bool New, bool[] weeks, string Notes, string SwimLevel, string SwimComments, bool YESphoto, string PhotoComments, string Extras)
	{
		public static IEnumerable<CamperInfoAddition> GetAll(string week)
		{
			using var reader = File.OpenRead($"Feedback\\{week}\\Camper Info Additions {week}.csv");

			var rows = Csv.CsvReader.ReadFromStream(reader);

			foreach (var row in rows)
			{
				yield return new CamperInfoAddition(
					PlayerID: int.Parse(row[0]),
					LastName: row[1],
					FirstName: row[2],
					Grade: int.Parse(row[3]),
					SchoolCode: row[4],
					Level: row[5],
					Promoted: bool.Parse(row[6]),
					SCATournament: bool.Parse(row[7]),
					New: bool.Parse(row[8]),
					weeks: row.Values[10..17].Select(x => bool.Parse(x)).ToArray(),
					Notes: row[21],
					SwimLevel: Program.NormalizeSwimLevel(row[22]),
					SwimComments: row[23],
					YESphoto: bool.Parse(row[24]),
					PhotoComments: row[25],
					Extras: row[22]
				);
			}
		}
	}
}