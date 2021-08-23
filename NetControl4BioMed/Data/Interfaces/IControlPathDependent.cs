using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a control path.
    /// </summary>
    public interface IControlPathDependent
    {
        /// <summary>
        /// Represents the control path ID of the model.
        /// </summary>
        string ControlPathId { get; set; }

        /// <summary>
        /// Represents the control path of the model.
        /// </summary>
        ControlPath ControlPath { get; set; }
    }
}
