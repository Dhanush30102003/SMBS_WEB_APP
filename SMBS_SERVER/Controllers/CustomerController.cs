using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Services;

[Authorize]
public class CustomerController : Controller
{
    private readonly CustomerService _service;

    public CustomerController(CustomerService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        int tenantId = 1; // get from JWT later
        var list = _service.GetAll(tenantId);
        return View(list);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(CustomerMaster model)
    {
        model.TenantID = 1;
        _service.Create(model);
        return RedirectToAction("Index");
    }
    public IActionResult ToggleStatus(int id)
    {
        _service.ToggleStatus(id);
        return RedirectToAction("Index");
    }


    public IActionResult Edit(int id)
    {
        var customer = _service.Get(id);
        return View(customer);
    }

    [HttpPost]
    public IActionResult Edit(CustomerMaster model)
    {
        _service.Update(model);
        return RedirectToAction("Index");
    }

    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return RedirectToAction("Index");
    }
}
