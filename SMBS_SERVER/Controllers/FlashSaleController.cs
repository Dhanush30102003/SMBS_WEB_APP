using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Repositories;
using SMBS_SERVER.Models.Masters;

[Authorize(Roles = "Admin")]
public class FlashSaleController : Controller
{
    private readonly FlashSaleRepository _repository;

    public FlashSaleController(FlashSaleRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var sales = _repository.GetAll();
        return View(sales);
    }

    public IActionResult Create()
    {
        return View();
    }
    public IActionResult Delete(int id)
    {
        _repository.Delete(id);
        return RedirectToAction("Index");
    }
    public IActionResult Edit(int id)
    {
        var sale = _repository.GetById(id);
        if (sale == null)
            return NotFound();

        return View(sale);
    }

    [HttpPost]
    public IActionResult Edit(FlashSaleMaster model)
    {
        try
        {
            _repository.Update(model);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    public IActionResult Create(FlashSaleMaster model)
    {
        try
        {
            var tenantClaim = User.FindFirst("TenantId");

            if (tenantClaim != null)
            {
                model.TenantID = int.Parse(tenantClaim.Value);
            }

            model.IsActive = true;
            model.IsNotified = false;

            _repository.Insert(model);

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}