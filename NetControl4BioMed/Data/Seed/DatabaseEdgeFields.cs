using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Seed
{
    /// <summary>
    /// Represents the seed data for the database edge fields.
    /// </summary>
    public class DatabaseEdgeFields
    {
        /// <summary>
        /// Represents the database node fields to be seeded.
        /// </summary>
        public static ICollection<DatabaseEdgeField> Seed { get; } = new List<DatabaseEdgeField>
        {
            // The edge fields in the generic database.
            new DatabaseEdgeField
            {
                Id = "295790d1-b331-49ca-af2c-75358a82d6bd",
                DateTimeCreated = DateTime.Now,
                Name = "Generic Name",
                Description = "The display name of the edge.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "08dff7db-d065-454b-9e4d-afef087c2b05"
            },
            // The edge fields in the InnateDb database.
            new DatabaseEdgeField
            {
                Id = "ee1447fd-3a99-4a1d-b5c0-a55918def827",
                DateTimeCreated = DateTime.Now,
                Name = "InnateDB PubMed ID",
                Description = "The interaction PubMed ID in the InnateDB database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "e833d529-bceb-4cac-808e-22acf5a5bf0a"
            },
            new DatabaseEdgeField
            {
                Id = "5d58928c-b497-4f4b-b485-661b93a3e361",
                DateTimeCreated = DateTime.Now,
                Name = "InnateDB ID",
                Description = "The interaction unique identifier(s) in the InnateDB database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "e833d529-bceb-4cac-808e-22acf5a5bf0a"
            },
            // The edge fields in the KEGG database.
            new DatabaseEdgeField
            {
                Id = "41e55a3c-efe2-4970-be50-28048e71798c",
                DateTimeCreated = DateTime.Now,
                Name = "KEGG Type",
                Description = "The interaction type in the KEGG database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "8364ed3b-cf03-40d8-a28d-1405c365cdee"
            },
            // The edge fields in the OmniPath database.
            new DatabaseEdgeField
            {
                Id = "9284c1a3-1baa-43e9-920e-9decddcb42f0",
                DateTimeCreated = DateTime.Now,
                Name = "OmniPath Is Directed",
                Description = "Is the interaction directed in the Omnipath database?",
                IsSearchable = false,
                Url = null,
                DatabaseId = "4aafabfc-48d6-4332-b55d-c181c8ba6c54"
            },
            // The edge fields in the SIGNOR database.
            new DatabaseEdgeField
            {
                Id = "ffb308ae-aa14-445c-b9f1-f877257489a1",
                DateTimeCreated = DateTime.Now,
                Name = "SIGNOR PubMed ID",
                Description = "The interaction PubMed ID in the SIGNOR database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "4f316156-8795-4173-a1b2-5d164a3e36e6"
            },
            new DatabaseEdgeField
            {
                Id = "509b6fa6-ee0e-4bc0-bcbe-a98900a24970",
                DateTimeCreated = DateTime.Now,
                Name = "SIGNOR ID",
                Description = "The interaction unique identifier(s) in the SIGNOR database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "4f316156-8795-4173-a1b2-5d164a3e36e6"
            }
            // The edge fields in the STRING database.
        };
    }
}
