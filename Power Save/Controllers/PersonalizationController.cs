using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerSave.Application.DTOs;
using PowerSave.Application.Interfaces;
using PowerSave.Domain.Enums;

namespace Power_Save.Controllers;

[ApiController]
[Route("api/personalization")]
[Authorize]
public class PersonalizationController : ControllerBase
{
    private readonly IPersonalizationService _personalizationService;
    private readonly IHouseholdProfileService _householdService;
    private readonly IMeterSetupService _meterService;
    private readonly ISavingTargetService _savingTargetService;

    public PersonalizationController(
        IPersonalizationService personalizationService,
        IHouseholdProfileService householdService,
        IMeterSetupService meterService,
        ISavingTargetService savingTargetService)
    {
        _personalizationService = personalizationService;
        _householdService = householdService;
        _meterService = meterService;
        _savingTargetService = savingTargetService;
    }

    private Guid GetAuthenticatedUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idStr, out var id)) return id;
        throw new UnauthorizedAccessException("المستخدم غير مصرح له.");
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<PersonalizationStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _personalizationService.GetStatusAsync(userId);
        return Ok(ApiResponse<PersonalizationStatusDto>.Ok(result.Data!));
    }

    [HttpGet("saved-answers")]
    [ProducesResponseType(typeof(ApiResponse<PersonalizationSavedAnswersDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavedAnswers()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _personalizationService.GetSavedAnswersAsync(userId);
        return Ok(ApiResponse<PersonalizationSavedAnswersDto>.Ok(result.Data!));
    }

    [HttpGet("resume")]
    [ProducesResponseType(typeof(ApiResponse<PersonalizationStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resume()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _personalizationService.ResumeOnboardingAsync(userId);
        return Ok(ApiResponse<PersonalizationStatusDto>.Ok(result.Data!));
    }

    [HttpPost("skip-stage")]
    [ProducesResponseType(typeof(ApiResponse<PersonalizationStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SkipStage([FromBody] SkipStageRequestDto request)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _personalizationService.SkipStageAsync(userId, request.Stage);
        return Ok(ApiResponse<PersonalizationStatusDto>.Ok(result.Data!, result.Message));
    }

    [HttpPost("complete")]
    [ProducesResponseType(typeof(ApiResponse<PersonalizationStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _personalizationService.CompletePersonalizationAsync(userId);
        return Ok(ApiResponse<PersonalizationStatusDto>.Ok(result.Data!, result.Message));
    }

    [HttpPost("household")]
    [ProducesResponseType(typeof(ApiResponse<HouseholdProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveHousehold([FromBody] HouseholdProfileDto dto)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _householdService.SaveHouseholdProfileAsync(userId, dto);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, ApiResponse.Fail(result.Message, result.ErrorCode.ToString()));
        }
        return Ok(ApiResponse<HouseholdProfileDto>.Ok(result.Data!, result.Message));
    }

    [HttpGet("household")]
    [ProducesResponseType(typeof(ApiResponse<HouseholdProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHousehold()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _householdService.GetHouseholdProfileAsync(userId);
        return Ok(ApiResponse<HouseholdProfileDto>.Ok(result.Data!));
    }

    [HttpPost("lifestyle")]
    [ProducesResponseType(typeof(ApiResponse<LifestyleProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveLifestyle([FromBody] LifestyleProfileDto dto)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _householdService.SaveLifestyleProfileAsync(userId, dto);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, ApiResponse.Fail(result.Message, result.ErrorCode.ToString()));
        }
        return Ok(ApiResponse<LifestyleProfileDto>.Ok(result.Data!, result.Message));
    }

    [HttpGet("lifestyle")]
    [ProducesResponseType(typeof(ApiResponse<LifestyleProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLifestyle()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _householdService.GetLifestyleProfileAsync(userId);
        return Ok(ApiResponse<LifestyleProfileDto>.Ok(result.Data!));
    }

    [HttpPost("meter")]
    [ProducesResponseType(typeof(ApiResponse<MeterSetupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveMeter([FromBody] MeterSetupDto dto)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _meterService.SaveMeterSetupAsync(userId, dto);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, ApiResponse.Fail(result.Message, result.ErrorCode.ToString()));
        }
        return Ok(ApiResponse<MeterSetupDto>.Ok(result.Data!, result.Message));
    }

    [HttpGet("meter")]
    [ProducesResponseType(typeof(ApiResponse<MeterSetupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMeter()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _meterService.GetMeterSetupAsync(userId);
        return Ok(ApiResponse<MeterSetupDto>.Ok(result.Data!));
    }

    [HttpPost("saving-target")]
    [ProducesResponseType(typeof(ApiResponse<SavingTargetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveSavingTarget([FromBody] SavingTargetDto dto)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _savingTargetService.SaveSavingTargetAsync(userId, dto);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, ApiResponse.Fail(result.Message, result.ErrorCode.ToString()));
        }
        return Ok(ApiResponse<SavingTargetDto>.Ok(result.Data!, result.Message));
    }

    [HttpGet("saving-target")]
    [ProducesResponseType(typeof(ApiResponse<SavingTargetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavingTarget()
    {
        var userId = GetAuthenticatedUserId();
        var result = await _savingTargetService.GetSavingTargetAsync(userId);
        return Ok(ApiResponse<SavingTargetDto>.Ok(result.Data!));
    }
}
