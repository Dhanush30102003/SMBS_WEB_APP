using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Services;
using System.Security.Claims;

[Authorize]
public class EmployeeController : Controller
{
    private readonly UserService _userService;

    public EmployeeController(UserService userService)
    {
        _userService = userService;
    }

    // ✅ LIST
    public IActionResult Index()
    {
        var employees = _userService.GetAllEmployees();
        return View(employees);
    }

    // ✅ CREATE (GET)
    public IActionResult Create()
    {
        return View(new UserMaster());
    }

    // ✅ CREATE (POST)
    
    [HttpPost]
    public IActionResult Create(UserMaster model)
    {
        if (!ModelState.IsValid)
            return View(model);
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized(); // JWT missing or expired
        }
        // 🔑 LOGGED-IN USER ID (FROM JWT CLAIM)
        int createdByUserId = int.Parse(userIdClaim.Value);


        _userService.SaveEmployee(model, createdByUserId);

        return RedirectToAction("Index");
    }
    // ✅ EDIT (GET)
    public IActionResult Edit(int id)
    {
        var user = _userService.GetById(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    // ✅ EDIT (POST)
    [HttpPost]
    public IActionResult Edit(UserMaster model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        int createdByUserId = int.Parse(userIdClaim.Value);
        _userService.SaveEmployee(model, createdByUserId);
        return RedirectToAction("Index");
    }
}
