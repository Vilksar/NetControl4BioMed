using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a network.
    /// </summary>
    public interface INetworkDependent
    {
        /// <summary>
        /// Represents the network ID of the model.
        /// </summary>
        string NetworkId { get; set; }

        /// <summary>
        /// Represents the network of the model.
        /// </summary>
        Network Network { get; set; }
    }
}
