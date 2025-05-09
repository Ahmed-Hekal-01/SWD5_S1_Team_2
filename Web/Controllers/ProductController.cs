using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public ProductController(ApplicationDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IActionResult> Index(
        string searchTerm = "",
        int? categoryId = null,
        string orderBy = "name",
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
    {
        // Validate and adjust page size
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        // Base query with include for Category
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
        }

        // Apply category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.Category.Id == categoryId.Value);
        }

        // Apply price range filter
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }
        // Apply sorting
        query = orderBy.ToLower() switch
        {
            "price_desc" => query.OrderByDescending(p => p.Price),
            "price" => query.OrderBy(p => p.Price),
            "created_desc" => query.OrderByDescending(p => p.CreatedAt),
            "created" => query.OrderBy(p => p.CreatedAt),
            "popularity_desc" => query.OrderByDescending(p => p.NoOfViews),
            "popularity" => query.OrderBy(p => p.NoOfViews),
            "category" => query.OrderBy(p => p.Category.Name),
            "category_desc" => query.OrderByDescending(p => p.Category.Name),
            _ => query.OrderBy(p => p.Name)
        };

        // Get total count before pagination
        int totalRecords;
        try
        {
            totalRecords = await query.CountAsync();
        }
        catch (SqlException ex)
        {
            Console.WriteLine("SQL Error: " + ex.Message);
            throw; // rethrow or handle appropriately
        }

        int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        // Apply pagination
        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Category = p.Category,
                CreatedAt = p.CreatedAt,
                NoOfViews = p.NoOfViews
            })
            .ToListAsync();
        // Get available categories for filter dropdown
        var availableCategories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        // Prepare view model
        var model = new ProductViewModel
        {
            Products = products,
            CurrentPage = pageNumber,
            TotalPages = totalPages,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            OrderBy = orderBy,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            AvailableCategories = availableCategories,
            TotalCount = totalRecords
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductCreateViewModel
        {
            AvailableCategories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(model);
        }

        var category = await _db.Categories.FindAsync(model.CategoryId);
        if (category == null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Invalid category selected");
            model.AvailableCategories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            Category = category,
            CreatedAt = DateTime.UtcNow,
            NoOfViews = 0
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> HotDeals()
    {
        return View();
    }
}
/*
using Domain.Entities;
using Domain.Response;
using Business.Services.ProductService;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public class ProductController : Controller
{
    private readonly IProductService _productService;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public ProductController(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    public async Task<IActionResult> Index(
        string searchTerm = "",
        int? categoryId = null,
        string orderBy = "name",
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
    {
        var result = await _productService.GetProductsAsync(
            searchTerm, categoryId, orderBy, minPrice, maxPrice, pageNumber, pageSize);

        if (!result.IsSuccess)
        {
            // Handle error - you might want to log this or show an error message
            TempData["Error"] = result.ErrorMessage;
            return View(new ProductViewModel());
        }

        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categoriesResult = await _productService.GetCategoriesAsync();

        if (!categoriesResult.IsSuccess)
        {
            TempData["Error"] = categoriesResult.ErrorMessage;
            return View(new ProductCreateViewModel());
        }

        var model = new ProductCreateViewModel
        {
            AvailableCategories = categoriesResult.Data
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categoriesResult = await _productService.GetCategoriesAsync();
            model.AvailableCategories = categoriesResult.IsSuccess ? categoriesResult.Data : new List<Category>();
            return View(model);
        }

        var result = await _productService.CreateProductAsync(model);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            var categoriesResult = await _productService.GetCategoriesAsync();
            model.AvailableCategories = categoriesResult.IsSuccess ? categoriesResult.Data : new List<Category>();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> HotDeals()
    {
        return View();
    }
}

*/
