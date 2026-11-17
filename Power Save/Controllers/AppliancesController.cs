using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerSave.Application.DTOs;
using PowerSave.Application.Interfaces;

namespace Power_Save.Controllers;

[ApiController]
[Route("api/appliances")]
[Authorize]
public class AppliancesController : ControllerBase
{
    private readonly IApplianceService _applianceService;
    private readonly IApplianceModelService _modelService;

    public AppliancesController(IApplianceService applianceService, IApplianceModelService modelService)
    {
        _applianceService = applianceService;
        _modelService = modelService;
    }

    private Guid GetAuthenticatedUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idStr, out var id)) return id;
        throw new UnauthorizedAccessException("المستخدم غير مصرح له.");
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApplianceCategoryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _applianceService.GetCategoriesWithAppliancesAsync();
        return Ok(ApiResponse<IReadOnlyList<ApplianceCategoryResponseDto>>.Ok(result.Data!));
    }

    [HttpGet("by-category/{categoryId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApplianceCatalogResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var result = await _applianceService.GetAppliancesByCategoryAsync(categoryId);
        return Ok(ApiResponse<IReadOnlyList<ApplianceCatalogResponseDto>>.Ok(result.Data!));
    }

    [HttpGet("my-appliances")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserApplianceResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAppliances()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _applianceService.GetUserAppliancesAsync(userId);
        return Ok(ApiResponse<IReadOnlyList<UserApplianceResponseDto>>.Ok(result.Data!));
    }

    [HttpPost("selections")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserApplianceResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveSelections([FromBody] SaveAppliancesRequestDto request)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _applianceService.SaveApplianceSelectionsAsync(userId, request);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, ApiResponse.Fail(result.Message, result.ErrorCode.ToString()));
        }
        return Ok(ApiResponse<IReadOnlyList<UserApplianceResponseDto>>.Ok(result.Data!, "تم تحويل وحفظ الأجهزة كوحدات مستقلة بنجاح."));
    }

    [HttpPost("custom")]
    [ProducesResponseType(typeof(ApiResponse<UserApplianceResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddCustom([FromBody] ApplianceSelectionItemDto customAppliance)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _applianceService.AddCustomApplianceAsync(userId, customAppliance);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, ApiResponse.Fail(result.Message, result.ErrorCode.ToString()));
        }
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UserApplianceResponseDto>.Ok(result.Data!, result.Message));
    }

    [HttpDelete("{userApplianceId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAppliance(Guid userApplianceId)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _applianceService.RemoveUserApplianceAsync(userId, userApplianceId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, ApiResponse.Fail(result.Message, result.ErrorCode.ToString()));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }

    [HttpDelete("all")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveAll()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _applianceService.RemoveAllUserAppliancesAsync(userId);
        return Ok(ApiResponse.Ok(result.Message));
    }

    [HttpGet("model-sensitive")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApplianceCatalogResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModelSensitive()
    {
        var result = await _modelService.GetModelSensitiveAppliancesAsync();
        return Ok(ApiResponse<IReadOnlyList<ApplianceCatalogResponseDto>>.Ok(result.Data!));
    }

    [HttpGet("models/{applianceCatalogId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApplianceModelResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModels(Guid applianceCatalogId)
    {
        var result = await _modelService.GetModelsForApplianceAsync(applianceCatalogId);
        return Ok(ApiResponse<IReadOnlyList<ApplianceModelResponseDto>>.Ok(result.Data!));
    }

    [HttpGet("models/search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApplianceModelResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchModels([FromQuery] string q)
    {
        var result = await _modelService.SearchModelsAsync(q);
        return Ok(ApiResponse<IReadOnlyList<ApplianceModelResponseDto>>.Ok(result.Data!));
    }

    [HttpPost("models/selections")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserApplianceResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveModelSelections([FromBody] SaveApplianceModelsRequestDto request)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _modelService.SaveModelSelectionsAsync(userId, request);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, ApiResponse.Fail(result.Message, result.ErrorCode.ToString()));
        }
        return Ok(ApiResponse<IReadOnlyList<UserApplianceResponseDto>>.Ok(result.Data!, result.Message));
    }
}
