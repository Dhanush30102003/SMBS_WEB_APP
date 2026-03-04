    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using SMBS_SERVER.Models.Masters;
    using SMBS_SERVER.Models.ViewModels;
    using SMBS_SERVER.Services;

    //[Authorize]
    public class ProductController : Controller
    {
        private readonly ProductService _service;
        private readonly CategoryService _categoryService;
        private readonly UomService _uomService;
    private readonly FcmService _fcmService;
    private readonly CustomerService _customerService;

    public ProductController(ProductService service, CategoryService cs, UomService us, FcmService fcmService,
    CustomerService customerService)
        {
            _service = service;
            _categoryService = cs;
            _uomService = us;
        _fcmService = fcmService;
        _customerService = customerService;
    }

        [HttpGet]
        public IActionResult Create()
        {
            int tenantId = 1; // later from JWT

            var vm = new ProductCreateViewModel();

            vm.Categories = _categoryService.GetAll(tenantId)
         .Select(c => new SelectListItem
         {
             Value = c.CategoryID.ToString(),   // ✅ MUST be ID
             Text = c.CategoryName              // ✅ Display name
         })
         .ToList();

            vm.UOMs = _uomService.GetAll()
                .Select(u => new SelectListItem
                {
                    Value = u.UOMID.ToString(),
                    Text = u.UOMName
                }).ToList();

            return View(vm);
        }
        public IActionResult Index()
        {
            int tenantId = 1;
            var products = _service.GetAllProducts(tenantId);
            return View(products);
        }
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = _service.GetById(id);

        if (product == null)
            return NotFound();

        var vm = new ProductCreateViewModel
        {
            ProductID = product.ProductID,
            ProductCode = product.ProductCode,
            SKU = product.SKU,
            ProductName = product.ProductName,
            Barcode = product.Barcode,
            Description = product.Description,
            CategoryID = product.CategoryID,
            UOMID = product.UOMID,
            SellingPrice = product.SellingPrice,
            MRP = product.MRP,
            DiscountPercentage = product.DiscountPercentage
        };

        vm.Categories = _categoryService.GetAll(1)
            .Select(c => new SelectListItem
            {
                Value = c.CategoryID.ToString(),
                Text = c.CategoryName
            }).ToList();

        vm.UOMs = _uomService.GetAll()
            .Select(u => new SelectListItem
            {
                Value = u.UOMID.ToString(),
                Text = u.UOMName
            }).ToList();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductCreateViewModel vm)
    {
        byte[]? imageBytes = null;

        if (vm.ImageFile != null)
        {
            using (var ms = new MemoryStream())
            {
                vm.ImageFile.CopyTo(ms);
                imageBytes = ms.ToArray();
            }
        }
        var existingProduct = _service.GetById(vm.ProductID);

        if (existingProduct == null)
            return NotFound();

        bool priceChanged = existingProduct.SellingPrice != vm.SellingPrice;
        _service.UpdateProduct(new ProductMaster
        {
            ProductID = vm.ProductID,
            ProductCode = vm.ProductCode,
            SKU = vm.SKU,
            ProductName = vm.ProductName,
            Barcode = vm.Barcode,
            Description = vm.Description,
            CategoryID = vm.CategoryID,
            UOMID = vm.UOMID,
            ProductImage = imageBytes,
            ModifiedDate = DateTime.Now
        },
        vm.SellingPrice,
        vm.MRP,
        vm.DiscountPercentage);
        if (priceChanged)
        {
            int tenantId = 1;
            var customers = _customerService.GetAllWithToken(tenantId);

            var tokens = customers
                .Where(c => !string.IsNullOrEmpty(c.FCMToken))
                .Select(c => c.FCMToken)
                .ToList();

            if (tokens.Any())
            {
                await _fcmService.SendMulticast(
                    tokens,
                    "Price Updated",
                    $"{vm.ProductName} price updated to ₹{vm.SellingPrice}"
                );
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProductCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = _categoryService.GetAll(1)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryID.ToString(),
                    Text = c.CategoryName
                }).ToList();

            vm.UOMs = _uomService.GetAll()
                .Select(u => new SelectListItem
                {
                    Value = u.UOMID.ToString(),
                    Text = u.UOMName
                }).ToList();

            return View(vm);
        }

        byte[]? imageBytes = null;

        if (vm.ImageFile != null)
        {
            using (var ms = new MemoryStream())
            {
                vm.ImageFile.CopyTo(ms);
                imageBytes = ms.ToArray();
            }
        }

        _service.AddProduct(new ProductMaster
        {
            TenantID = 1,
            ProductCode = vm.ProductCode,
            SKU = vm.SKU,
            ProductName = vm.ProductName,
            Barcode = vm.Barcode,
            Description = vm.Description,
            CategoryID = vm.CategoryID,
            UOMID = vm.UOMID,
            ProductImage = imageBytes,   // ✅ Saving image
            IsActive = true,
            CreatedDate = DateTime.Now,
            ModifiedDate = DateTime.Now
        });

        return RedirectToAction(nameof(Index));
    }






}
