using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCAAccessTools
{
	record CamperInfoAddition(int SCAID, string LastName, string FirstName, int Grade, string SchoolCode, string Level, bool Promoted, bool SCATournament, bool New, bool[] weeks, string Notes, string SwimLevel, string SwimComments, bool YESphoto, string PhotoComments, string Extras, int ContactInfoID, decimal? CamperPaid, bool Teeshirt, decimal Discount)
	{
		public static IEnumerable<CamperInfoAddition> GetAll(string week)
		{
			using var reader = File.OpenRead($"Feedback\\{week}\\Camper Info Additions {week}.csv");

			var rows = Csv.CsvReader.ReadFromStream(reader);

			foreach (var row in rows)
			{
				yield return new CamperInfoAddition(
					SCAID: int.Parse(row["ID"]),
					LastName: row["Last Name"],
					FirstName: row["First Name"],
					Grade: int.Parse(row["Grade"]),
					SchoolCode: row["SchoolCode"],
					Level: row["Level"],
					Promoted: bool.Parse(row["Promoted/Demoted"]),
					SCATournament: bool.Parse(row["SCATournaments"]),
					New: bool.Parse(row["new"]),
					weeks: [
						bool.Parse(row["1"]),
						bool.Parse(row["2"]),
						bool.Parse(row["3"]),
						bool.Parse(row["4"]),
						bool.Parse(row["5"]),
						bool.Parse(row["6"]),
						bool.Parse(row["7"]),
					],
					Notes: row["Notes"],
					SwimLevel: Program.NormalizeSwimLevel(row["SwimLevel"]),
					SwimComments: row["SwimmingComments"],
					YESphoto: bool.Parse(row["YESphoto"]),
					PhotoComments: row["PhotoComments"],
					Extras: row["Extras"],
					ContactInfoID: int.Parse(row["ID"]),
					CamperPaid: decimal.TryParse(row["CamperPaid"], out decimal camperPaid) ? camperPaid : null,
					Teeshirt: bool.Parse(row["Teeshirt"]),
					Discount: decimal.Parse(row["Discount"])
				);
			}
		}
	}
}