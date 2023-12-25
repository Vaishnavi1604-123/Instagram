using Instagram_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class HomeController : Controller
{
    private readonly Uri _baseAddress;
    private readonly HttpClient _httpClient;

    public HomeController()
    {
        _baseAddress = new Uri("http://localhost:5203/api/Instagram/");
        _httpClient = new HttpClient { BaseAddress = _baseAddress };
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(Register register)
    {
        try
        {
            using (var stream = new MemoryStream())
            {
                register.ImageFile.CopyTo(stream);
                register.ImageData = stream.ToArray();
            }

            RegisterModel registerModel = new RegisterModel
            {
                Email = register.Email,
                Password = register.Password,
                ImageData = register.ImageData,
                UserName = register.UserName
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("Register", registerModel);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (loginResponse.Success)
                {
                    ViewBag.SuccessMessage = "You have registered successfully. Please log in.";
                    ModelState.Clear();
                }
                else
                {
                    ViewBag.ErrorMessage = "User with same credentials already exists";
                    ModelState.Clear();
                }
            }

            return View("Register");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during registration attempt: {ex.Message}");
            ModelState.AddModelError(string.Empty, "Error during registration attempt.");


            return View("Register");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(Login model)
    {
        if (ModelState.IsValid)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("Login", model);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (loginResponse.Success)
                    {
                        return RedirectToAction("MainPage", model);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid credentials";
                        ModelState.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing response content: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Error during login attempt.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error during login attempt.");

            }
        }

        return View(model);
    }

    public async Task<IActionResult> Mainpage(Login login)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"Getuserdetails/data/{login.Email}");
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();

            // Instead of deserializing into a List<LoginModel>, deserialize into a single LoginModel
            LoginModel item = JsonConvert.DeserializeObject<LoginModel>(content);
            HttpContext.Session.SetInt32("UserId", item.Id);
            HttpContext.Session.SetString("UserName", item.UserName);
            HttpContext.Session.Set("ImageData", item.Imagedata);

            // Create a list and add the single object to it
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.ImageData = HttpContext.Session.Get("ImageData") as byte[];

            return RedirectToAction("TimeLine");
        }
        return View();
    }
    [HttpGet]
    public async Task<IActionResult> Post(Login login)
    {
        return View("Post");
    }
    [HttpPost]
    public async Task<IActionResult> Post(PostModel postModel)
    {
        int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

        using (var stream = new MemoryStream())
        {
            postModel.ImageFile.CopyTo(stream);
            postModel.ImageData = stream.ToArray();
        }
        PostModelll postModelll = new PostModelll();
        postModelll.userId = userId;
        postModelll.Text = postModel.Text;
        postModelll.ImageData = postModel.ImageData;

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("SaveDataToDB/SaveTheDataToDataBase", postModelll);
        if (response.IsSuccessStatusCode)
        {
            ViewBag.Success = "You Have Posted Successfully";
        }
        else
        {
            ViewBag.Failure = "An Error Occured While Posting";
        }

        return View("Post");
    }

    public IActionResult Logout()
    {
        return View("Login");
    }
    [HttpGet]
    public async Task<IActionResult> TimeLine()
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"GetPost/getposts");
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();

            List<ViewPostsModel> item = JsonConvert.DeserializeObject<List<ViewPostsModel>>(content);
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.ImageData = HttpContext.Session.Get("ImageData") as byte[];

            return View("TimeLine", item);
        }
        return View();
    }
    public async Task<IActionResult> ViewMyPost()
    {
        int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        HttpResponseMessage response = await _httpClient.GetAsync($"MyPost/GetMyPost/{userId}");
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            List<ViewPostsModel> item1 = JsonConvert.DeserializeObject<List<ViewPostsModel>>(content);
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.ImageData = HttpContext.Session.Get("ImageData") as byte[];

            return View("ViewMyPost", item1);
        }
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> LikePost(int Id)
    {
        int updatedLikesCount1=0;
        int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        int postId = Id;
        LikeModel model = new LikeModel();
        model.userId = userId;
        model.postId = postId;
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("CheckLike/CheckLikes",model);
        if(response.IsSuccessStatusCode)
        {
            bool likeexists = await response.Content.ReadFromJsonAsync<bool>();
            if (likeexists)
            {
                HttpResponseMessage response1 = await _httpClient.PostAsJsonAsync("GetLike/GetLikes", model);
                if (response1.IsSuccessStatusCode)
                {
                    string content = await response1.Content.ReadAsStringAsync();
                    if (int.TryParse(content, out int updatedLikesCount))
                    {
                        updatedLikesCount1 = updatedLikesCount;
                    }
                }
            }
            else
            {
                HttpResponseMessage response1 = await _httpClient.PostAsJsonAsync("AddLike/AddLikes", model);
                if (response1.IsSuccessStatusCode)
                {
                    string content = await response1.Content.ReadAsStringAsync();
                    if (int.TryParse(content, out int updatedLikesCount))
                    {
                        updatedLikesCount1 = updatedLikesCount;
                    }
                }

            }
        }
        return Json(new { likes = updatedLikesCount1 });

    }
}
