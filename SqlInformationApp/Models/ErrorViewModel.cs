using System;
using Microsoft.AspNetCore.Diagnostics;

namespace AndersonEnterprise.SqlInformationApp.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public Exception ErrorException { get; internal set; }
    }
}