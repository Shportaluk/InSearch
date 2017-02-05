using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using InSearch;
using InSearch.Entities;

namespace InSearch.Repositories
{
    public class SqlRepository
    {
        private string _conStr = ConfigurationManager.ConnectionStrings["con_str_to_fake_db"].ConnectionString;
        private SqlConnection _con;

        public SqlRepository()
        {
            _con = new SqlConnection(_conStr);
        }


        public void NewQuestions(string login, string nameQuestion, string opinionsAns)
        {
            string[] list = opinionsAns.Split( new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach( string askAndOpinions in list )
            {
                string[] arr = askAndOpinions.Split( ':' );
                string comm = "INSERT INTO Questions VALUES( '" + nameQuestion + "', '" + arr[0] + "', '" + arr[1] + "')";

                SqlCommand sqlComm = new SqlCommand(comm, _con);

                _con.Open();
                SqlDataReader reader = sqlComm.ExecuteReader();
                _con.Close();
            }
        }

        public string CompleteQuestion(string login, string nameQuestion, string opinionsAns)
        {
            string res = null;
            string comm = "EXEC CompleteQuestion @login = @Login, @nameQuestion = @NameQuestion, @opinionsAns = @OpinionsAns";

            SqlCommand sqlComm = new SqlCommand(comm, _con);

            SqlParameter Param1 = new SqlParameter("@Login", System.Data.SqlDbType.NVarChar);
            SqlParameter Param2 = new SqlParameter("@NameQuestion", System.Data.SqlDbType.NVarChar);
            SqlParameter Param3 = new SqlParameter("@OpinionsAns", System.Data.SqlDbType.NVarChar);

            Param1.Value = login;
            Param2.Value = nameQuestion;
            Param3.Value = opinionsAns;

            sqlComm.Parameters.Add(Param1);
            sqlComm.Parameters.Add(Param2);
            sqlComm.Parameters.Add(Param3);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();

            while (reader.Read())
            {
                res = reader[0].ToString();
            }

            _con.Close();
            return res;
        }
        public User GetUserByLogin ( string login )
        {
            User user = null;
            string comm = "SELECT * FROM Users WHERE Login = @Login";

            SqlCommand sqlComm = new SqlCommand(comm, _con);
            SqlParameter Param1 = new SqlParameter("@Login", System.Data.SqlDbType.NVarChar);

            Param1.Value = login;

            sqlComm.Parameters.Add(Param1);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();

            while (reader.Read())
            {
                user = new User();
                user.Login = reader[1].ToString();
                user.FirstName = reader[3].ToString();
                user.LastName = reader[4].ToString();
                user.Sex = reader[5].ToString();
                user.Country = reader[6].ToString();
                user.City = reader[7].ToString();
                user.Phone = reader[8].ToString();
                user.PathImg = reader[9].ToString();
                user.MiniPathImg = "mini_" + reader[9].ToString();
                user.Roles = reader[10].ToString();
                user.IsOnline = Boolean.Parse(reader[11].ToString());
            }

            _con.Close();
            return user;
        }
        public List<Question> GetQuestionByName(string name)
        {
            List<Question> listQuestions = new List<Question>();
            string comm = "SELECT Ask, OptionsAns FROM Questions WHERE Name = @Name";

            SqlCommand sqlComm = new SqlCommand(comm, _con);
            SqlParameter Param1 = new SqlParameter("@Name", System.Data.SqlDbType.NVarChar);

            Param1.Value = name;
            sqlComm.Parameters.Add(Param1);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();

            while (reader.Read())
            {
                Question question = new Question();
                question.Ask = reader[0].ToString();
                question.OptionsAnswer = reader[1].ToString().Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                listQuestions.Add(question);
            }

            _con.Close();
            return listQuestions;
        }
        public string GetRolesOfUser( string login )
        {
            string res = null;
            string comm = "SELECT Roles FROM Users WHERE Login = @Login";

            SqlCommand sqlComm = new SqlCommand(comm, _con);

            SqlParameter Param1 = new SqlParameter("@Login", System.Data.SqlDbType.NVarChar);

            Param1.Value = login;

            sqlComm.Parameters.Add(Param1);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();

            while (reader.Read())
            {
                res = reader[0].ToString();
            }

            _con.Close();
            return res;
        }
        public string LogIn(string login, string pass)
        {
            string res = null;
            string comm = "EXEC LogIn @login = @Login, @pass = @Pass";

            SqlCommand sqlComm = new SqlCommand(comm, _con);

            SqlParameter Param1 = new SqlParameter("@Login", System.Data.SqlDbType.NVarChar);
            SqlParameter Param2 = new SqlParameter("@Pass", System.Data.SqlDbType.NVarChar);

            Param1.Value = login;
            Param2.Value = pass;

            sqlComm.Parameters.Add(Param1);
            sqlComm.Parameters.Add(Param2);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();

            while (reader.Read())
            {
                res = reader[0].ToString();
            }

            _con.Close();
            return res;
        }
        public string LogInWithoutPass(string login)
        {
            string res = null;
            string comm = "EXEC LogInWithoutPass @login = @Login";

            SqlCommand sqlComm = new SqlCommand(comm, _con);
            SqlParameter Param1 = new SqlParameter("@Login", System.Data.SqlDbType.NVarChar);

            Param1.Value = login;
            sqlComm.Parameters.Add(Param1);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();

            while (reader.Read())
            {
                res = reader[0].ToString();
            }

            _con.Close();
            return res;
        }
        public void LogOut(string login)
        {
            SqlCommand sqlComm = new SqlCommand( "UPDATE Users SET IsOnline = 0 WHERE Login = @login" , _con);
            SqlParameter Param1 = new SqlParameter("@Login", System.Data.SqlDbType.NVarChar);
            Param1.Value = login;
            sqlComm.Parameters.Add(Param1);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();
            _con.Close();
        }
        public string Registration(string login, string pass, string firstName, string lastName, string sex)
        {
            string res = null;
            string comm = "EXEC Registration @login = @Login, @pass = @Pass, @firstName = @FirstName, @lastName = @LastName, @sex = @Sex";

            SqlCommand sqlComm = new SqlCommand(comm, _con);

            SqlParameter Param1 = new SqlParameter("@Login", System.Data.SqlDbType.NVarChar);
            SqlParameter Param2 = new SqlParameter("@Pass", System.Data.SqlDbType.NVarChar);
            SqlParameter Param3 = new SqlParameter("@FirstName", System.Data.SqlDbType.NVarChar);
            SqlParameter Param4 = new SqlParameter("@LastName", System.Data.SqlDbType.NVarChar);
            SqlParameter Param5 = new SqlParameter("@Sex", System.Data.SqlDbType.NVarChar);

            Param1.Value = login;
            Param2.Value = pass;
            Param3.Value = firstName;
            Param4.Value = lastName;
            Param5.Value = sex;

            sqlComm.Parameters.Add(Param1);
            sqlComm.Parameters.Add(Param2);
            sqlComm.Parameters.Add(Param3);
            sqlComm.Parameters.Add(Param4);
            sqlComm.Parameters.Add(Param5);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();

            while (reader.Read())
            {
                res = reader[0].ToString();
            }

            _con.Close();
            return res;
        }
        public string Registration(User user)
        {
            string res = null;
            string comm = "EXEC RegistrationTwo @login = @Login, @pass = @Pass, @firstName = @FirstName, @lastName = @LastName, @sex = @Sex, @coutry = @Coutry, @city = @City, @pathImg= @PathImg";

            SqlCommand sqlComm = new SqlCommand(comm, _con);

            SqlParameter Param1 = new SqlParameter("@Login", System.Data.SqlDbType.NVarChar);
            SqlParameter Param2 = new SqlParameter("@Pass", System.Data.SqlDbType.NVarChar);
            SqlParameter Param3 = new SqlParameter("@FirstName", System.Data.SqlDbType.NVarChar);
            SqlParameter Param4 = new SqlParameter("@LastName", System.Data.SqlDbType.NVarChar);
            SqlParameter Param5 = new SqlParameter("@Sex", System.Data.SqlDbType.NVarChar);
            SqlParameter Param6 = new SqlParameter("@Coutry", System.Data.SqlDbType.NVarChar);
            SqlParameter Param7 = new SqlParameter("@City", System.Data.SqlDbType.NVarChar);
            SqlParameter Param8 = new SqlParameter("@PathImg", System.Data.SqlDbType.NVarChar);

            Param1.Value = user.Login;
            Param2.Value = "";
            Param3.Value = user.FirstName;
            Param4.Value = user.LastName;
            Param5.Value = user.Sex;
            Param6.Value = user.Country;
            Param7.Value = user.City;
            Param8.Value = user.PathImg;

            sqlComm.Parameters.Add(Param1);
            sqlComm.Parameters.Add(Param2);
            sqlComm.Parameters.Add(Param3);
            sqlComm.Parameters.Add(Param4);
            sqlComm.Parameters.Add(Param5);
            sqlComm.Parameters.Add(Param6);
            sqlComm.Parameters.Add(Param7);
            sqlComm.Parameters.Add(Param8);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();

            while (reader.Read())
            {
                res = reader[0].ToString();
            }

            _con.Close();
            return res;
        }

        public bool IsLoginInDB(string login)
        {
            bool res = false;

            SqlCommand sqlComm = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Login = @Login", _con);
            SqlParameter Param1 = new SqlParameter("@Login", System.Data.SqlDbType.NVarChar);
            Param1.Value = login;
            sqlComm.Parameters.Add(Param1);

            _con.Open();
            SqlDataReader reader = sqlComm.ExecuteReader();

            while (reader.Read())
            {
                if (int.Parse(reader[0].ToString()) == 1)
                { res = true; }
            }

            _con.Close();
            return res;
        }
    }
}
