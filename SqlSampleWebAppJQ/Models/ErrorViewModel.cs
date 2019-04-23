using System;
using System.ComponentModel;

namespace SqlSampleWebApp.Models
{
    public class ErrorViewModel
    {
        [DisplayName("FooFooBooooo")]
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}