using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueAndPray.Application.Common.Resilience;

public sealed record RetryExecutionResult(bool IsSuccess, string? ErrorMessage = null);