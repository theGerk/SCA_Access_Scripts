using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCAAccessTools
{
	record CamperChange(int ID, string LastName, string FirstName, string Table, string Field, string Old, string Comment, string New, DateTime ChangedAfter, DateTime ChangedBefore, int Week)
	{
		public static IEnumerable<CamperChange> GetAll(string week)
		{
			using var reader = File.OpenRead($"Feedback\\{week}\\Camper Changes {week}.csv");
			
			var rows = Csv.CsvReader.ReadFromStream(reader);

			foreach (var row in rows)
			{
				yield return new CamperChange(
					int.Parse(row[0]),
					row[1],
					row[2],
					row[3],
					row[4],
					row[5],
					row[6],
					row[7],
					DateTime.Parse(row[8]),
					DateTime.Parse(row[9]),
					int.Parse(row[10])
				);
			}
		}
	}

}
