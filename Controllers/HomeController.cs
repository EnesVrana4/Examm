using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace TestFinal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context;

    // here we can "inject" our context service into the constructor
    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }
    public IActionResult Index()

    {
         // Pjesa me request

        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }
        int id = (int)HttpContext.Session.GetInt32("userId");

       

        //Marrim gjithe perdoruesit e tjere
        List<Request> rq =  _context.Requests.Include(e=>e.Reciver).Include(e=>e.Sender).Where(e => e.ReciverId == id).Where(e => e.Accepted == false).ToList();

        ViewBag.perdoruesit2 = _context.Users.Include(e=>e.Requests).Where(e=> e.UserId != id).Where(e=>(e.Requests.Any(f=> f.SenderId == id) == false) && (e.Requests.Any(f=> f.ReciverId == id) == false) ).ToList();
        
        //shfaqim gjith requests
        ViewBag.requests = _context.Requests.Include(e=>e.Reciver).Include(e=>e.Sender).Where(e => e.ReciverId == id).Where(e => e.Accepted == false).ToList();

        // shfaq gjith miqte
        var miqte = _context.Requests.Where(e => (e.SenderId == id) || (e.ReciverId == id)).Include(e=>e.Reciver).Include(e=>e.Sender).Where(e=>e.Accepted ==true).ToList();
        ViewBag.miqte = _context.Requests.Where(e => (e.SenderId == id) || (e.ReciverId == id)).Include(e=>e.Reciver).Include(e=>e.Sender).Where(e=>e.Accepted ==true).ToList();
        
        //Marr te loguarin me te dhena
        ViewBag.iLoguari = _context.Users.FirstOrDefault(e => e.UserId == id);

        // Perdorues qe nuk i kemi shoke dhe as u kemi derguar e as na kane derguar requests
        var perdoruesit = _context.Users.Where(x => x.UserId != id).ToList();
        var requestes = _context.Requests.Include(e=>e.Reciver).Include(e=>e.Sender).Where(e => e.Accepted == false).ToList();
        List<User> ataQeNukKemiKontakte = new List<User>();
        foreach (var perdorues in perdoruesit)
        {
            if (!miqte.Any(x => x.SenderId == perdorues.UserId || x.ReciverId == perdorues.UserId) 
            && !requestes.Any(x => x.SenderId == perdorues.UserId || x.ReciverId == perdorues.UserId))
                ataQeNukKemiKontakte.Add(perdorues);
        }

        ViewBag.perdoruesit = ataQeNukKemiKontakte;
        


       
       
        //Perfshi dhe return
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {


        if (HttpContext.Session.GetInt32("userId") == null)
        {

            return View();
        }

        return RedirectToAction("Index");

    }
    [HttpPost("Register")]
    public IActionResult Register(User user)
    {
        // Check initial ModelState
        if (ModelState.IsValid)
        {
            // If a User exists with provided email
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                // Manually add a ModelState error to the Email field, with provided
                // error message
                ModelState.AddModelError("Email", "Email already in use!");

                return View();
                // You may consider returning to the View at this point
            }
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            user.Password = Hasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("userId", user.UserId);
           
            return RedirectToAction("Index");
        }
        return View();
    }

    [HttpPost("Login")]
    public IActionResult LoginSubmit(LoginUser userSubmission)
    {
        if (ModelState.IsValid)
        {
            // If initial ModelState is valid, query for a user with provided email
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
            // If no user exists with provided email
            if (userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("User", "Invalid UserName/Password");
                return View("Register");
            }

            // Initialize hasher object
            var hasher = new PasswordHasher<LoginUser>();

            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

            // result can be compared to 0 for failure
            if (result == 0)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return View("Register");
                // handle failure (this should be similar to how "existing email" is handled)
            }
            HttpContext.Session.SetInt32("userId", userInDb.UserId);

            return RedirectToAction("Index");
        }

        return View("Register");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {

        HttpContext.Session.Clear();
        return RedirectToAction("register");
    }




  
    
    // Pjesa me request


    [HttpGet("SendR/{id}")]
    public IActionResult SendR(int id)
    {
        int idFromSession = (int)HttpContext.Session.GetInt32("userId");
        Request newRequest = new Models.Request()
        {
            SenderId = idFromSession,
            ReciverId = id,
          
        };
        _context.Requests.Add(newRequest);
        _context.SaveChanges();
        // User dbUser = _context.Users.Include(e=>e.Requests).First(e=> e.UserId == idFromSession);
        // dbUser.Requests.Add(newRequest);
        _context.SaveChanges();
        return RedirectToAction("index");

    }
    [HttpGet("AcceptR/{id}")]
    public IActionResult AcceptR(int id)
    {
        
        Request requestii = _context.Requests.First(e => e.RequestId == id);
        requestii.Accepted=true;
        // _context.Remove(hiqFans);
        _context.SaveChanges();
        return RedirectToAction("index");
    }
     [HttpGet("DeclineR/{id}")]
    public IActionResult Decline(int id)
    {
        
        Request requestii = _context.Requests.First(e => e.RequestId == id);
         _context.Remove(requestii);
        _context.SaveChanges();
        return RedirectToAction("index");
    }
    
    [HttpGet("Profile/{id}")]
    public IActionResult Profile(int id)
    {
        var userProfile = _context.Users.Where(x => x.UserId == id).FirstOrDefault();

        if (userProfile == null)
            return NotFound();
        
        ViewBag.Name = userProfile.Name;
        ViewBag.About = userProfile.About;
        return View();
    }

    [HttpGet("MyProfile/{id}")]
    public IActionResult MyProfile(int id)
    {
        var userProfile = _context.Users.Where(x => x.UserId == id).FirstOrDefault();

        if (userProfile == null)
            return NotFound();
        
        ViewBag.Name = userProfile.Name;
        ViewBag.About = userProfile.About;
        
        //shfaqim gjith requests
        ViewBag.Requests = _context.Requests.Include(e=>e.Reciver).Include(e=>e.Sender).Where(e => e.ReciverId == id).Where(e => e.Accepted == false).ToList();

        // shfaq gjith miqte
        var miqQeKemiShtuar = _context
            .Requests
            .Where(e => (e.SenderId == id))
            .Include(e=>e.Reciver)
            .Include(e=>e.Sender
            ).Where(e=>e.Accepted ==true)
            .Select(x => x.Reciver).ToList();
        var miqQeNaKaneShtuar = _context
            .Requests
            .Where(e => (e.ReciverId == id))
            .Include(e=>e.Reciver)
            .Include(e=>e.Sender
            ).Where(e=>e.Accepted ==true)
            .Select(x => x.Sender).ToList();

        ViewBag.Miqte = miqQeKemiShtuar.Union(miqQeNaKaneShtuar).ToList();
        ViewBag.requests = _context.Requests.Include(e=>e.Reciver).Include(e=>e.Sender).Where(e => e.ReciverId == id).Where(e => e.Accepted == false).ToList();

        ViewBag.iLoguari = _context.Users.FirstOrDefault(e => e.UserId == id);
        return View();
    }

    [HttpGet("allusers")]
    public IActionResult AllUsers()
    {
        ViewBag.AllUsers = _context.Users.ToList();
        return View();
    }
}

