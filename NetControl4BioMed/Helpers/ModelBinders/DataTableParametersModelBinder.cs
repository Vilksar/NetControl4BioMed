using Microsoft.AspNetCore.Mvc.ModelBinding;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ModelBinders
{
    /// <summary>
    /// Represents the model binder for the DataTable parameters.
    /// </summary>
    public class DataTableParametersModelBinder : IModelBinder
    {
        /// <summary>
        /// Binds the DataTable parameter model.
        /// </summary>
        /// <param name="modelBindingContext">The model binding context.</param>
        /// <returns></returns>
        public Task BindModelAsync(ModelBindingContext modelBindingContext)
        {
            // Get the current request.
            var request = modelBindingContext.HttpContext.Request;
            // Get the required values.
            // Define the parameters.
            var parameters = new DataTableParametersViewModel
            {
                Draw = Convert.ToInt32(request.Query["draw"]),
                Start = Convert.ToInt32(request.Query["start"]),
                Length = Convert.ToInt32(request.Query["length"]),
                Search = new DataTableParametersViewModel.DataTableSearchViewModel
                {
                    Value = request.Query["search[value]"],
                    Regex = Convert.ToBoolean(request.Query["search[regex]"])
                },
                Order = new List<DataTableParametersViewModel.DataTableColumnOrderViewModel>(),
                Columns = new List<DataTableParametersViewModel.DataTableColumnViewModel>()
            };
            // Get the order values.
            var orderCount = 0;
            // Check whether the current count exists in the query.
            while (!string.IsNullOrEmpty(request.Query[$"order[{orderCount}][column]"]))
            {
                // Define a  new item and add it to the parameters.
                parameters.Order.Add(new DataTableParametersViewModel.DataTableColumnOrderViewModel
                {
                    Column = Convert.ToInt32(request.Query[$"order[{orderCount}][column]"]),
                    Direction = request.Query[$"order[{orderCount}][dir]"] == "asc" ? "Ascending" : "Descending"
                });
                // Increment the order count.
                orderCount++;
            }
            // Get the column values.
            var columnCount = 0;
            // Check whether the current count exists in the query.
            while (!string.IsNullOrEmpty(request.Query[$"columns[{columnCount}][name]"]))
            {
                // Define a  new item and add it to the parameters.
                parameters.Columns.Add(new DataTableParametersViewModel.DataTableColumnViewModel
                {
                    Data = request.Query[$"columns[{columnCount}][data]"],
                    Name = request.Query[$"columns[{columnCount}][name]"],
                    Orderable = Convert.ToBoolean(request.Query[$"columns[{columnCount}][orderable]"]),
                    Searchable = Convert.ToBoolean(request.Query[$"columns[{columnCount}][searchable]"]),
                    Search = new DataTableParametersViewModel.DataTableSearchViewModel
                    {
                        Value = request.Query[$"columns[{columnCount}][search][value]"],
                        Regex = Convert.ToBoolean(request.Query[$"columns[{columnCount}][search][regex]"])
                    }
                });
                // Increment the order count.
                columnCount++;
            }
            // Bind the parameters as successful.
            modelBindingContext.Result = ModelBindingResult.Success(parameters);
            // Return a completed task.
            return Task.CompletedTask;
        }
    }
}
