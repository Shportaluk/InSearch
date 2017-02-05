using InSearch.Entities;
using InSearch.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using InSearch.Filters;
using InSearch.Models;


namespace InSearch.Controllers
{
    [Culture]
    public class InSearchController : Controller
    {
        IndexContext ic = new IndexContext();
        #region init
        private SqlRepository _sqlRepository = new SqlRepository();
        private string _secretKey = "6Lc-HAoUAAAAAHOG9VBTB8ubQ95kkMBlRWBoogd-";
        private WebClient _client = new WebClient();
        
        // VK
        private string _VkClientId = "5695105";
        private string _VkClientSecret = "UweMZtmwob884I0rbAcM";
        private string _VkRedirectURI = "http://localhost:2798/vkAuth";

        // FB
        private string _FbClientId = "973610479448616";
        private string _FbSecret = "cc83f8563100f084e927e2125dde60f5";
        private string _FbRedirectURI = "http://localhost:2798/fbAuth";

        // OK
        private string _OkClientId = "1248646656";
        private string _OkPublic = "CBAHNNGLEBABABABA";
        private string _OkSecret = "ADAF6EBA820B8B79D5A39EF2";
        private string _OkRedirectURI = "http://localhost:2798/okAuth";

        // Yandex
        private string _YandexId = "910be0e9678342da8d16ba8233535ad5";
        private string _YandexSecret = "6359090cdcf04566ae64bed6b6902450";

        // Google
        private string _GoogleId = "1095993003643-fj276hvktnvpmhs9fvo35m6ebpl511oc.apps.googleusercontent.com";
        private string _GoogleSecret = "OhfsXA3QIjvAgqW0Y32f5R0w";
        private string _GoogleRedirectURI = "http://localhost:2798/googleAuth";
        #endregion
        #region Auth with other services
        [HttpGet]
        public ActionResult vkAuth()
        {
            string code = HttpContext.Request.Url.Query.Substring(6);
            string path = String.Format("https://oauth.vk.com/access_token?client_id={0}&client_secret={1}&redirect_uri={2}&code={3}",
                _VkClientId, _VkClientSecret, _VkRedirectURI, code);
            string json = _client.DownloadString(path);

            string token = JObject.Parse(json).SelectToken("access_token").ToString();
            string userId = JObject.Parse(json).SelectToken("user_id").ToString();
            string login = userId + "_VK_";

            return LoginOrRegistrationWithOtherService(login, "_VK_", userId, token);
        }
        public ActionResult fbAuth()
        {
            string code = HttpContext.Request.Url.Query.Substring(6);
            string path = "https://graph.facebook.com/v2.8/oauth/access_token?client_id=" + _FbClientId +
                "&redirect_uri=" + _FbRedirectURI +
                "&client_secret=" + _FbSecret +
                "&code=" + code;
            string json = _client.DownloadString(path);
            string token = JObject.Parse(json).SelectToken("access_token").ToString();

            path = "https://graph.facebook.com/me?fields=id&access_token=" + token;
            json = _client.DownloadString(path);
            string login = JObject.Parse(json).SelectToken("id").ToString() + "_FB_";

            return LoginOrRegistrationWithOtherService(login, "_FB_", null, token);
        }
        public ActionResult okAuth()
        {
            string code = HttpContext.Request.Url.Query.Substring(6);
            string path = String.Format("https://api.ok.ru/oauth/token.do?code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code",
                code, _OkClientId, _OkSecret, _OkRedirectURI );

            HttpWebRequest request = WebRequest.Create(path) as HttpWebRequest;
            request.Method = "POST";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            string json = null;
            using (Stream rspStm = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(rspStm))
                {
                    json = reader.ReadToEnd();
                }
            }

            string token = JObject.Parse(json).SelectToken("access_token").ToString();

            string secretKeyMd5 = CalculateMD5Hash(token + _OkSecret).ToLower();
            string appKey = "application_key=" + _OkPublic + "fields=uidmethod=users.getCurrentUser" + secretKeyMd5;
            string sig = CalculateMD5Hash(appKey).ToLower();
            path = "https://api.ok.ru/fb.do?application_key=" + _OkPublic +
                "&fields=uid&method=users.getCurrentUser&sig=" + sig + "&access_token=" + token;

            json = _client.DownloadString(path);
            string login = JObject.Parse(json).SelectToken("uid").ToString() + "_OK_";

            return LoginOrRegistrationWithOtherService(login, "_OK_", null, token, secretKeyMd5);
        }
        public ActionResult yandexAuth()
        {
            string code = HttpContext.Request.Url.Query.Substring(6);
            string url = "https://oauth.yandex.ru/token";

            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["grant_type"] = "authorization_code";
                values["code"] = code;
                values["client_id"] = _YandexId;
                values["client_secret"] = _YandexSecret;

                var response = client.UploadValues(url, values);

                var json = Encoding.Default.GetString(response);
                string token = JObject.Parse(json).SelectToken("access_token").ToString();
                
                json = _client.DownloadString("https://login.yandex.ru/info?format=json&oauth_token=" + token );
                string login = JObject.Parse(json).SelectToken("id").ToString() + "_YANDEX_";


                return LoginOrRegistrationWithOtherService( login, "_YANDEX_", null, token, null );
            }
        }
        public ActionResult googleAuth()
        {
            string code = HttpContext.Request.Url.Query.Substring(6);
            string url = "https://accounts.google.com/o/oauth2/token";
            
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                
                values["code"] = code;
                values["client_id"] = _GoogleId;
                values["client_secret"] = _GoogleSecret;
                values["redirect_uri"] =_GoogleRedirectURI;
                values["grant_type"] = "authorization_code";

                var response = client.UploadValues(url, values);

                var json = Encoding.Default.GetString(response);
                string token = JObject.Parse(json).SelectToken("access_token").ToString();

                json = _client.DownloadString("https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + token);
                string login = JObject.Parse(json).SelectToken("id").ToString() + "_GOOGLE_";

                return LoginOrRegistrationWithOtherService(login, "_GOOGLE_", null, token, null);
            }
        }
        private ActionResult LoginOrRegistrationWithOtherService( string login, string nameService, string userId, string token, string secretKeyMd5 = null)
        {
            if (!_sqlRepository.IsLoginInDB(login))
            {
                User user = null;
                switch (nameService)
                {
                    case "_VK_":
                        user = GetProfileOfUserFromVk(userId, token);
                        break;
                    case "_FB_":
                        user = GetProfileOfUserFromFb(token);
                        break;
                    case "_OK_":
                        user = GetProfileOfUserFromOk(token, secretKeyMd5);
                        break;
                    case "_YANDEX_":
                        user = GetProfileOfUserFromYandex(token);
                        break;
                    case "_GOOGLE_":
                        user = GetProfileOfUserFromGoogle(token);
                        break;
                }
                if (_sqlRepository.Registration(user) != "OK")
                {
                    return RedirectToAction("index");
                }
            }
            return Login(login, null, false, false);
        }
        #endregion

        public ActionResult Index()
        {
            return View(ic);
        }
        [Authorize(Roles = "User,Administrator")]
        public ActionResult peopleId( string id )
        {
            ViewBag.IsMyProfile = false;
            string myName = HttpContext.User.Identity.Name;
            if (id != null)
            {
                ViewBag.User = _sqlRepository.GetUserByLogin(id);
                if (myName == id)
                {
                    ViewBag.IsMyProfile = true;
                }
            }
            else
            {
                ViewBag.User = _sqlRepository.GetUserByLogin(HttpContext.User.Identity.Name);
                ViewBag.IsMyProfile = true;
            }
            if(ViewBag.User == null)
            {
                return RedirectToAction("NoUser");
            }
            return View();
        }
        [Authorize(Roles = "User,Administrator")]
        public ActionResult search()
        {
            return View();
        }
        [Authorize(Roles = "User,Administrator")]
        public ActionResult messages()
        {
            var login = HttpContext.User.Identity.Name;
            return View();
        }
        [Authorize(Roles = "User,Administrator")]
        public ActionResult friends()
        {
            return View();
        }
        [Authorize(Roles = "User,Administrator")]
        public ActionResult question( string id )
        {
            if (id != null)
            {
                var s = _sqlRepository.GetQuestionByName(id);
                ViewBag.Questions = s;
                if (ViewBag.Questions == null)
                {
                    return RedirectToAction("NoQuestion");
                }
            }
            return View();
        }



        // Admin
        public ActionResult LoginToAdminPanelGoNOW()
        {
            return View();
        }
        public ActionResult LoginToAdmin( string login, string pass )
        {
            return Login(login, pass, false, true, true);
        }
        [Authorize(Roles = "Administrator")]
        public ActionResult AdminPanelADMIN()
        {
            return View();
        }
        [Authorize(Roles = "Administrator")]
        public ActionResult NewQuestion()
        {
            string login = HttpContext.User.Identity.Name;
            _sqlRepository.NewQuestions( login, "Test", "Have you car?:yes,no|Have you bus?:yes,no|Do you have a bus?:yes,no,maybe" );
            return RedirectToAction("AdminPanelADMIN");
        }

        public string CompleteQuestion(string nameQuestion, string arrQuest)
        {
            int[] ansEtraverYes = { 1, 3, 8, 10, 13, 17, 22, 25, 27, 37, 39, 44, 46, 49, 53, 56 };
            int[] ansEtraverNo = { 5, 15, 20, 29, 32, 34, 41, 51 };
            int[] ansEmotionStabilityYes = { 2, 4, 7, 9, 11, 14, 16, 19, 21, 23, 26, 28, 31, 33, 35, 38, 40, 43, 45, 47, 50, 52, 55, 57 };

            string[] lines = arrQuest.Split( '|' ).Where(n => !string.IsNullOrEmpty(n)).ToArray();
            string ans;
            string temperament;
            int levelExtraver = 0;
            int levelEmotionStability = 0;

            for ( int i = 0; i < lines.Length; i++ )
            {
                ans = lines[i].Split(':')[1];
                if (( IsInArray( i, ansEtraverYes ) && ans == "Так" ) ||
                   (  IsInArray( i, ansEtraverNo )  && ans == "Ні"    ))
                {
                    levelExtraver++;
                }
                else if ( IsInArray( i, ansEmotionStabilityYes) && ans == "Так" )
                {
                    levelEmotionStability++;
                }

            }

            if( levelExtraver < 12 )
            {
                if ( levelEmotionStability < 12 )
                {
                    temperament = "Flagmatic";
                }
                else
                {
                    temperament = "Melanholic";
                }
            }
            else
            {
                if (levelEmotionStability < 12)
                {
                    temperament = "Sangvinic";
                }
                else
                {
                    temperament = "Holeric";
                }
            }

            return temperament;
            //return _sqlRepository.CompleteQuestion( HttpContext.User.Identity.Name, nameQuestion, arrQuest );
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult logout()
        {
            var httpCookie = HttpContext.Response.Cookies[".ASPXAUTH"];
            if (httpCookie != null)
            {
                httpCookie.Value = string.Empty;
            }
            if (HttpContext.User.Identity.Name != null)
            {
                _sqlRepository.LogOut(HttpContext.User.Identity.Name);
            }
            return RedirectToAction("index");
        }
        [HttpPost]
        public ActionResult Login(string login, string pass, bool checkCaptcha = true, bool checkPass = true, bool isAdmin = false)
        {
            ViewBag.IsShowFormLogin = true;
            if (checkCaptcha)
            {
                var response = Request["g-recaptcha-response"];
                var res = _client.DownloadString(String.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", _secretKey, response));
                if (!(bool)JObject.Parse(res).SelectToken("success"))
                {
                    ViewBag.ErrorLogin = "Error Captcha";
                    return View("Index");
                }
            }
            #region LogIn With/Without Pass
            string resSqlRep;
            if (checkPass)
            {
                if (isAdmin)
                {
                    resSqlRep = _sqlRepository.LogIn(login, CalculateMD5Hash( CalculateMD5Hash(pass) ) );
                }
                else
                {
                    resSqlRep = _sqlRepository.LogIn(login, CalculateMD5Hash(pass));
                }
            }
            else
            {
                resSqlRep = _sqlRepository.LogInWithoutPass(login);
            }
            #endregion
            if (resSqlRep == "OK")
            {
                var roles = _sqlRepository.GetRolesOfUser(login);
                var ticket = new FormsAuthenticationTicket(2, login, DateTime.Now, DateTime.Now.AddMinutes(30), true, roles);
                var encTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);

                Response.Cookies.Add(cookie);

                if( isAdmin )  { return RedirectToAction("AdminPanelADMIN");  } 
                return RedirectToAction("peopleId");
            }
            else
            {
                ViewBag.ErrorLogin = resSqlRep;
            }
            return View("Index");
        }
        [HttpPost]
        public ActionResult Registration(string login, string pass, string firstName, string lastName, string sex)
        {
            ViewBag.ErrorRegistration = null;
            #region Check
            #region login length
            if ( login.Length < 6 )
            {
                ViewBag.ErrorRegistration = "Login min length is 6";
                goto returnView;
            }
            #endregion
            #region pass length
            else if ( pass.Length < 8 )
            {
                ViewBag.ErrorRegistration = "Pass min length is 8";
                goto returnView;
            }
            #endregion
            #region pass for numers
            else if ( !CheckStringForNumber(pass) )
            {
                ViewBag.ErrorRegistration = "Pass must contain numbers";
                goto returnView;
            }
            #endregion
            #region firstName || lastName length
            else if ( firstName.Length < 3 || lastName.Length < 3 )
            {
                ViewBag.ErrorRegistration = "First/Last name, min length is 3";
                goto returnView;
            }
            #endregion
            #region Male || Famale
            if (sex == "Male" || sex == "Famale")
            {
                ViewBag.ErrorRegistration = "Error. Incorrect data";
                goto returnView;
            }
            #endregion
            #endregion

            var res = _sqlRepository.Registration(login, CalculateMD5Hash(pass), firstName, lastName, sex);
            if (res == "OK")
            {
                return Login(login, pass, false);
            }
            else
            {
                ViewBag.ErrorRegistration = res;
                goto returnView;
            }

        returnView:
            ViewBag.Login = login;
            ViewBag.Pass = pass;
            ViewBag.FirstName = firstName;
            ViewBag.LastName = lastName;
            ViewBag.Sex = sex;

            return View("Index");
        }

        private bool CheckStringForNumber(string str)
        {
            foreach (char c in str)
            {
                if( Char.IsDigit(c) ) { return true; }
            }
            return false;
        }
        private string CalculateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();

        }
        private User GetProfileOfUserFromVk(string userId, string token)
        {
            User user = null;
            string path = String.Format("https://api.vk.com/method/users.get?user_id={0}&access_token={1}&fields=sex,country,city,photo_max_orig&v=5.59", userId, token);
            try
            {
                _client.Encoding = Encoding.UTF8;
                string json = _client.DownloadString(path);
                user = new User();
                user.Id = userId;
                var response = JObject.Parse(json).SelectToken("response")[0];
                user.FirstName = response.SelectToken("first_name").ToString();
                user.LastName = response.SelectToken("last_name").ToString();
                user.Sex = response.SelectToken("sex").ToString();
                user.Country = response.SelectToken("country").SelectToken("title").ToString();
                user.City = response.SelectToken("city").SelectToken("title").ToString();
                user.Login = userId + "_VK_";
                
                string urlImg = response.SelectToken("photo_max_orig").ToString();
                user.PathImg = UploadImgToServer(urlImg);
            }
            catch { }
            return user;
        }
        private User GetProfileOfUserFromFb(string token)
        {
            User user = null;
            string path = "https://graph.facebook.com/me?fields=id,first_name,last_name,gender,birthday,location&access_token=" + token;
            try
            {
                _client.Encoding = Encoding.UTF8;
                string json = _client.DownloadString(path);
                var jObj = JObject.Parse(json);
                user = new User();

                user.Id = jObj.SelectToken("id").ToString();
                user.FirstName = jObj.SelectToken("first_name").ToString();
                user.LastName = jObj.SelectToken("last_name").ToString();
                user.Sex = jObj.SelectToken("gender").ToString();
                user.Country = jObj.SelectToken("location").SelectToken("name").ToString();
                user.Login = user.Id + "_FB_";
                
                string urlImg = "https://graph.facebook.com/" + user.Id + "/picture?width=1000&height=1000";
                user.PathImg = UploadImgToServer(urlImg);
            }
            catch { }
            return user;
        }
        private User GetProfileOfUserFromOk(string token, string secretKeyMd5)
        {
            User user = null;

            string appKey = "application_key=" + _OkPublic + "fields=uid,gender,location,first_name,last_name,pic1024x768method=users.getCurrentUser" + secretKeyMd5;
            string sig = CalculateMD5Hash(appKey).ToLower();
            string path = "https://api.ok.ru/fb.do?application_key=" + _OkPublic +
                "&fields=uid%2Cgender%2Clocation%2Cfirst_name%2Clast_name%2Cpic1024x768" +
                "&method=users.getCurrentUser" +
                "&sig=" + sig +
                "&access_token=" + token;
            try
            {
                _client.Encoding = Encoding.UTF8;
                string json = _client.DownloadString(path);
                var jObj = JObject.Parse(json);
                user = new User();

                user.Id = jObj.SelectToken("uid").ToString();
                user.FirstName = jObj.SelectToken("first_name").ToString();
                user.LastName = jObj.SelectToken("last_name").ToString();
                user.Sex = jObj.SelectToken("gender").ToString();
                user.Country = jObj.SelectToken("location").SelectToken("countryName").ToString();
                user.City = jObj.SelectToken("location").SelectToken("city").ToString();
                user.Login = user.Id + "_OK_";
                
                string urlImg = jObj.SelectToken("pic1024x768").ToString();
                user.PathImg = UploadImgToServer(urlImg);
            }
            catch { }
            return user;
        }
        private User GetProfileOfUserFromYandex(string token)
        {
            User user = null;
            string path = "https://login.yandex.ru/info?format=json&oauth_token=" + token;
            try
            {
                _client.Encoding = Encoding.UTF8;
                string json = _client.DownloadString(path);
                var jObj = JObject.Parse(json);
                user = new User();

                user.Id = jObj.SelectToken("id").ToString();
                user.FirstName = jObj.SelectToken("first_name").ToString();
                user.LastName = jObj.SelectToken("last_name").ToString();
                user.Sex = jObj.SelectToken("sex").ToString();
                user.Login = user.Id + "_YANDEX_";


                string urlImg = "https://avatars.mds.yandex.net/get-yapic/" + jObj.SelectToken("default_avatar_id").ToString() + "/big";
                user.PathImg = UploadImgToServer(urlImg);
            }
            catch { }
            return user;
        }
        private User GetProfileOfUserFromGoogle(string token)
        {
            User user = null;
            string path = "https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + token;
            try
            {
                _client.Encoding = Encoding.UTF8;
                string json = _client.DownloadString(path);
                var jObj = JObject.Parse(json);
                user = new User();

                user.Id = jObj.SelectToken("id").ToString();
                user.FirstName = jObj.SelectToken("given_name").ToString();
                user.LastName = jObj.SelectToken("family_name").ToString();
                user.Sex = jObj.SelectToken("locale").ToString();
                user.Login = user.Id + "_GOOGLE_";
                
                string urlImg = jObj.SelectToken("picture").ToString();
                user.PathImg = UploadImgToServer(urlImg);
                
            }
            catch { }
            return user;
        }
        private string UploadImgToServer( string url )
        {
            try
            {
                string fileName = CalculateMD5Hash(DateTime.Now.ToString()) + ".jpg";
                string pathImg = Path.Combine(HttpContext.Server.MapPath("~/ServerImages/" + fileName));
                string pathImgMini = Path.Combine(HttpContext.Server.MapPath("~/ServerImages/mini_" + fileName));
                _client.DownloadFile(url, pathImg);

                var imageMini = new Bitmap(new Bitmap(pathImg), new Size(64, 64));
                imageMini.Save(pathImgMini, System.Drawing.Imaging.ImageFormat.Jpeg);

                return fileName;
            }
            catch { return ""; }
        }

        private bool IsInArray(int n, int[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == n)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
