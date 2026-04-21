using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using APBD_Cw5.DTOs.Requests;
using APBD_Cw5.DTOs.Responses;

namespace APBD_Cw5.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        private readonly IConfiguration _configuration;

        public SqlServerDbService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<StudentResponse> GetStudents()
        {
            var students = new List<StudentResponse>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var con = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(@"
                SELECT s.IndexNumber, s.FirstName, s.LastName, s.BirthDate,
                       st.Name AS Studies, e.Semester
                FROM Student s
                JOIN Enrollment e ON s.IdEnrollment = e.IdEnrollment
                JOIN Studies st ON e.IdStudy = st.IdStudy", con);

            con.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                students.Add(new StudentResponse
                {
                    IndexNumber = reader["IndexNumber"].ToString(),
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    BirthDate = Convert.ToDateTime(reader["BirthDate"]),
                    Studies = reader["Studies"].ToString(),
                    Semester = Convert.ToInt32(reader["Semester"])
                });
            }

            return students;
        }

        public EnrollmentResponse EnrollStudent(EnrollStudentRequest request)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var con = new SqlConnection(connectionString);
            con.Open();

            using var transaction = con.BeginTransaction();

            try
            {
                int idStudy;

                using (var cmd = new SqlCommand("SELECT IdStudy FROM Studies WHERE Name = @name", con, transaction))
                {
                    cmd.Parameters.AddWithValue("@name", request.Studies);
                    var result = cmd.ExecuteScalar();

                    if (result == null)
                        throw new Exception("Studies not found");

                    idStudy = Convert.ToInt32(result);
                }

                using (var cmd = new SqlCommand("SELECT 1 FROM Student WHERE IndexNumber = @index", con, transaction))
                {
                    cmd.Parameters.AddWithValue("@index", request.IndexNumber);
                    var result = cmd.ExecuteScalar();

                    if (result != null)
                        throw new Exception("Student already exists");
                }

                int idEnrollment;
                int semester = 1;
                DateTime startDate;

                using (var cmd = new SqlCommand(@"
                    SELECT IdEnrollment, Semester, StartDate
                    FROM Enrollment
                    WHERE IdStudy = @idStudy AND Semester = 1", con, transaction))
                {
                    cmd.Parameters.AddWithValue("@idStudy", idStudy);

                    using var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        idEnrollment = Convert.ToInt32(reader["IdEnrollment"]);
                        startDate = Convert.ToDateTime(reader["StartDate"]);
                    }
                    else
                    {
                        reader.Close();

                        using var cmdMax = new SqlCommand(
                            "SELECT ISNULL(MAX(IdEnrollment), 0) + 1 FROM Enrollment",
                            con, transaction);

                        idEnrollment = Convert.ToInt32(cmdMax.ExecuteScalar());
                        startDate = DateTime.Now;

                        using var insertEnrollment = new SqlCommand(@"
                            INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate)
                            VALUES(@idEnrollment, @semester, @idStudy, @startDate)", con, transaction);

                        insertEnrollment.Parameters.AddWithValue("@idEnrollment", idEnrollment);
                        insertEnrollment.Parameters.AddWithValue("@semester", semester);
                        insertEnrollment.Parameters.AddWithValue("@idStudy", idStudy);
                        insertEnrollment.Parameters.AddWithValue("@startDate", startDate);

                        insertEnrollment.ExecuteNonQuery();
                    }
                }

                using (var cmd = new SqlCommand(@"
                    INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment)
                    VALUES(@index, @firstName, @lastName, @birthDate, @idEnrollment)", con, transaction))
                {
                    cmd.Parameters.AddWithValue("@index", request.IndexNumber);
                    cmd.Parameters.AddWithValue("@firstName", request.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", request.LastName);
                    cmd.Parameters.AddWithValue("@birthDate", request.BirthDate);
                    cmd.Parameters.AddWithValue("@idEnrollment", idEnrollment);

                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();

                return new EnrollmentResponse
                {
                    IdEnrollment = idEnrollment,
                    Semester = semester,
                    IdStudy = idStudy,
                    StartDate = startDate
                };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public EnrollmentResponse PromoteStudents(PromoteStudentsRequest request)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var con = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("PromoteStudents", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Studies", request.Studies);
            cmd.Parameters.AddWithValue("@Semester", request.Semester);

            con.Open();

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new EnrollmentResponse
                {
                    IdEnrollment = Convert.ToInt32(reader["IdEnrollment"]),
                    Semester = Convert.ToInt32(reader["Semester"]),
                    IdStudy = Convert.ToInt32(reader["IdStudy"]),
                    StartDate = Convert.ToDateTime(reader["StartDate"])
                };
            }

            throw new Exception("Promotion failed");
        }
    }
}