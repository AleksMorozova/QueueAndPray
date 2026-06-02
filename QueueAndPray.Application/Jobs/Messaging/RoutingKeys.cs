using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueAndPray.Application.Jobs.Messaging;

public static class RoutingKeys
{
    public const string JobQueued = "job.queued";
    public const string JobStatus = "job.status";
}
