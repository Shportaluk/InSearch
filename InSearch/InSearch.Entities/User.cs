using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSearch.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex
        {
            get
            {
                return _sex;
            }
            set
            {
                switch (value.ToLower())
                {
                    // Мужской
                    case "famale":
                    case "female":
                    case "1":
                        _pathImg = "default/default_girl.jpg";
                        _sex = "1";
                        break;
                    case "male":
                    case "2":
                        _pathImg = "default/default_man.jpg";
                        _sex = "2";
                        break;
                    default:
                        _pathImg = "default/default_no_sex.jpg";
                        _sex = "0";
                        break;
                }
            }
        }
        public string Country { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string PathImg
        {
            get
            {
                return _pathImg;
            }
            set
            {
                _pathImg = value;
            }
        }
        public string MiniPathImg { get; set; }
        public string Roles { get; set; }
        public bool IsOnline { get; set; }

        

        private string _pathImg;
        private string _sex;

        public User()
        {
            Login = "";
            Name = "";
            FirstName = "";
            LastName = "";
            Sex = "0";
            Country = "";
            City = "";
            Phone = "";
            PathImg = "default/default_no_sex.jpg";
            MiniPathImg = "";
            Roles = "";
        }
    }
}
