using API.Configuration;
using BUS.HumanResources.Employees;
using DTO.HumanResources.Employees;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.HumanResources;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController(EmployeeService employeeService, DatabaseSettings database) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(CancellationToken cancellationToken)
    {
        if (!database.IsConfigured)
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Chưa cấu hình kết nối SQL Server.",
                detail: "Thiết lập HRM_CONNECTION_STRING hoặc ConnectionStrings__HumanResourceManagement trên máy chạy API.");

        return Ok(await employeeService.GetAllAsync(cancellationToken));
    }
}
