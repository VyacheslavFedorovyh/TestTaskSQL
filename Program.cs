using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace TestTaskSQL
{
	class Program
	{
		private static SqlConnection sqlConnection = null;

		static void Main(string[] args)
		{
			sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CompanyDB"].ConnectionString);

			sqlConnection.Open();

			if (sqlConnection.State == ConnectionState.Open)
			{
				Console.WriteLine("Подключение установленно");
				Console.WriteLine();

				Console.WriteLine("1. Суммарная зарплата в разрезе департаментов");
				Console.WriteLine();

				SqlCommand withoutLead = new SqlCommand(
					$"SELECT department.name, SUM(employee.salary) AS total FROM employee INNER JOIN department ON employee.department_id = department.id GROUP BY department.name; "
					, sqlConnection);

				using (SqlDataReader reader = withoutLead.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine(String.Format("Без руководителя в {0}, {1}",
							reader[0], reader[1]));
					}
				}

				Console.WriteLine();

				SqlCommand withLead = new SqlCommand(
					$"with a as (SELECT department_id, chief_id, SUM(employee.salary) AS total FROM employee WHERE chief_id is not null GROUP BY department_id, chief_id) select department.name, salary + total as total FROM a INNER JOIN department ON a.department_id = department.id inner join employee as e on a.chief_id = e.id "
					, sqlConnection);

				using (SqlDataReader reader = withLead.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine(String.Format("С руководителями в {0}, {1}",
							reader[0], reader[1]));
					}
				}

				Console.WriteLine();

				SqlCommand departmentMaxSum = new SqlCommand(
					$"SELECT department.name, employee.id FROM employee INNER JOIN department ON employee.department_id = department.id WHERE employee.salary = (SELECT MAX(employee.salary) FROM employee);"
					, sqlConnection);

				using (SqlDataReader reader = departmentMaxSum.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine(String.Format("2. Максимальная зарплата в департаменте {0}, у сотрудника {1}", reader[0], reader[1]));
					}
				}

				Console.WriteLine();

				Console.WriteLine("3. Зарплаты руководителей департаментов (по убыванию)");
				Console.WriteLine();

				SqlCommand salaryLead = new SqlCommand(
					$"SELECT id, salary FROM employee WHERE department_id = (SELECT MAX(department_id) FROM employee) ORDER BY salary DESC"
					, sqlConnection);

				using (SqlDataReader reader = salaryLead.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine(String.Format("id {0} = {1}", reader[0], reader[1]));
					}
				}
			}

			Console.ReadLine();
		}
	}
}