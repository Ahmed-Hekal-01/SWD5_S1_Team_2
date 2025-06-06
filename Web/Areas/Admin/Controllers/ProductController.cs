using Business.Services.ProductService;
using Microsoft.AspNetCore.Mvc;
using Business.ViewModels.ProductViewModels;
using Domain;
using Business.Services.CategoryService;
using AutoMapper;
using Web.Helpers;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;


    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public ProductController(IProductService productService, ICategoryService categoryService, IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

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
        string? status = null)
    {
        try
        {
            var result = await _productService.GetProductsAsync(
            searchTerm, categoryId, orderBy, minPrice, maxPrice, pageNumber, pageSize, status);

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
        catch (Exception ex)
        {
            // Handle error - you might want to log this or show an error message
            TempData["Error"] = ex.Message;
            return View(new List<ProductViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = categories;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductViewModel model)
    {


        var result = await _productService.CreateProductAsync(model);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();

            var updateProductViewModel = _mapper.Map<UpdateProductViewModel>(result.Data);

            return View(updateProductViewModel);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProductViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data";
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(model);
            }

            var result = await _productService.UpdateProductAsync(model);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(model);
            }

            TempData["Success"] = "Product updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result)
        {
            TempData["Error"] = "Failed to delete the product";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Product deleted successfully";
        return RedirectToAction(nameof(Index));
    }
}
