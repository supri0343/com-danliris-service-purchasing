using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using Asp.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace Com.DanLiris.Service.Purchasing.WebApi.SchedulerJobs
{
    public class MasterRegistry : Registry
    {
        public MasterRegistry(IServiceProvider serviceProvider)
        {
            var coreService = serviceProvider.GetService<ICoreData>();
            Schedule(() =>
            {
                coreService.SetBankAccount();
                coreService.SetCategoryCOA();
                coreService.SetDivisionCOA();
                coreService.SetPPhCOA();
                coreService.SetUnitCOA();
            }).ToRunNow();

        }
    }

    public class ResponseHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-timezone-offset",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Required = true
            });
        }
    }
}
