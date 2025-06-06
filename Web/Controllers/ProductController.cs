using Business.Services.CategoryService;
using Business.Services.ProductService;
using Business.ViewModels.ProductViewModels;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Web.Helpers;

namespace Web.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public ProductController(ApplicationDbContext db, IProductService productService, ICategoryService categoryService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
    }

    public async Task<IActionResult> Index(
        string searchTerm = "",
        int? categoryId = null,
        OrderBy orderBy = OrderBy.NameAsc,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string status = null)
    {
        var result = await _productService.GetProductsAsync(
            searchTerm, categoryId, orderBy, minPrice, maxPrice, pageNumber, pageSize, status, 
            filterBy: FilterBy.Featured);
            
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return View(new List<ProductViewModel>());
        }

        SetViewBag.SetViewBagData(
            this,
            searchTerm,
            categoryId,
            orderBy,
            minPrice,
            maxPrice,
            status,
            result.TotalCount,
            result.TotalPages,
            pageNumber
        );

        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();

        return View(result.Data);
    }

    public async Task<IActionResult> HotDeals(
        string searchTerm = "",
        int? categoryId = null,
        OrderBy orderBy = OrderBy.NameAsc,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string status = null)
    {
        var result = await _productService.GetProductsAsync(
            searchTerm, categoryId, orderBy, minPrice, maxPrice, pageNumber, pageSize, status,
            filterBy: FilterBy.Discounted);
            
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return View(new List<ProductViewModel>());
        }

        SetViewBag.SetViewBagData(
            this,
            searchTerm,
            categoryId,
            orderBy,
            minPrice,
            maxPrice,
            status,
            result.TotalCount,
            result.TotalPages,
            pageNumber
        );

        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();

        return View(result.Data);
    }
    
    public async Task<IActionResult> NewArrivals(
        string searchTerm = "",
        int? categoryId = null,
        OrderBy orderBy = OrderBy.NameAsc,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string status = null)
    {
        var result = await _productService.GetProductsAsync(
            searchTerm, categoryId, orderBy, minPrice, maxPrice, pageNumber, pageSize, status,
            filterBy: FilterBy.NewArrivals);
            
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return View(new List<ProductViewModel>());
        }

        SetViewBag.SetViewBagData(
            this,
            searchTerm,
            categoryId,
            orderBy,
            minPrice,
            maxPrice,
            status,
            result.TotalCount,
            result.TotalPages,
            pageNumber
        );

        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();

        return View(result.Data);
    }
    
    public async Task<IActionResult> BestSellers(
        string searchTerm = "",
        int? categoryId = null,
        OrderBy orderBy = OrderBy.NameAsc,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string status = null)
    {
        var result = await _productService.GetProductsAsync(
            searchTerm, categoryId, orderBy, minPrice, maxPrice, pageNumber, pageSize, status,
            filterBy: FilterBy.BestSelling);
            
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return View(new List<ProductViewModel>());
        }

        SetViewBag.SetViewBagData(
            this,
            searchTerm,
            categoryId,
            orderBy,
            minPrice,
            maxPrice,
            status,
            result.TotalCount,
            result.TotalPages,
            pageNumber
        );

        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();

        return View(result.Data);
    }
}
