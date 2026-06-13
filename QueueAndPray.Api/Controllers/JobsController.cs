using Microsoft.AspNetCore.Mvc;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Contracts.Common;
using QueueAndPray.Contracts.Jobs.Requests;
using QueueAndPray.Contracts.Jobs.Responses;

namespace QueueAndPray.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;
        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }   
        
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<JobResponse>>>> GetJobs([FromQuery] GetJobsRequest request, CancellationToken cancellationToken)
        {
            var result = await _jobService.GetJobsAsync(request, cancellationToken);

            return Ok(ApiResponse<PagedResponse<JobResponse>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<JobDetailedResponse>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _jobService.GetJobAsync(id, cancellationToken);

            return Ok(ApiResponse<JobDetailedResponse>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Guid>>> CreateJob(CreateJobRequest request, CancellationToken cancellationToken)
        {
            var result = await _jobService.CreateJobAsync(request, cancellationToken);

            return Ok(ApiResponse<Guid>.Ok(result.JobId));
        }
    }
}
