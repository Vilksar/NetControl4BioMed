using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Seed
{
    /// <summary>
    /// Represents the seed data for the database types.
    /// </summary>
    public static class DatabaseTypes
    {
        /// <summary>
        /// Represents the database types to be seeded.
        /// </summary>
        public static ICollection<DatabaseType> Seed { get; } = new List<DatabaseType>
        {
            new DatabaseType
            {
                Id = "4d5ed537-070d-47fc-b018-43af2d2bf47f",
                DateTimeCreated = DateTime.UtcNow,
                Name = "Generic",
                Description = "Databases of this type contain generic nodes, that do not fall into any of the other types."
            },
            new DatabaseType
            {
                Id = "9c87fcad-4395-406a-80e4-faadbf6e4d14",
                DateTimeCreated = DateTime.UtcNow,
                Name = "PPI",
                Description = "Databases of this type contain proteins and protein-protein interactions."
            }
        };
    }
}
