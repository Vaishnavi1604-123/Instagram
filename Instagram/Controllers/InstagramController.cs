using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Instagram_MVC.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Instagram.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class InstagramController : Controller
    {
        private readonly string _configuration;
        public InstagramController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("connectionstring");
        }
        [HttpPost]
        public IActionResult Register(RegisterModel register)
        {
            if (UserExists(register.UserName, register.Email))
            {
                return base.Json(new LoginResponse { Success = false });
                //return Conflict("User with the same username or email already exists.");
            }
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("LogRegisterDetails", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", register.UserName);
                    command.Parameters.AddWithValue("@Email", register.Email);
                    command.Parameters.AddWithValue("@Password", register.Password);
                    command.Parameters.AddWithValue("@ImageData", register.ImageData);
                    int result = command.ExecuteNonQuery();
                }
            }
            return base.Json(new LoginResponse { Success = true });
        }
        private bool UserExists(string userName, string email)
        {
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM Register WHERE UserName = @UserName and Email = @Email";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@Email", email);

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }
        [HttpPost]
        public IActionResult Login(Login model)
        {
            if (IsValidUser(model))
            {
                return base.Json(new LoginResponse { Success = true });
            }

            return base.Json(new LoginResponse { Success = false });
        }

        private bool IsValidUser(Login model)
        {
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT COUNT(*) FROM Register WHERE Email=@Email AND Password = @Password", connection))
                {
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.Parameters.AddWithValue("@Password", model.Password);

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }

        }
        [HttpGet]
        [Route("data/{Email}")]
        public ActionResult<LoginModel> Getuserdetails(string Email)
        {
            LoginModel loginModel = new LoginModel();
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("getuserdetails", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", Email);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        loginModel.Id = (int)reader["Id"];
                        loginModel.Email = reader.GetString("Email");
                        loginModel.Password = reader.GetString("Password");
                        loginModel.UserName = reader.GetString("UserName");
                        loginModel.Imagedata = (byte[])reader["ImageData"];
                    }

                }
            }
            return Ok(loginModel);
        }
        [HttpPost]
        [Route("SaveTheDataToDataBase")]
        public ActionResult<LoginModel> SaveDataToDB(PostModelll postModelll)
        {
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();
                using (var command = new SqlCommand("savePostToDb", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userid", postModelll.userId);
                    command.Parameters.AddWithValue("@Text", postModelll.Text);
                    command.Parameters.AddWithValue("@ImageData", postModelll.ImageData);
                    
                    int result = command.ExecuteNonQuery();
                }
                return base.Json(new LoginResponse { Success = true });
            }

        }
        [Route("getposts")]
        public ActionResult<IEnumerable<ViewPostsModel>> GetPost()
        {
            List< ViewPostsModel>  posts = new List< ViewPostsModel>();
            //ViewPostsModel viewPostsModel = new ViewPostsModel();
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();
                using (var command = new SqlCommand("GetPosts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataReader reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        ViewPostsModel viewPostsModel = new ViewPostsModel();
                        viewPostsModel.id =(int) reader["postId"];
                        viewPostsModel.Username = reader["username"].ToString();
                        viewPostsModel.ImageData = (byte[])reader["postImage"];
                        viewPostsModel.DateTime = reader.GetDateTime(reader.GetOrdinal("DateTime"));
                        viewPostsModel.Profilepic= (byte[])reader["Profilepic"];
                        viewPostsModel.Text = reader["Text"].ToString();
                        viewPostsModel.Likes = (int)reader["LikeCount"];
                        posts.Add(viewPostsModel);
                    }
                }
                return Ok(posts);
            }
        }
        [HttpGet]
        [Route("GetMyPost/{userId}")]
        public ActionResult<IEnumerable<ViewPostsModel>> MyPost(int userId)
        {
            List<ViewPostsModel> viewPostsModels = new List<ViewPostsModel>();
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("GetPostsByUserId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ViewPostsModel viewPostsModel = new ViewPostsModel
                            {
                                id = (int)reader["postId"],
                                Username = reader["UserName"].ToString(),
                                ImageData = reader["postImage"] == DBNull.Value ? null : (byte[])reader["postImage"],
                                DateTime = reader.GetDateTime(reader.GetOrdinal("DateTime")),
                                Profilepic = reader["Profilepic"] == DBNull.Value ? null : (byte[])reader["Profilepic"],
                                Text = reader["Text"].ToString(),
                                Likes = (int)reader["LikesCount"]
                            };

                            viewPostsModels.Add(viewPostsModel);
                        }
                    }
                }
            }

            return Ok(viewPostsModels);
        }

        [HttpPost]
        [Route("CheckLikes")]
        public ActionResult<bool> CheckLike([FromBody] LikeModel model)
        {
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("GetLikedUsers", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@postId", model.postId);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if ((int)reader["UserId"] == model.userId)
                    {
                        return Ok(true);
                    }
                }
            }
            return Ok(false);

        }
        [HttpPost]
        [Route("AddLikes")]
        public int AddLike([FromBody] LikeModel model)
        {
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("AddLike", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", model.userId);
                cmd.Parameters.AddWithValue("@PostId", model.postId);

                // Add the output parameter for the updated like count
                SqlParameter likeCountParam = new SqlParameter("@likeCount", SqlDbType.Int);
                likeCountParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(likeCountParam);


                cmd.ExecuteNonQuery();

                // Retrieve the updated like count
                int updatedLikeCount = Convert.ToInt32(cmd.Parameters["@likeCount"].Value);

                // Return the updated like count to the client
                return updatedLikeCount;

            }
            

        }
        [HttpPost]
        [Route("GetLikes")]
        public int GetLike([FromBody] LikeModel model)
        {
            using (SqlConnection connection = new SqlConnection(_configuration))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("GetLikes", connection);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@postId", model.postId);

                int LikeCount = (int)cmd.ExecuteScalar();

                return LikeCount;

            }
            

        }


    }

}

