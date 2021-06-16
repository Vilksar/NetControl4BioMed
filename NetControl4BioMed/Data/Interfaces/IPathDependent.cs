using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a path.
    /// </summary>
    public interface IPathDependent
    {
        /// <summary>
        /// Represents the path ID of the model.
        /// </summary>
        string PathId { get; set; }

        /// <summary>
        /// Represents the path of the model.
        /// </summary>
        Path Path { get; set; }
    }
}
