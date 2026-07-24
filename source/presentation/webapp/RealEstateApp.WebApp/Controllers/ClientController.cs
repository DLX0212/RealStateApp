using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Chat;
using RealEstateApp.Core.Application.Dtos.Offer;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Chat;
using RealEstateApp.Core.Application.ViewModels.Offer;
using RealEstateApp.Core.Application.ViewModels.Property;
using System.Security.Claims;

namespace RealEstateApp.WebApp.Controllers;

[Authorize(Roles = "Client")]
public class ClientController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly IPropertyTypeService _propertyTypeService;
    private readonly IFavoritePropertyService _favoritePropertyService;
    private readonly IMessageService _messageService;
    private readonly IOfferService _offerService;

    public ClientController(
        IPropertyService propertyService,
        IPropertyTypeService propertyTypeService,
        IFavoritePropertyService favoritePropertyService,
        IMessageService messageService,
        IOfferService offerService)
    {
        _propertyService = propertyService;
        _propertyTypeService = propertyTypeService;
        _favoritePropertyService = favoritePropertyService;
        _messageService = messageService;
        _offerService = offerService;
    }

    private string CurrentClientId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index(PropertyFilterViewModel filter)
    {
        var propertyTypes = await _propertyTypeService.GetAllAsync();
        filter.PropertyTypes = propertyTypes
            .Select(pt => new Core.Application.ViewModels.Catalog.PropertyTypeViewModel
            {
                Id = pt.Id,
                Name = pt.Name,
                Description = pt.Description,
                PropertiesCount = pt.PropertiesCount
            })
            .ToList();

        List<PropertyDto> properties;
        var hasFilters = filter.PropertyTypeId.HasValue || filter.MinPrice.HasValue || filter.MaxPrice.HasValue
                          || filter.Bedrooms.HasValue || filter.Bathrooms.HasValue;
        var hasCodeSearch = !string.IsNullOrWhiteSpace(filter.Code);

        if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice > filter.MaxPrice)
        {
            ModelState.AddModelError(nameof(filter.MinPrice), "Minimum price cannot be greater than maximum price.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.NoResultsMessage = "Please correct the filter values entered.";
            return View(new List<PropertyListItemViewModel>());
        }

        if (hasCodeSearch)
        {
            var property = await _propertyService.GetAvailableByCodeAsync(filter.Code!.Trim());
            properties = property is null ? new List<PropertyDto>() : new List<PropertyDto> { property };

            if (properties.Count == 0)
            {
                ViewBag.NoResultsMessage = "No available property was found with the entered code.";
            }
        }
        else if (hasFilters)
        {
            var filterDto = new PropertyFilterDto
            {
                PropertyTypeId = filter.PropertyTypeId,
                MinPrice = filter.MinPrice,
                MaxPrice = filter.MaxPrice,
                Bedrooms = filter.Bedrooms,
                Bathrooms = filter.Bathrooms
            };
            properties = await _propertyService.GetAvailableFilteredAsync(filterDto);

            if (properties.Count == 0)
            {
                ViewBag.NoResultsMessage = "No available properties were found with the selected filters.";
            }
        }
        else
        {
            properties = await _propertyService.GetAvailableAsync();
        }

        var favoriteIds = await _favoritePropertyService.GetFavoritePropertyIdsAsync(CurrentClientId);

        var items = properties.Select(p => new PropertyListItemViewModel
        {
            Id = p.Id,
            Code = p.Code,
            PropertyTypeName = p.PropertyTypeName,
            SaleTypeName = p.SaleTypeName,
            MainImagePath = p.Images.Select(i => i.Path).FirstOrDefault(),
            Price = p.Price,
            Size = p.Size,
            Bedrooms = p.Bedrooms,
            Bathrooms = p.Bathrooms,
            Status = p.Status,
            IsFavorite = favoriteIds.Contains(p.Id)
        }).ToList();

        ViewBag.Filter = filter;
        return View(items);
    }

    public async Task<IActionResult> MyProperties()
    {
        var favoriteIds = await _favoritePropertyService.GetFavoritePropertyIdsAsync(CurrentClientId);
        var allProperties = await _propertyService.GetAvailableAsync();

        var favoriteProperties = allProperties.Where(p => favoriteIds.Contains(p.Id)).ToList();

        if (favoriteProperties.Count == 0)
        {
            ViewBag.NoResultsMessage = "You do not have any available favorite properties at this time.";
        }

        var items = favoriteProperties.Select(p => new PropertyListItemViewModel
        {
            Id = p.Id,
            Code = p.Code,
            PropertyTypeName = p.PropertyTypeName,
            SaleTypeName = p.SaleTypeName,
            MainImagePath = p.Images.Select(i => i.Path).FirstOrDefault(),
            Price = p.Price,
            Size = p.Size,
            Bedrooms = p.Bedrooms,
            Bathrooms = p.Bathrooms,
            Status = p.Status,
            IsFavorite = true
        }).ToList();

        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int propertyId, string returnAction)
    {
        var isFavorite = await _favoritePropertyService.IsFavoriteAsync(CurrentClientId, propertyId);

        try
        {
            if (isFavorite)
            {
                await _favoritePropertyService.RemoveAsync(CurrentClientId, propertyId);
                TempData["SuccessMessage"] = "The property was removed from your favorites successfully.";
            }
            else
            {
                await _favoritePropertyService.AddAsync(CurrentClientId, propertyId);
                TempData["SuccessMessage"] = "The property was added to your favorites successfully.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return returnAction switch
        {
            "MyProperties" => RedirectToAction(nameof(MyProperties)),
            "Details" => RedirectToAction("Details", new { id = propertyId }),
            _ => RedirectToAction(nameof(Index))
        };
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property is null)
        {
            return NotFound();
        }

        var isFavorite = await _favoritePropertyService.IsFavoriteAsync(CurrentClientId, id);
        var messages = await _messageService.GetConversationAsync(id, CurrentClientId, property.AgentId);
        var offers = await _offerService.GetByPropertyAndClientAsync(id, CurrentClientId);

        var isSold = property.Status == "Sold";
        var hasAcceptedOffer = offers.Any(o => o.Status == "Accepted");
        var hasPendingOffer = offers.Any(o => o.Status == "Pending");

        string? blockedReason = null;
        if (isSold) blockedReason = "This property has already been sold.";
        else if (hasAcceptedOffer) blockedReason = "This property already has an accepted offer.";
        else if (hasPendingOffer) blockedReason = "You already have a pending offer for this property.";

        var vm = new PropertyDetailsViewModel
        {
            Id = property.Id,
            Code = property.Code,
            PropertyTypeName = property.PropertyTypeName,
            SaleTypeName = property.SaleTypeName,
            Price = property.Price,
            Size = property.Size,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            Description = property.Description,
            Status = property.Status,
            ImagePaths = property.Images.Select(i => i.Path).ToList(),
            Improvements = property.Improvements,
            AgentId = property.AgentId,
            AgentFullName = property.AgentFullName,
            AgentPhone = property.AgentPhone,
            AgentEmail = property.AgentEmail,
            AgentProfilePicturePath = property.AgentProfilePicturePath,
            IsFavorite = isFavorite,
            Messages = messages.Select(m => new ConversationViewModel
            {
                Id = m.Id,
                Content = m.Content,
                SenderRole = m.SenderRole,
                CreatedAt = m.CreatedAt
            }).ToList(),
            NewMessage = new SendMessageViewModel { PropertyId = id },
            Offers = offers.Select(o => new OfferViewModel
            {
                Id = o.Id,
                PropertyId = id,
                Amount = o.Amount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList(),
            NewOffer = new SendOfferViewModel { PropertyId = id },
            CanCreateOffer = blockedReason is null,
            OfferBlockedReason = blockedReason
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(SendMessageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please write a message before sending.";
            return RedirectToAction(nameof(Details), new { id = model.PropertyId });
        }

        var property = await _propertyService.GetByIdAsync(model.PropertyId);
        if (property is null)
        {
            return NotFound();
        }

        var dto = new MessageDto
        {
            Content = model.Content,
            SenderRole = "Client",
            ClientId = CurrentClientId,
            AgentId = property.AgentId,
            PropertyId = model.PropertyId
        };

        await _messageService.SendAsync(dto);

        return RedirectToAction(nameof(Details), new { id = model.PropertyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendOffer(SendOfferViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "The offer amount must be greater than zero.";
            return RedirectToAction(nameof(Details), new { id = model.PropertyId });
        }

        try
        {
            await _offerService.CreateAsync(new SaveOfferDto
            {
                Amount = model.Amount,
                PropertyId = model.PropertyId,
                ClientId = CurrentClientId
            });
            TempData["SuccessMessage"] = "Your offer was submitted successfully.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.PropertyId });
    }
}