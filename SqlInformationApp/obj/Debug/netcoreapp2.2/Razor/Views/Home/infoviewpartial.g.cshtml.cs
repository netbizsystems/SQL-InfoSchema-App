#pragma checksum "C:\Users\David\source\repos\SQL-InfoSchema-App-Azure\SQLInformationApp\Views\Home\infoviewpartial.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "402dd25d8e4104ac5e06d7df3c812f7b2bbe73c8"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_infoviewpartial), @"mvc.1.0.view", @"/Views/Home/infoviewpartial.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Home/infoviewpartial.cshtml", typeof(AspNetCore.Views_Home_infoviewpartial))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\Users\David\source\repos\SQL-InfoSchema-App-Azure\SQLInformationApp\Views\_ViewImports.cshtml"
using AndersonEnterprise.SqlInformationApp;

#line default
#line hidden
#line 2 "C:\Users\David\source\repos\SQL-InfoSchema-App-Azure\SQLInformationApp\Views\_ViewImports.cshtml"
using AndersonEnterprise.SqlInformationApp.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"402dd25d8e4104ac5e06d7df3c812f7b2bbe73c8", @"/Views/Home/infoviewpartial.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"40636fcf9639861a6ba24a30eac00f65ee821acb", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_infoviewpartial : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(0, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 2 "C:\Users\David\source\repos\SQL-InfoSchema-App-Azure\SQLInformationApp\Views\Home\infoviewpartial.cshtml"
   /* razor context */ 

#line default
#line hidden
            BeginContext(28, 316, true);
            WriteLiteral(@"
<div class=""text-center"">
    <h1 class=""display-4"">SQL InformationViewer</h1>
    <p>Learn about <a href=""https://github.com/netbizsystems/SQL-InfoSchema-App"">building & running InformationViewer queries</a>.</p>
</div>
<br />

<a data-bind=""click:onResetQueryClick"" href=""#"">reset query</a><br /><br />

");
            EndContext();
            BeginContext(366, 129, true);
            WriteLiteral("<div data-bind=\"foreach: $root.infoSchemas\">\r\n    <h4 class=\"text-primary\" data-bind=\"text:$root.setSchemaContext($data)\"></h4>\r\n");
            EndContext();
            BeginContext(530, 74, true);
            WriteLiteral("    <div class=\"row\" data-bind=\"foreach: $root.getTables($root.$index)\">\r\n");
            EndContext();
            BeginContext(650, 151, true);
            WriteLiteral("        <div class=\"col-3\" data-bind=\"click: $root.onTableClick, template: { name: \'infotables-template\', data: $data }\"></div>\r\n    </div>\r\n</div>\r\n\r\n");
            EndContext();
            BeginContext(827, 1546, true);
            WriteLiteral(@"<script type=""text/html"" id=""infotables-template"">
    <div style=""margin-bottom:10px;"" class=""card"" data-bind=""class: cardClass"">
        <div style=""width:100%"" class=""flip-card"">
            <div class=""flip-card-inner"">

                <div class=""flip-card-front"">
                    <h4 data-bind=""text: table_name""></h4>
                    <h6 data-bind=""text: table_type""></h6>
                    <span data-bind=""visible:isSelectedQueryTable,text:selectedTableSeq"" class=""badge badge-pill badge-secondary""></span>
                    <span data-bind=""visible:isJoinedQueryTable,text:parentTableSeq"" class=""badge badge-pill badge-light"">1</span>
                    <div style=""position:absolute;bottom:0; margin-bottom:7px;margin:7px;right:5px;"">
                        <span class=""fa fa-star"" style=""font-size:1.5em;"" data-bind=""visible:isPrimaryQueryTable""></span>
                        <span class=""fa fa-star-o"" style=""font-size:1.5em;"" data-bind=""visible:isJoinedQueryTable""></span>
      ");
            WriteLiteral(@"              </div>
                </div>

                <div class=""flip-card-back"">
                    <h4 data-bind=""text: table_name""></h4>
                    <div style=""position:absolute; bottom:0; margin-bottom:7px; margin:7px; right: 5px;"">
                        <span data-bind=""click:function() {$root.onLockClick($data)}, clickBubble: false"" class=""fa fa-lock"" style=""font-size:1.5em;""></span>
                    </div>
                </div>

            </div>
        </div>
    </div>
");
            EndContext();
            BeginContext(2440, 11, true);
            WriteLiteral("</script>\r\n");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
