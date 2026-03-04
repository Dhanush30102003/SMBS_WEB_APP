using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Services;
using SMBS_SERVER.Models.ViewModels;

public class InventoryController : Controller
{
    private readonly InventoryService _inventoryService;

    public InventoryController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        int tenantId = 1;
        int branchId = 1;

        ViewBag.Inventory = _inventoryService.GetInventory(tenantId, branchId);
        return View();
    }

    [HttpPost]
    public IActionResult AdjustStock(int productId, decimal quantity, string actionType)
    {
        int tenantId = 1;
        int branchId = 1;

        if (actionType == "Add")
        {
            _inventoryService.AddStock(productId, branchId, quantity, "Manual");
        }
        else if (actionType == "Reduce")
        {
            _inventoryService.ReduceStock(productId, branchId, quantity, "Manual");
        }

        return RedirectToAction("Index");
    }
}
